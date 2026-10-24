using System.Text.Json.Nodes;
using MeadowPeaks;

internal static class ComponentSwapTests
{
    public static void Run(Action<bool, string> check)
    {
        var rules = new TriPeaksRules();
        var originalDeals = new SeededDealProvider(new PeakLayouts(), rules);
        var originalHands = new SolitaireFactory(originalDeals, rules, originalDeals);
        var catalog = new PowerupCatalog(new IPowerup[] { new WildPowerup(), new ExtraCardsPowerup(), new RemovePowerup() });
        var clock = new TestClock();
        var random = new TestRandom();
        var replacementDeals = new TinyDeals();
        var replacementHands = new SolitaireFactory(replacementDeals, rules, originalDeals);
        var coins = new TestCoinGameFactory();
        var helper = new TestPowerup();
        var store = new MemoryProfileStore(new Progress { Coins = 30 });
        GameSession Open(IProfileStore storage, ISolitaireFactory factory, IEconomyPolicy? economy = null, PowerupCatalog? helpers = null) =>
            new(factory, coins, storage, economy ?? new TestEconomy(), helpers ?? new(new[] { helper }), clock, random);

        var session = Open(store, replacementHands);
        session.StartHand(1, false);
        check(replacementDeals.Calls == 1 && session.Hand!.Deal.Cards.Count == 1, "Injected deal provider supplies the active hand");
        check(session.UsePowerup("custom-sweep", 0) == CommandResult.MissingItem, "Custom helper needs its own inventory");
        check(session.BuyPowerup("unknown") == CommandResult.Unavailable && session.Wallet.Balance == 30, "Unknown item cannot spend coins");
        check(session.BuyPowerup("custom-sweep") == CommandResult.Applied && session.Wallet.Balance == 23, "Replacement helper price is used");
        check(session.Inventory.Count("custom-sweep") == 1, "New helper requires no profile property");
        check(session.UsePowerup("custom-sweep", -1) == CommandResult.Unavailable && session.Inventory.Count("custom-sweep") == 1, "Failed targeted helper is not consumed");
        check(session.UsePowerup("custom-sweep", 0) == CommandResult.Applied && session.Hand!.Won && helper.Applied == 1, "Injected helper strategy executes");
        var win = session.CompleteHand();
        check(win == new WinReward(2, 17) && session.Wallet.Balance == 40, "Replacement economy determines the payout");
        check(session.Profile.Unlocked == 2 && session.Profile.Stars[1] == 2 && session.Profile.Garden == 1, "Progression runs outside the UI");
        check(session.CompleteHand() == null && session.Wallet.Balance == 40, "Completion cannot pay twice");
        check(session.ClaimDaily() == 11 && session.ClaimDaily() == 0, "Injected daily gift and clock");
        clock.Today = clock.Today.AddDays(1);
        check(session.ClaimDaily() == 11, "Daily claim follows the injected clock");
        check(session.ClaimGarden() == 13 && session.ClaimGarden() == 0, "Replacement garden interval and reward");
        session.Replay();
        check(!session.Hand!.Won && session.Hand.Deal.Cards.Count == 1 && session.Hand.FreeUndos == 3, "Replay uses saved board and resets hand state");

        var waitingHand = session.Hand;
        session.OpenCoinGame(); session.OpenCoinGame();
        check(coins.Calls == 1, "Opening the coin game preserves its round");
        check(session.PlayGems(0, 1) && session.Wallet.Balance == 84, "Replacement coin game credits shared wallet");
        check(!session.PlayGems(0, 1) && session.Wallet.Balance == 84, "Rejected coin move cannot pay stale gain again");
        check(ReferenceEquals(session.Hand, waitingHand), "Coin-game replacement keeps the solitaire hand operational");
        session.NewCoinGame();
        check(coins.Calls == 2 && random.Calls > 0, "Round creation uses injected factory and randomness");
        session.Save();
        var reopened = Open(store, new SolitaireFactory(new TinyDeals(7), rules, originalDeals));
        check(reopened.Hand!.Deal.Cards[0].Rank == 2 && reopened.Wallet.Balance == 84 && reopened.Inventory.Count("custom-sweep") == 0, "Saved board and custom inventory survive replacing the deal provider");
        reopened.StartHand(2, false);
        check(reopened.Hand!.Deal.Cards[0].Rank == 7, "Replacement provider is used for new hands");

        var permissiveRules = new AnyRankRules();
        var alternate = new Hand(new TinyDeals(7).Generate(0, 1), permissiveRules);
        check(alternate.Play(0) && permissiveRules.Calls > 0, "Hand uses replacement rules, not static default rules");
        var tinyLayouts = new TinyLayouts();
        var tinyGenerated = new SeededDealProvider(tinyLayouts, rules).Generate(5, 100);
        check(tinyLayouts.Calls == 1 && tinyGenerated.Cards.Count == 1 && tinyGenerated.Stock.Count == 50, "Layout component can be replaced independently");

        var poor = Open(new MemoryProfileStore(new Progress { Coins = 0 }), originalHands, helpers: catalog);
        poor.StartHand(1, false);
        check(poor.BuyPowerup("wild") == CommandResult.InsufficientCoins && poor.Inventory.Count("wild") == 2, "Failed purchase cannot grant inventory");
        for (int i = 0; i < 3; i++) { poor.Draw(); check(poor.Undo() == CommandResult.Applied, "Free undo through coordinator"); }
        poor.Draw();
        int stockBefore = poor.Hand!.Stock.Count;
        check(poor.Undo() == CommandResult.InsufficientCoins && poor.Hand.Stock.Count == stockBefore, "Paid undo checks funds before changing the board");

        string directory = Path.Combine(Path.GetTempPath(), "MeadowPeaks-components-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "progress.json");
            var fileStore = new JsonProfileStore(path);
            session.Save();
            fileStore.Save(session.Profile);
            var fromDisk = Open(fileStore, replacementHands);
            check(fromDisk.Profile.GardenClaimed == 1 && fromDisk.Profile.Items.ContainsKey("custom-sweep"), "Storage accepts a replacement economy and additional item IDs");

            var oldHand = originalHands.Create(105726, 1);
            oldHand.Play(oldHand.Moves.First());
            var oldProfile = new Progress { Active = oldHand.Capture(105726, 1, false) };
            fileStore.Save(oldProfile);
            var json = JsonNode.Parse(File.ReadAllText(path))!;
            json["Active"]!.AsObject().Remove("DealtBoard");
            json.AsObject().Remove("Items");
            File.WriteAllText(path, json.ToJsonString());
            var migrated = Open(fileStore, replacementHands);
            check(migrated.Hand != null && migrated.Hand.Deal.Cards.SequenceEqual(oldHand.Deal.Cards) && migrated.Hand.Removed.SequenceEqual(oldHand.Removed), "Original version-1 save restores using the legacy generator after a provider swap");
            migrated.Replay();
            check(migrated.Hand!.Deal.Cards.SequenceEqual(oldHand.Deal.Cards) && migrated.Hand.Removed.All(x => !x), "Legacy deal can still be replayed after a provider swap");
        }
        finally { Directory.Delete(directory, true); }
        Console.WriteLine("Component replacement tests passed: providers, layouts, rules, rewards, helpers, coin games, stores, clock, randomness, and legacy saves.");
    }

