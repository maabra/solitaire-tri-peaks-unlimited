using MeadowPeaks;
using System.Text.Json;

static class ExpansionTests
{
    public static void Run(Action<bool,string> check)
    {
        var rules=new TriPeaksRules();
        Deal Board(params Card[] cards) => new() { TemporaryRules=true, Generation="MP16", Layout=cards.Select((_,i)=>new Slot(i*2,0)).ToArray(), Cards=cards, Waste=new(5,0), Stock=new[] {new Card(7,1),new Card(8,0)} };
        var h=new Hand(Board(new Card(6,0){Kind=CardKind.Gate,Strength=1},new(6,1)),rules);
        check(!h.CanPlay(0) && TimedMoves.Draw(h) && h.CanPlay(0), "Successful draws flip phase gates");
        check(h.Undo() && !h.CanPlay(0) && h.UseEffect("phase") && h.CanPlay(0), "Phase bypass temporarily opens a closed gate");
        h.AdvanceTime(7);check(!h.CanPlay(0), "Phase bypass expires");
        h=new Hand(Board(new Card(6,0){Kind=CardKind.ColorSeal,Strength=1},new(6,1)),rules);
        check(!h.CanPlay(0) && TimedMoves.Draw(h) && h.CanPlay(0), "Red seal requires red waste as well as adjacent rank");
        check(h.Undo() && h.Wild() && h.CanPlay(0), "Wild bypasses a color seal");
        h=new Hand(Board(new Card(6,0){Kind=CardKind.Lock,Strength=3},new(7,0)),rules);
        check(!h.CanPlay(0)&&h.UseEffect("keypass")&&h.CanPlay(0), "Temporary keypass bypasses reinforced locks");
        h.AdvanceTime(7);check(!h.CanPlay(0), "Keypass expires without permanently granting keys");
        h=new Hand(Board(new Card(6,0){Kind=CardKind.Vine,Strength=3},new(7,0)),rules);
        check(h.UseEffect("prune")&&h.CanPlay(0), "Pruning bypasses vine streak requirement");
        h.AdvanceTime(9);check(!h.CanPlay(0), "Vines return after pruning expires");
        h=new Hand(Board(new(6,0),new(7,0)),rules);check(h.UseEffect("peek")&&h.Effects.PeekSeconds==6, "Stock preview starts for six seconds");
        var snapshot=JsonSerializer.Deserialize<SavedRun>(JsonSerializer.Serialize(h.Capture(1,2300,false)))!;
        var provider=new AdventureDealProvider(new SeededDealProvider(new AdventureLayouts(),rules));
        var factory=new SolitaireFactory(provider,rules,provider);
        var loaded=(Hand)factory.Restore(snapshot)!;loaded.AdvanceTime(7);
        check(loaded.Effects.PeekSeconds==0, "Preview duration persists and expires after restore");
        foreach(var address in new[] {new DealReference(1,0,false),new DealReference(2300,int.MaxValue,false),new DealReference(900000,98765,true)})
        {
            string code=DealCodes.Encode(address);
            check(DealCodes.TryParse(code,out var parsed)&&parsed==address, "Deal code round trip");
            check(!DealCodes.TryParse(code+"X",out _)&&!DealCodes.TryParse(code.Replace("MP16","MP15"),out _), "Corrupt and incompatible codes are rejected");
        }
        check(!DealCodes.TryParse(null,out _)&&!DealCodes.TryParse(new string('A',1000),out _), "Invalid code input is bounded");
        var profile=new Progress {Unlocked=2300};var clock=new Clock();
        var catalog=new PowerupCatalog(new IPowerup[]{new WildPowerup(),new TemporaryPowerup("peek","eye","6s",40)});
        var store=new MemoryProfileStore(profile);
        GameSession Open(IProfileStore save) => new(factory,new GemGameFactory(),save,new StandardEconomyPolicy(),catalog,clock,new Seeds());
        var session=Open(store);profile=session.Profile;session.StartHand(2300,false);string share=session.ShareCode!;
        var original=session.Hand!.Deal;
        check(session.StartSharedDeal(share)&&session.Hand!.Deal.Cards.SequenceEqual(original.Cards)&&session.Hand.Deal.Layout.SequenceEqual(original.Layout), "Shared deal reproduces cards and geometry");
        check(!session.StartSharedDeal(DealCodes.Encode(new(2301,42,false)))&&session.Level==2300, "Sharing cannot unlock campaign levels");
        check(session.ToggleFavorite(200)&&profile.Favorites.Contains(200)&&session.ToggleFavorite(200)&&!profile.Favorites.Contains(200), "Favorites toggle without affecting progression");
        check(!session.ToggleFavorite(2301), "Locked levels cannot be bookmarked by command");
        session.ToggleFavorite(200);session.Save();check(Open(store).Profile.Favorites.Contains(200), "Favorites survive save/reopen");
        var picnic=session.Picnic;int wallet=session.Wallet.Balance;
        check(picnic.Claim("wins")==0 && picnic.Claim("unknown")==0, "Unfinished daily goals cannot pay");
        picnic.Win();picnic.Win();picnic.Clear(24);picnic.Gems(80);
        int inventory=catalog.All.Sum(p=>session.Inventory.Count(p.Id));
        check(picnic.Claim("wins")==40&&picnic.Claim("wins")==0&&picnic.Claim("clears")==50&&picnic.Claim("gems")==60, "Each daily goal pays exactly once");
        check(session.Wallet.Balance==wallet+150&&catalog.All.Sum(p=>session.Inventory.Count(p.Id))==inventory+1, "Finishing picnic grants exactly one consumable");
        check(picnic.Claim("gems")==0&&catalog.All.Sum(p=>session.Inventory.Count(p.Id))==inventory+1, "Final daily reward cannot be reclaimed");
        session.Save();check(Open(store).Picnic.Goals.All(g=>g.Claimed), "Daily claims survive save/reopen");
        clock.Today=clock.Today.AddDays(1);check(picnic.Goals.All(g=>!g.Claimed&&g.Current==0), "Picnic progress resets on the injected next day");
        var styles=new HashSet<string>();
        for(int level=600;level<=1000;level+=25)
        {
            var deal=provider.Generate(104729+level*997,level);styles.Add(deal.OpeningEffect);
            var bonusHand=new Hand(deal,rules);check(bonusHand.Effects.HintSeconds>0 || bonusHand.Effects.LuckySeconds>0 || bonusHand.Effects.ShieldSeconds>0, "Bonus opening has a temporary effect");
            bonusHand.AdvanceTime(30);check(bonusHand.Effects.LuckySeconds==0&&bonusHand.Effects.ShieldSeconds==0, "Bonus effects are never permanent");
        }
        check(styles.SetEquals(new[]{"glow","harvest","shelter"}), "All three bonus styles appear");
        string directory=Path.Combine(Path.GetTempPath(),"MeadowPeaks-expansion-"+Guid.NewGuid());Directory.CreateDirectory(directory);
        try
        {
            var disk=new JsonProfileStore(Path.Combine(directory,"progress.json"));session.Save();disk.Save(profile);var reopened=Open(disk);
            check(reopened.Profile.Favorites.Contains(200)&&reopened.Hand?.Deal.Generation=="MP16", "Journal and generation metadata persist in JSON");
        }
        finally {Directory.Delete(directory,true);}
        Console.WriteLine("Phase gates, color seals, four temporary helpers, sharing, favorites, daily picnic and bonus styles passed.");
    }
    sealed class Clock:IGameClock { public DateTime Today {get;set;}=new(2026,10,24); }
    sealed class Seeds:IRandomSource { public int NextSeed()=>42; }
}
