namespace MeadowPeaks;

// The composition root is the one place that selects concrete implementations.
// Replace a constructor argument here to install a compatible component.
public static class Composition
{
    public static GameSession CreateSession(IProfileStore store)
    {
        var rules = new TriPeaksRules();
        var deals = new SeededDealProvider(new PeakLayouts(), rules);
        return new GameSession(
            new SolitaireFactory(new AdventureDealProvider(new SeededDealProvider(new AdventureLayouts(), rules)), rules, deals),
            new GemGameFactory(),
            store,
            new StandardEconomyPolicy(),
            new PowerupCatalog(new IPowerup[] { new WildPowerup(), new ExtraCardsPowerup(), new RemovePowerup() }),
            new LocalGameClock(),
            new RandomSource(),
            new MeadowCampaign());
    }
}
