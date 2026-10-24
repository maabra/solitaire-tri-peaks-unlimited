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
            new PowerupCatalog(new IPowerup[] { new WildPowerup(), new ExtraCardsPowerup(), new RemovePowerup(),
                new TemporaryPowerup("lucky", "\U0001f340 +1", "+1 stock per clear\nfor 10 seconds", 85),
                new TemporaryPowerup("thaw", "\U0001f525", "Ignore ice for 8 seconds", 70),
                new TemporaryPowerup("shield", "\U0001f6e1", "Pause bomb fuses\nfor 10 seconds", 80),
                new TemporaryPowerup("scout", "\U0001f526", "Show matches\nfor 5 seconds", 35) }),
            new LocalGameClock(),
            new RandomSource(),
            new MeadowCampaign());
    }
}
