using MeadowPeaks;
using System.Text.Json;
static class StoryTests
{
    public static void Run(Action<bool,string> check,bool all=false)
    {
        var rules=new TriPeaksRules();var provider=new StoryDealProvider(rules);
        var legacy=new SeededDealProvider(new PeakLayouts(),rules);
        var factory=new SolitaireFactory(provider,rules,legacy);
        var clock=new Clock();var store=new MemoryProfileStore();
        var session=new GameSession(factory,new GemGameFactory(),store,new StandardEconomyPolicy(),new PowerupCatalog(new IPowerup[]{new WildPowerup()}),clock,new Seeds());
        session.OpenCoinGame();var original=session.CoinGame;var pair=original!.Hint()!.Value;session.PlayGems(pair.Item1,pair.Item2);session.Save();
        check(!session.NewCoinGame()&&ReferenceEquals(session.CoinGame,original),"Fresh Garden rejects reset during one-hour cooldown");
        clock.UtcNow=clock.UtcNow.AddSeconds(3599);check(!session.NewCoinGame(),"Garden still locked after 59m59s");
        var reopened=new GameSession(factory,new GemGameFactory(),store,new StandardEconomyPolicy(),session.Powerups,clock,new Seeds());
        check(reopened.GardenRefreshRemaining==1 && reopened.CoinGame!.Cells.SequenceEqual(original.Cells)&&reopened.CoinGame.Moves==19,"Garden board, moves and real-time deadline survive reopen");
        clock.UtcNow=clock.UtcNow.AddSeconds(1);check(reopened.NewCoinGame() && reopened.GardenRefreshRemaining==3600,"Garden refresh becomes available after one hour");
        session.StartHand(1,false);string share=session.ShareCode!;
        check(!session.HelperDiscovered("thaw")&&!session.HelperDiscovered("phase")&&!session.HelperDiscovered("peek"),"Unknown helpers are hidden before their mechanics are discovered");
        session.Profile.SeenMechanics.Add(CardKind.Ice);
        check(session.HelperDiscovered("thaw")&&!session.HelperRelevant("thaw"),"Known ice helper stays off a board without ice");
        session.Profile.Unlocked=100;session.StartHand(100,false);
        check(session.HelperDiscovered("thaw")&&session.HelperRelevant("thaw"),"Playing an ice level discovers its helper");
        session.StartHand(1,false);
        check(!session.HelperRelevant("thaw"),"Irrelevant helpers disappear on return to a normal board");
        check(share.StartsWith("MP19-")&&session.StartSharedDeal(share),"Story deals use a new reproducible code generation");
        check(session.StartSharedDeal(DealCodes.Encode(new(1,105726,false)))&&session.Hand!.Deal.Generation=="MP16","Old MP16 deal codes still use their original generator");
        int count=all?MeadowCampaign.TotalLevels:512;
        var patterns=new HashSet<string>();
        for(int level=1;level<=count;level++)
        {
            var h=(Hand)factory.Create(104729+level*997,level);
            check(h.Deal.Cards.Count>=20&&h.Deal.Cards.Count<=34,"Story board uses a bounded deck");
            var geometry=h.Deal.Layout.SelectMany(s=>new[]{s.DrawX!.Value,s.DrawY!.Value,s.Angle}).SelectMany(BitConverter.GetBytes).ToArray();
            check(patterns.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(geometry))),$"Story geometry is distinct: {level}");
            check(h.Deal.Layout.Select((s,i)=>s.BlockedBy!=null&&s.BlockedBy.All(j=>j>i&&j<h.Deal.Layout.Count)).All(x=>x),"Cover graph is acyclic and in range");
            foreach(var step in h.Deal.Solution){h.AdvanceTime(30);check(step.Index<0?h.Draw():h.Play(step.Index),$"Story witness {level}/{step.Index}");}
            check(h.Won&&!h.Failed,"Story formation is solvable without helpers or timed effects");
            check(factory.Restore(JsonSerializer.Deserialize<SavedRun>(JsonSerializer.Serialize(h.Capture(104729+level*997,level,false)))!)?.Won==true,"Rotations and cover graph survive JSON save restore");
            if(all&&level%40000==0)Console.WriteLine($"Story witnesses: {level:N0}/{count:N0}");
        }
        Console.WriteLine($"PASS: {count:N0} story formations; one-hour garden cooldown, board persistence and versioned deal codes.");
    }
    sealed class Clock:IGameClock {public DateTime Today=>UtcNow.Date;public DateTimeOffset UtcNow {get;set;}=new(2026,10,24,12,0,0,TimeSpan.Zero);}
    sealed class Seeds:IRandomSource{public int NextSeed()=>42;}
}
