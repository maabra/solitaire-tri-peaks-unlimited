using MeadowPeaks;
using System.Text.Json;

static class WebFixtures
{
    // Export native engine results for browser parity tests rather than duplicating expected outcomes by hand.
    public static void Export(string path)
    {
        var rules = new TriPeaksRules();
        var provider = new AdventureDealProvider(new SeededDealProvider(new AdventureLayouts(), rules));
        var levels = Enumerable.Range(1, 105).Concat(new[] { 199,200,299,300,399,400,499,500,599,600,699,700,799,800,899,900,999,1000,1299,1300,1999,2000,2099,2100,2299,2300,2399,2400,10000,479999,480000 });
        var deals = levels.Select(level => {int seed=104729+level*997; return new { Level=level, Seed=seed, Deal=provider.Generate(seed,level) }; }).ToArray();
        var storyProvider=new StoryDealProvider(rules);
        var storyDeals=Enumerable.Range(1,128).Concat(new[]{200,300,500,600,700,800,900,2000,2300,2104,480000}).Select(level=>new{Level=level,Seed=104729+level*997,Deal=storyProvider.Generate(104729+level*997,level)}).Concat(Enumerable.Range(1,32).Select(level=>new{Level=level,Seed=104729+level*997,Deal=storyProvider.GenerateVersion(104729+level*997,level,"MP18")})).ToArray();
        var gems = new[] { 1,42,987654321 }.Select(seed => {var game=new GemGame(seed);var frames=new List<object>();for(int i=0;i<20;i++){var pair=game.Hint();if(pair==null)break;game.Play(pair.Value.Item1,pair.Value.Item2);frames.Add(new{Cells=game.Cells.ToArray(),game.Earned,game.Moves});}return new{Seed=seed,Frames=frames};}).ToArray();
        File.WriteAllText(path,JsonSerializer.Serialize(new {Deals=deals,StoryDeals=storyDeals,Gems=gems}));
        Console.WriteLine($"Exported {deals.Length} legacy + {storyDeals.Length} story-engine deals and {gems.Length} gem runs.");
    }
}

