using MeadowPeaks;
using System.Text.Json;

static class TemporaryTests
{
    // Exercise timed helper expiry, cooldowns and undo/restoration without relying on wall-clock delays.
    public static void Run(Action<bool,string> check)
    {
        var rules = new TriPeaksRules();
        // Build a small purpose-specific deal fixture so the test isolates the intended mechanics.
        Deal Board(params Card[] cards) => new() { TemporaryRules = true, Cards = cards, Layout = cards.Select((_,i) => new Slot(i*2,0)).ToArray(), Waste = new(5,0), Stock = new[] { new Card(8,1), new Card(9,1) } };
        var plain = Board(new(6,0),new(7,0),new(8,0));
        var drawTimer=new Hand(plain,rules);
        check(drawTimer.Draw() && drawTimer.Effects.DrawCooldown == 0,"Stock draws remain immediate");
        check(drawTimer.Undo() && drawTimer.Draw(),"Drawing after undo stays immediate");
        var timerSave=JsonSerializer.Deserialize<SavedRun>(JsonSerializer.Serialize(drawTimer.Capture(1,1,false)))!;
        var timerFactory=new SolitaireFactory(new StaticDeals(plain),rules,new StaticDeals(plain));
        var timerRestore=(Hand)timerFactory.Restore(timerSave)!;
        check(timerRestore.Draw(),"Drawing stays immediate after save restore");
        timerRestore.AdvanceTime(2);check(!timerRestore.Draw(),"Empty stock cannot be drawn after advancing time");
        var h = new Hand(plain,rules);
        check(h.RequestHint(), "Hint starts briefly"); h.AdvanceTime(4);
        check(h.Effects.HintSeconds == 0 && !h.RequestHint(), "Hint expires and cannot bypass cooldown");
        h.AdvanceTime(11); check(h.RequestHint(), "Hint recharges");
        h.AdvanceTime(100000); check(!h.Failed && h.CanPlay(0), "There is no level countdown");
        check(h.UseEffect("lucky") && h.Play(0) && h.Stock.Count == 3, "Temporary +1 adds one stock per clear");
        h.AdvanceTime(11); check(h.Play(1) && h.Stock.Count == 3, "+1 stops at expiry");
        check(h.Undo() && h.Effects.LuckySeconds == 0, "Undo does not restore expired effect duration");
        var rescue = new Hand(new Deal {TemporaryRules=true, Cards=plain.Cards, Layout=plain.Layout, Waste=plain.Waste},rules);
        check(rescue.Extra(42) && rescue.Stock.Count==5 && rescue.Stock.All(c=>!c.TemporaryStock), "Fresh rescue cards remain until used");
        check(rescue.Draw() && rescue.Stock.Count==4, "Fresh rescue card can be drawn");
        rescue.AdvanceTime(11);
        check(rescue.Stock.Count==4 && rescue.Effects.FreshSeconds==0, "Unused rescue cards do not expire");
        check(rescue.Undo() && rescue.Stock.Count==5, "Undo restores a drawn rescue card");
        h = new Hand(Board(new Card(6,0) { Kind=CardKind.Ice, Strength=99 },new(7,0)),rules);
        check(!h.CanPlay(0) && h.UseEffect("thaw") && h.CanPlay(0), "Temporary thaw bypasses ice");
        h.AdvanceTime(9); check(!h.CanPlay(0), "Ice returns when thaw expires");
        h = new Hand(Board(new Card(6,0) { Kind=CardKind.Bomb, Strength=1 },new(7,0)),rules);
        check(h.UseEffect("shield") && h.Draw() && !h.Failed && h.Effects.FuseDelay==1, "Shield pauses bomb fuse on a move");
        h.AdvanceTime(11); check(h.Draw() && h.Failed, "Fuses resume when shield expires");
        check(h.Undo() && !h.Failed && h.Effects.ShieldSeconds==0, "Undo rescues a bomb without restoring shield duration");
        h = new Hand(Board(new Card(6,0) { Kind=CardKind.Stone },new(7,0)),rules);
        check(h.Play(0) && !h.Removed[0] && h.Waste.Rank==5 && h.Score==0, "Armor takes a hit without clearing the card");
        check(h.Undo() && h.Effects.Peeled==0 && h.Play(0) && h.Play(0) && h.Removed[0], "Armor can be undone and broken");
        h = new Hand(Board(new(6,0),new(7,0),new Card(8,0) { Kind=CardKind.Vine }),rules);
        check(h.Play(0) && h.Play(1) && h.CanPlay(2), "Vines open after two uninterrupted clears");
        check(h.Draw() && h.Effects.Run==0 && !h.CanPlay(2), "Drawing closes vines again");
        h = new Hand(plain,rules);
        check(h.Wild(), "Temporary wild starts"); h.AdvanceTime(13);
        check(h.Waste.Rank==0, "Unused wild stays armed without a time limit");
        check(!h.Wild() && h.Play(0), "An armed Wild cannot be stacked and is consumed by a match"); h.AdvanceTime(13);
        check(h.Undo() && h.Waste.Rank==0, "Undo restores the armed Wild");
        h.UseEffect("lucky"); h.AdvanceTime(3); h.StartHelperCooldown();
        var saved = JsonSerializer.Deserialize<SavedRun>(JsonSerializer.Serialize(h.Capture(1,1,false)))!;
        var factory = new SolitaireFactory(new StaticDeals(plain),rules,new StaticDeals(plain));
        var restored = (Hand)factory.Restore(saved)!;
        check(restored.Effects.LuckySeconds==7 && restored.Effects.HelperCooldown==5, "Effect durations persist through JSON save");
        restored.AdvanceTime(double.NaN); check(restored.Effects.LuckySeconds==7, "Invalid time delta is ignored");
        var catalog = new PowerupCatalog(new IPowerup[] { new WildPowerup(), new TemporaryPowerup("lucky","+1","10s",85) });
        var bonus = new Deal { TemporaryRules=true, Bonus=true, Cards=new[] { new Card(6,0) }, Layout=new[] { new Slot(1,0) }, Waste=new(5,0) };
        var profile = new Progress { Unlocked=25, Coins=1000 }; profile.Items["lucky"]=2; profile.SeenMechanics.Add(CardKind.Lucky);
        var session = new GameSession(new SolitaireFactory(new StaticDeals(bonus),rules,new StaticDeals(bonus)),new GemGameFactory(),new MemoryProfileStore(profile),new StandardEconomyPolicy(),catalog,new LocalGameClock(),new Seeds());
        session.StartHand(25,false);
        check(session.UsePowerup("lucky")==CommandResult.Applied && session.UsePowerup("wild")==CommandResult.Unavailable, "Helpers share a temporary recharge");
        int stock = session.Inventory.Count("lucky");
        ((ITemporaryHand)session.Hand!).AdvanceTime(6); session.PlayCard(0); var win=session.CompleteHand();
        check(win?.BonusCoins==125 && session.Inventory.Count("lucky")==stock+1, "Bonus level pays coins and a consumable");
        session.Replay(); check(((ITemporaryHand)session.Hand!).Effects.LuckySeconds==0, "Replay resets temporary boosts");
        session.PlayCard(0); var replay=session.CompleteHand();
        check(replay?.BonusCoins==0 && session.Inventory.Count("lucky")==stock+1, "Bonus consumable cannot be farmed by replay");
        session.Remix(); check(session.Level==25 && session.Seed==42 && !session.Hand!.Won, "Remix deals another seeded hand at the same level");
        check(session.Profile.FreshDealCooldown==30,"Fresh deals begin a thirty second recharge");
        session.AdvanceTime(31);check(session.Profile.FreshDealCooldown==0,"Fresh deal cooldown elapses with play time");
        string folder = Path.Combine(Path.GetTempPath(), "MeadowPeaks-large-wallet-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            var large = new JsonProfileStore(Path.Combine(folder,"progress.json"));
            large.Save(new Progress { Coins = 200000000 });
            check(large.Load(out var warning).Coins==200000000 && warning=="", "Long campaign wallet is not rejected at 100 million coins");
        }
        finally { Directory.Delete(folder,true); }
        Console.WriteLine("Temporary effects, armor, vines, bonus rewards, cooldowns and remix tests passed.");
    }
    sealed class StaticDeals : IDealProvider
    {
        readonly Deal deal; /* Retain the exact supplied deal for targeted rule and restoration checks. */
    public StaticDeals(Deal deal) => this.deal=deal;
        // Return the supplied fixture instead of introducing unrelated generation randomness.
        public Deal Generate(int seed,int level) => deal;
    }
    sealed class Seeds : IRandomSource { /* Supply a seed through the injected random source so game logic need not own global randomness. */
    public int NextSeed() => 42; }
}
