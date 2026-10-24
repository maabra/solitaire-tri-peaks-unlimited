namespace MeadowPeaks;

// Application coordinator: the UI issues commands here, never changes coins or
// constructs concrete game engines. All replaceable parts enter through the constructor.
public sealed class GameSession
{
    readonly ISolitaireFactory solitaire;
    readonly ICoinGameFactory coinGames;
    readonly IProfileStore store;
    readonly IGameClock clock;
    readonly IRandomSource random;
    public IEconomyPolicy Economy { get; }
    public PowerupCatalog Powerups { get; }
    public Progress Profile { get; }
    public Wallet Wallet { get; }
    public PowerupInventory Inventory { get; }
    public ISolitaireHand? Hand { get; private set; }
    public ICoinGame? CoinGame { get; private set; }
    public int Level { get; private set; } = 1;
    public int Seed { get; private set; }
    public bool Endless { get; private set; }
    public string LoadWarning { get; private set; }
    public bool DailyClaimed => Profile.Daily == clock.Today.ToString("yyyy-MM-dd");
    public int GardenGifts => Math.Max(0, Profile.Garden / Economy.GardenInterval - Profile.GardenClaimed);

    public GameSession(ISolitaireFactory solitaire, ICoinGameFactory coinGames,
        IProfileStore store, IEconomyPolicy economy, PowerupCatalog powerups,
        IGameClock clock, IRandomSource random)
    {
        if (economy.GardenInterval <= 0 || economy.UndoCost < 0 || economy.DailyGift < 0 || economy.GardenGift < 0)
            throw new ArgumentException("Invalid economy policy.", nameof(economy));
        this.solitaire = solitaire;
        this.coinGames = coinGames;
        this.store = store;
        this.clock = clock;
        this.random = random;
        Economy = economy;
        Powerups = powerups;
        Profile = store.Load(out var warning);
        LoadWarning = warning;
        Wallet = new(Profile);
        Inventory = new(Profile);
        if (Profile.Active != null)
        {
            try { Hand = solitaire.Restore(Profile.Active); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { Hand = null; }
            if (Hand != null)
            {
                Level = Profile.Active.Level;
                Seed = Profile.Active.Seed;
                Endless = Profile.Active.Endless;
            }
            else LoadWarning = "The saved hand could not be restored. Your wallet and adventure progress are available.";
        }
    }

    public void StartHand(int level, bool endless)
    {
        if (level < 1 || (!endless && (level > 2000 || level > Profile.Unlocked)))
            throw new ArgumentOutOfRangeException(nameof(level));
        int seed = endless ? (int)((170003L + level * 7919L) % int.MaxValue) : 104729 + level * 997;
        var next = solitaire.Create(seed, level); // Build successfully before replacing an active hand.
        Hand = next;
        Level = level;
        Seed = seed;
        Endless = endless;
        if (endless) Profile.EndlessDeal = Math.Min(int.MaxValue - 1, level + 1);
    }

    public void Replay()
    {
        if (Hand == null) return;
        // Replay the stored board, not whichever provider happens to be installed now.
        var run = Hand.Capture(Seed, Level, Endless);
        var fresh = run with { Removed = new bool[Hand.Removed.Count], Stock = Hand.Deal.Stock.ToList(), Waste = Hand.Deal.Waste, Score = 0, Streak = 0, BestStreak = 0, FreeUndos = 3, Paid = false };
        Hand = solitaire.Restore(fresh) ?? throw new InvalidOperationException("Unable to replay this deal.");
    }

    public bool PlayCard(int index) => Hand?.Play(index) == true;
    public bool Draw() => Hand?.Draw() == true;

    public CommandResult Undo()
    {
        if (Hand == null || !Hand.CanUndo) return CommandResult.Unavailable;
        int cost = Hand.FreeUndos > 0 ? 0 : Economy.UndoCost;
        if (Wallet.Balance < cost) return CommandResult.InsufficientCoins;
        if (!Hand.Undo()) return CommandResult.Unavailable;
        Wallet.Spend(cost);
        return CommandResult.Applied;
    }

    public CommandResult UsePowerup(string id, int? target = null)
    {
        var powerup = Powerups.Find(id);
        if (Hand == null || powerup == null || !powerup.CanUse(Hand)) return CommandResult.Unavailable;
        if (Inventory.Count(id) == 0) return CommandResult.MissingItem;
        if (!powerup.Apply(Hand, target, random.NextSeed())) return CommandResult.Unavailable;
        Inventory.Add(id, -1);
        return CommandResult.Applied;
    }

    public CommandResult BuyPowerup(string id)
    {
        var powerup = Powerups.Find(id);
        if (powerup == null) return CommandResult.Unavailable;
        if (!Wallet.Spend(powerup.Price)) return CommandResult.InsufficientCoins;
        Inventory.Add(id, 1);
        return CommandResult.Applied;
    }

    public WinReward? CompleteHand()
    {
        if (Hand == null || !Hand.Won || Hand.Paid) return null;
        var result = Economy.Reward(new(Hand.BestStreak, Hand.Stock.Count, !Endless && Profile.Stars.ContainsKey(Level)));
        if (result.Coins < 0 || result.Stars < 1 || result.Stars > 3) throw new InvalidOperationException("Invalid win reward.");
        Wallet.Credit(result.Coins);
        Hand.Paid = true;
        Profile.Garden++;
        if (Endless) Profile.EndlessWins++;
        else
        {
            Profile.Stars[Level] = Math.Max(Profile.Stars.GetValueOrDefault(Level), result.Stars);
            Profile.Scores[Level] = Math.Max(Profile.Scores.GetValueOrDefault(Level), Hand.Score);
            Profile.Unlocked = Math.Min(2000, Math.Max(Profile.Unlocked, Level + 1));
        }
        return result;
    }

    public int ClaimDaily()
    {
        if (DailyClaimed) return 0;
        Wallet.Credit(Economy.DailyGift);
        Profile.Daily = clock.Today.ToString("yyyy-MM-dd");
        return Economy.DailyGift;
    }

    public int ClaimGarden()
    {
        int gifts = Math.Max(0, GardenGifts);
        int coins = checked(gifts * Economy.GardenGift);
        Wallet.Credit(coins);
        Profile.GardenClaimed += gifts;
        return coins;
    }

    public void OpenCoinGame() => CoinGame ??= coinGames.Create(random.NextSeed());
    public void NewCoinGame() => CoinGame = coinGames.Create(random.NextSeed());
    public bool PlayGems(int first, int second)
    {
        if (CoinGame?.Play(first, second) != true) return false;
        Wallet.Credit(CoinGame.LastGain);
        return true;
    }

    public void Save()
    {
        Profile.Active = Hand?.Capture(Seed, Level, Endless);
        store.Save(Profile);
    }
}