    sealed class TinyDeals : IDealProvider
    {
        readonly int rank;
        public int Calls { get; private set; }
        public TinyDeals(int rank = 2) { this.rank = rank; }
        public Deal Generate(int seed, int level)
        {
            Calls++;
            return new() { Layout = new[] { new Slot(4, 0) }, Cards = new[] { new Card(rank, 0) }, Stock = new[] { new Card(3, 0) }, Waste = new(1, 0), Solution = new[] { new Step(0) } };
        }
    }
    sealed class TinyLayouts : ILayoutProvider
    {
        public int Calls { get; private set; }
        public List<Slot> Create(int level) { Calls++; return new() { new(4, 0) }; }
    }
    sealed class AnyRankRules : ISolitaireRules
    {
        public int Calls { get; private set; }
        public bool Adjacent(int a, int b) { Calls++; return true; }
        public bool Exposed(IReadOnlyList<Slot> layout, IReadOnlyList<bool> removed, int index) => !removed[index];
    }
    sealed class TestEconomy : IEconomyPolicy
    {
        public int UndoCost => 7;
        public int DailyGift => 11;
        public int GardenGift => 13;
        public int GardenInterval => 1;
        public WinReward Reward(WinContext context) => new(2, 17);
    }
    sealed class TestPowerup : IPowerup
    {
        public string Id => "custom-sweep";
        public string Name => "Sweep";
        public string Description => "A replacement targeted helper.";
        public int Price => 7;
        public bool RequiresTarget => true;
        public int Applied { get; private set; }
        public bool CanUse(ISolitaireHand hand) => !hand.Won;
        public bool Apply(ISolitaireHand hand, int? target, int seed)
        {
            if (!target.HasValue || !hand.Remove(target.Value)) return false;
            Applied++; return true;
        }
    }
    sealed class TestClock : IGameClock { public DateTime Today { get; set; } = new(2026, 10, 24); }
    sealed class TestRandom : IRandomSource { public int Calls { get; private set; } public int NextSeed() => ++Calls; }
    sealed class TestCoinGameFactory : ICoinGameFactory
    {
        public int Calls { get; private set; }
        public ICoinGame Create(int seed) { Calls++; return new TestCoinGame(); }
    }
    sealed class TestCoinGame : ICoinGame
    {
        public IReadOnlyList<int> Cells { get; } = Array.AsReadOnly(new int[64]);
        public int Moves { get; private set; } = 20;
        public int Earned { get; private set; }
        public int LastGain { get; private set; }
        public (int, int)? Hint() => (0, 1);
        public bool Play(int first, int second)
        {
            if (Moves != 20 || first != 0 || second != 1) return false;
            Moves--; LastGain = 9; Earned = 9; return true;
        }
    }
}
