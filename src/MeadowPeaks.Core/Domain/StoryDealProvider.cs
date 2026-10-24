namespace MeadowPeaks;
public sealed class StoryDealProvider : IVersionedDealProvider
{
    readonly IDealProvider story,previous;
    public StoryDealProvider(ISolitaireRules rules)
    {
        story=new AdventureDealProvider(new SeededDealProvider(new StoryLayouts(),rules));
        previous=new AdventureDealProvider(new SeededDealProvider(new AdventureLayouts(),rules));
    }
    public Deal Generate(int seed,int level)=>story.Generate(seed,level);
    public Deal GenerateVersion(int seed,int level,string generation)=>generation=="MP16"?previous.Generate(seed,level):generation=="MP18"?story.Generate(seed,level):throw new ArgumentException("Unknown deal generation.");
}
