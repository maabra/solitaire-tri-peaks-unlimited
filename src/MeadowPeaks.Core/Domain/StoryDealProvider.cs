namespace MeadowPeaks;
public sealed class StoryDealProvider : IVersionedDealProvider
{
    readonly IDealProvider story,previous,current;
    // Bind historical generation providers while keeping the current generator as the default.
    public StoryDealProvider(ISolitaireRules rules)
    {
        current=new AdventureDealProvider(new SeededDealProvider(new StoryLayoutsV2(),rules),generation:"MP19");
        story=new AdventureDealProvider(new SeededDealProvider(new StoryLayouts(),rules));
        previous=new AdventureDealProvider(new SeededDealProvider(new AdventureLayouts(),rules));
    }
    // Generate a new hand with the current supported story algorithm.
    public Deal Generate(int seed,int level)=>current.Generate(seed,level);
    // A saved generation identifies the original rules/layout algorithm, not just a visual style.
    // Dispatch to that version so replay and save restoration remain deterministic.
    public Deal GenerateVersion(int seed,int level,string generation)=>generation=="MP16"?previous.Generate(seed,level):generation=="MP18"?story.Generate(seed,level):generation=="MP19"?current.Generate(seed,level):throw new ArgumentException("Unknown deal generation.");
}
