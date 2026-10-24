namespace MeadowPeaks;

public sealed class SolitaireFactory : ISolitaireFactory
{
    readonly IDealProvider deals;
    readonly ISolitaireRules rules;
    readonly IDealProvider legacyDeals;

    public SolitaireFactory(IDealProvider deals, ISolitaireRules rules, IDealProvider legacyDeals)
    {
        this.deals = deals;
        this.rules = rules;
        this.legacyDeals = legacyDeals;
    }

    public ISolitaireHand Create(int seed, int level) => new Hand(deals.Generate(seed, level), rules);

    public ISolitaireHand? Restore(SavedRun run)
    {
        // New saves carry their board. Legacy saves use the original generator,
        // even after the provider for new deals has been replaced.
        var deal = run.DealtBoard ?? legacyDeals.Generate(run.Seed, run.Level);
        if (!ValidBoard(deal)) return null;
        return Hand.Restore(run, deal, rules);
    }

    static bool ValidBoard(Deal? deal) => deal != null && deal.Layout != null && deal.Cards != null && deal.Stock != null && deal.Layout.Count > 0 && deal.Layout.Count <= 52 && deal.Layout.Count == deal.Cards.Count && deal.Layout.All(s => s != null && float.IsFinite(s.X) && s.Row >= 0 && s.Row <= 20) && deal.Cards.Concat(deal.Stock).Append(deal.Waste).All(c => c != null && c.Rank >= 1 && c.Rank <= 13 && c.Suit >= 0 && c.Suit <= 3);
}

public sealed class GemGameFactory : ICoinGameFactory
{
    public ICoinGame Create(int seed) => new GemGame(seed);
}
