using MeadowPeaks;
using System.Text.Json;

static class ProgressionTests
{
    public static void Run(Action<bool,string> check)
    {
        var progression = new CampaignProgression();
        foreach (CardKind kind in Enum.GetValues<CardKind>().Where(k => k != CardKind.Normal))
        {
            int first = CampaignProgression.Introduction(kind);
            check(!progression.Unlocked(kind,first-1) && progression.Unlocked(kind,first), "Mechanic unlock boundary: " + kind);
        }
        DifficultyProfile? previous = null;
        for (int level = 100; level <= MeadowCampaign.TotalLevels; level += 100)
        {
            var p = progression.ForLevel(level);
            check(p.Tier == level/100 && p == progression.ForLevel(level+99), "Difficulty stable within each 100-level tier");
            check(p.ArmorHits is >=2 and <=4 && p.KeysRequired is >=1 and <=3 && p.VineRun is >=2 and <=3 && p.BombSlack is >=2 and <=6 && p.HazardBudget<=10 && p.ReserveTrim<=8, "Upgrades have bounded strength");
            if (previous != null)
                check(p.ArmorHits>=previous.ArmorHits && p.KeysRequired>=previous.KeysRequired && p.VineRun>=previous.VineRun && p.ReserveTrim>=previous.ReserveTrim && p.BombSlack<=previous.BombSlack && p.IceLead<=previous.IceLead, "Baseline pressure never decreases between tiers");
            if (level > 1000) check(p.Focus != previous!.Focus, "Each late tier changes the component emphasis");
            previous = p;
        }
        check(AdventureLayouts.DepthFor(299)==4 && Enumerable.Range(300,400).Select(AdventureLayouts.DepthFor).Max()==5, "Five rows arrive at 300");
        check(Enumerable.Range(700,600).Select(AdventureLayouts.DepthFor).Max()==6 && Enumerable.Range(1300,800).Select(AdventureLayouts.DepthFor).Max()==7 && Enumerable.Range(2100,100).Select(AdventureLayouts.DepthFor).Max()==8, "Six to eight rows are stretched across later tiers");
        var rules = new TriPeaksRules();
        var source = new SeededDealProvider(new AdventureLayouts(),rules);
        var provider = new AdventureDealProvider(source,progression);
        foreach (int level in new[] {49,50,99,100,199,200,299,300,399,400,499,500,599,600,699,700,799,800,899,900,1000,1100,1200,1300,1400,1500,1700,1900,3000,480000})
        {
            int seed=104729+level*997; var deal=provider.Generate(seed,level); var original=source.Generate(seed,level);
            check(deal.Cards.All(c => progression.Unlocked(c.Kind,level)), "No premature special card: " + level);
            check(deal.Stock.Count==Math.Max(deal.Solution.Count(s=>s.Index<0),original.Stock.Count-progression.ForLevel(level).ReserveTrim), "Only nonessential reserve draws are trimmed");
            var hand=new Hand(deal,rules);
            foreach (var step in deal.Solution) { hand.AdvanceTime(60); check(step.Index<0 ? TimedMoves.Draw(hand) : hand.Play(step.Index), "Milestone is solvable without active effects"); }
            check(hand.Won, "Milestone solved");
        }
        Deal Board(params Card[] cards) => new() { TemporaryRules=true, Cards=cards, Layout=cards.Select((_,i)=>new Slot(i*2,0)).ToArray(), Waste=new(5,0) };
        var h=new Hand(Board(new Card(6,0){Kind=CardKind.Stone,Strength=4},new(7,0)),rules);
        for(int hit=1;hit<=3;hit++) check(h.Play(0) && !h.Removed[0] && h.Effects.ArmorDamage![0]==hit, "Four-hit armor peels one layer at a time");
        var run=JsonSerializer.Deserialize<SavedRun>(JsonSerializer.Serialize(h.Capture(1,1900,false)))!;
        var factory=new SolitaireFactory(provider,rules,source);
        var restored=(Hand)factory.Restore(run)!;
        check(restored.Play(0) && restored.Removed[0], "Advanced armor damage survives JSON restore");
        check(h.Undo() && h.Effects.ArmorDamage![0]==2 && h.Play(0) && h.Play(0) && h.Removed[0], "Undo restores armor layers without aliasing snapshots");
        var legacyArmor = new Hand(Board(new Card(6,0){Kind=CardKind.Stone},new Card(6,1){Kind=CardKind.Stone}),rules);
        var legacyRun = legacyArmor.Capture(1,500,false) with { Mechanics = new MechanicState(Peeled:1) };
        h=(Hand)factory.Restore(legacyRun)!;
        check(h.Play(1) && !h.Removed[1] && h.Play(0) && h.Removed[0], "Legacy armor damage remains after a different armor card is hit");
        h=new Hand(Board(new(6,0){Kind=CardKind.Key},new(7,0){Kind=CardKind.Key},new(8,0){Kind=CardKind.Key},new(9,0){Kind=CardKind.Lock,Strength=3}),rules);
        check(h.Play(0)&&h.Play(1)&&h.Effects.Keys==2, "Multiple keys accumulate");
        check(!new CardMechanics().Allows(h.Deal.Cards[3],h.Effects) && h.Play(2)&&h.Play(3), "Reinforced locks require every key");
        h=new Hand(Board(new(6,0),new(7,0),new(8,0),new(9,0){Kind=CardKind.Vine,Strength=3}),rules);
        check(h.Play(0)&&h.Play(1)&&!new CardMechanics().Allows(h.Deal.Cards[3],h.Effects)&&h.Play(2)&&h.Play(3), "Advanced vines need a three-clear run");
        Console.WriteLine("Stretched unlocks, 100-level tiers, capped upgrades and advanced-state persistence passed.");
    }
}
