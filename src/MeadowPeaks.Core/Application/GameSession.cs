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
    public ICampaignCatalog Campaign { get; }
    public PicnicTrail Picnic { get; }
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
        IGameClock clock, IRandomSource random, ICampaignCatalog? campaign = null, IPicnicPolicy? picnicPolicy = null)
    {
        if (economy.GardenInterval <= 0 || economy.UndoCost < 0 || economy.DailyGift < 0 || economy.GardenGift < 0 || economy.ChapterGift < 0 || economy.ChapterWilds < 0)
            throw new ArgumentException("Invalid economy policy.", nameof(economy));
        this.solitaire = solitaire;
        this.coinGames = coinGames;
        this.store = store;
        this.clock = clock;
        this.random = random;
        Economy = economy;
        Powerups = powerups;
        Campaign = campaign ?? new MeadowCampaign();
        Profile = store.Load(out var warning);
        // A profile that finished the old campaign can continue into new chapters.
        while (Profile.Unlocked < Campaign.LevelCount && Profile.Stars.ContainsKey(Profile.Unlocked)) Profile.Unlocked++;
        LoadWarning = warning;
        Wallet = new(Profile);
        Inventory = new(Profile);
        Picnic = new(Profile,clock,Wallet,Inventory,Powerups,picnicPolicy);
        if(Profile.ActiveGarden is {} garden && coinGames is IPersistentCoinGameFactory persistent)CoinGame=persistent.Restore(garden);
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
        DiscoverHand();
    }
    void DiscoverHand(){if(Hand==null)return;Profile.HighestPlayedLevel=Math.Max(Profile.HighestPlayedLevel,Level);foreach(var card in Hand.Deal.Cards)Profile.SeenMechanics.Add(card.Kind);}
    public bool HelperDiscovered(string id)=>HelperDiscovery.Discovered(Profile,id);
    public bool HelperRelevant(string id)=>HelperDiscovery.Relevant(Hand,id);

    public void StartHand(int level, bool endless)
    {
        if (level < 1 || (!endless && (level > Campaign.LevelCount || level > Profile.Unlocked)))
            throw new ArgumentOutOfRangeException(nameof(level));
        int seed = endless ? (int)((170003L + level * 7919L) % int.MaxValue) : 104729 + level * 997;
        var next = solitaire.Create(seed, level); // Build successfully before replacing an active hand.
        Hand = next;
        Level = level;
        Seed = seed;
        Endless = endless;
        DiscoverHand();
        if (endless) Profile.EndlessDeal = Math.Min(int.MaxValue - 1, level + 1);
    }

    public void Replay()
    {
        if (Hand == null) return;
        // Replay the stored board, not whichever provider happens to be installed now.
        var run = Hand.Capture(Seed, Level, Endless);
        var fresh = run with { Removed = new bool[Hand.Removed.Count], Stock = Hand.Deal.Stock.ToList(), Waste = Hand.Deal.Waste, Score = 0, Streak = 0, BestStreak = 0, FreeUndos = 3, Paid = false, Mechanics = null };
        Hand = solitaire.Restore(fresh) ?? throw new InvalidOperationException("Unable to replay this deal.");
    }

    public void Remix()
    {
        if (Profile.FreshDealCooldown>0 || Hand == null) return;
        int nextSeed = random.NextSeed();
        var next = solitaire.Create(nextSeed, Level);
        Hand = next; Seed = nextSeed; Profile.FreshDealCooldown=30;
    }

    public void AdvanceTime(double seconds)
    {
        if(!double.IsFinite(seconds) || seconds<=0) return;
        Profile.FreshDealCooldown=Math.Max(0,Profile.FreshDealCooldown-seconds);
        (Hand as ITemporaryHand)?.AdvanceTime(seconds);
    }
    public bool ToggleFavorite(int level)
    {
        if(level<1 || level>Profile.Unlocked) return false;
        if(!Profile.Favorites.Add(level)) Profile.Favorites.Remove(level);
        return true;
    }
    public string? ShareCode => Hand==null || Hand.Deal.Generation is not ("MP16" or "MP18") ? null : DealCodes.Encode(new(Level,Seed,Endless,Hand.Deal.Generation));
    public bool StartSharedDeal(string code)
    {
        if(!DealCodes.TryParse(code,out var reference) || reference==null || !reference.Endless && reference.Level>Profile.Unlocked) return false;
        var next=solitaire is IVersionedSolitaireFactory versioned ? versioned.CreateVersion(reference.Seed,reference.Level,reference.Generation) : solitaire.Create(reference.Seed,reference.Level);
        Hand=next; Level=reference.Level; Seed=reference.Seed; Endless=reference.Endless;DiscoverHand();
        if(Endless) Profile.EndlessDeal=Math.Max(Profile.EndlessDeal,Math.Min(int.MaxValue-1,Level+1));
        return true;
    }
    public bool PlayCard(int index)
    {
        if(Hand==null) return false;
        int before=Hand.Removed.Count(x=>x);
        if(!Hand.Play(index)) return false;
        Picnic.Clear(Hand.Removed.Count(x=>x)-before); return true;
    }
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
        if (Hand == null || powerup == null || !HelperDiscovered(id) || !HelperRelevant(id) || (Hand is ITemporaryHand timed && timed.Effects.HelperCooldown > 0) || !powerup.CanUse(Hand)) return CommandResult.Unavailable;
        if (Inventory.Count(id) == 0) return CommandResult.MissingItem;
        int before=Hand.Removed.Count(x=>x);
        if (!powerup.Apply(Hand, target, random.NextSeed())) return CommandResult.Unavailable;
        Picnic.Clear(Hand.Removed.Count(x=>x)-before);
        Inventory.Add(id, -1);
        (Hand as ITemporaryHand)?.StartHelperCooldown();
        return CommandResult.Applied;
    }

    public CommandResult BuyPowerup(string id)
    {
        var powerup = Powerups.Find(id);
        if (powerup == null || !HelperDiscovered(id)) return CommandResult.Unavailable;
        if (!Wallet.Spend(powerup.Price)) return CommandResult.InsufficientCoins;
        Inventory.Add(id, 1);
        return CommandResult.Applied;
    }

    public WinReward? CompleteHand()
    {
        if (Hand == null || !Hand.Won || Hand.Paid) return null;
        var result = Economy.Reward(new(Hand.BestStreak, Hand.Stock.Count, !Endless && Profile.Stars.ContainsKey(Level)));
        if (result.Coins < 0 || result.Stars < 1 || result.Stars > 3) throw new InvalidOperationException("Invalid win reward.");
        bool bonus = !Endless && Hand.Deal.Bonus && !Profile.Stars.ContainsKey(Level);
        if (bonus)
        {
            var discovered=Powerups.All.Where(p=>HelperDiscovered(p.Id)).ToArray();
            string item = discovered[(Level / 25) % discovered.Length].Id;
            Inventory.Add(item, 1);
            result = result with { Coins = checked(result.Coins + 125), BonusCoins = 125, BonusItem = item };
        }
        Chapter chapter = Campaign.ForLevel(Level);
        bool chapterComplete = !Endless && !Profile.ChapterGifts.Contains(chapter.Number) && Enumerable.Range(chapter.FirstLevel, chapter.LastLevel - chapter.FirstLevel + 1).All(n => n == Level || Profile.Stars.ContainsKey(n));
        if (chapterComplete) result = result with { Coins = checked(result.Coins + Economy.ChapterGift), ChapterBonus = Economy.ChapterGift, ChapterName = chapter.World + " · " + chapter.Name };
        Wallet.Credit(result.Coins);
        Hand.Paid = true;
        Profile.Garden++;
        Picnic.Win();
        if (Endless) Profile.EndlessWins++;
        else
        {
            Profile.Stars[Level] = Math.Max(Profile.Stars.GetValueOrDefault(Level), result.Stars);
            Profile.Scores[Level] = Math.Max(Profile.Scores.GetValueOrDefault(Level), Hand.Score);
            Profile.Unlocked = Math.Min(Campaign.LevelCount, Math.Max(Profile.Unlocked, Level + 1));
        }
        if (chapterComplete) { Profile.ChapterGifts.Add(chapter.Number); Inventory.Add("wild", Economy.ChapterWilds); }
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

    public const int GardenRefreshSeconds = 3600;
    public double GardenRefreshRemaining => Math.Max(0, (Profile.GardenRefreshAt-clock.UtcNow.ToUnixTimeMilliseconds())/1000.0);
    public void OpenCoinGame()
    {
        if(CoinGame!=null) return;
        CoinGame=coinGames.Create(random.NextSeed());
        if(GardenRefreshRemaining<=0) Profile.GardenRefreshAt=clock.UtcNow.ToUnixTimeMilliseconds()+GardenRefreshSeconds*1000;
        Save();
    }
    public bool NewCoinGame()
    {
        if(GardenRefreshRemaining>0) return false;
        var next=coinGames.Create(random.NextSeed());
        CoinGame=next;
        Profile.GardenRefreshAt=clock.UtcNow.ToUnixTimeMilliseconds()+GardenRefreshSeconds*1000;
        Save();return true;
    }
    public bool PlayGems(int first, int second)
    {
        if (CoinGame?.Play(first, second) != true) return false;
        Wallet.Credit(CoinGame.LastGain);
        Picnic.Gems(CoinGame.LastGain);
        return true;
    }

    public void Save()
    {
        Profile.Active = Hand?.Capture(Seed, Level, Endless);
        Profile.ActiveGarden=(CoinGame as IPersistentCoinGame)?.Capture();
        store.Save(Profile);
    }
}
