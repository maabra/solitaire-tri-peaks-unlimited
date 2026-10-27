namespace MeadowPeaks;

public sealed class SolitaireFactory : ISolitaireFactory, IVersionedSolitaireFactory
{
    readonly IDealProvider deals;
    readonly ISolitaireRules rules;
    readonly IDealProvider legacyDeals;
    readonly ICardMechanics mechanics;

    // Bind replaceable deal, rule and mechanic policies behind the common factory contract.
    public SolitaireFactory(IDealProvider deals, ISolitaireRules rules, IDealProvider legacyDeals, ICardMechanics? mechanics = null)
    {
        this.mechanics = mechanics ?? new CardMechanics();
        this.deals = deals;
        this.rules = rules;
        this.legacyDeals = legacyDeals;
    }

    // Construct a hand from a newly generated deal with the configured rule policies.
    public ISolitaireHand Create(int seed, int level) => new Hand(deals.Generate(seed, level), rules, mechanics);
    // Use the requested historical generation when constructing a restored/shared deal.
    public ISolitaireHand CreateVersion(int seed,int level,string generation) => deals is IVersionedDealProvider versioned ? new Hand(versioned.GenerateVersion(seed,level,generation),rules,mechanics) : Create(seed,level);

    // Prefer the captured board and validate it before reconstructing saved live state.
    public ISolitaireHand? Restore(SavedRun run)
    {
        // New saves carry their board. Legacy saves use the original generator,
        // even after the provider for new deals has been replaced.
        var deal = run.DealtBoard ?? legacyDeals.Generate(run.Seed, run.Level);
        if (!ValidBoard(deal)) return null;
        for(int i=0;i<deal.Layout.Count;i++)
        {
            var s=deal.Layout[i];
            if(s.DrawX.HasValue!=s.DrawY.HasValue || s.DrawX.HasValue && (!float.IsFinite(s.DrawX.Value)||!float.IsFinite(s.DrawY!.Value)||Math.Abs(s.DrawX.Value)>2000||Math.Abs(s.DrawY.Value)>1000) || !float.IsFinite(s.Angle) || Math.Abs(s.Angle)>720 || s.BlockedBy!=null && s.BlockedBy.Any(j=>j<=i||j>=deal.Layout.Count))return null;
        }
        return Hand.Restore(run, deal, rules, mechanics);
    }

    // Check card/layout relationships before accepting a serialized board as playable.
    static bool ValidBoard(Deal? deal) => deal != null && deal.Layout != null && deal.Cards != null && deal.Stock != null && deal.Layout.Count > 0 && deal.Layout.Count <= 52 && deal.Layout.Count == deal.Cards.Count && deal.Layout.All(s => s != null && float.IsFinite(s.X) && s.Row >= 0 && s.Row <= 20) && deal.Cards.Concat(deal.Stock).Append(deal.Waste).All(c => c != null && c.Rank >= 1 && c.Rank <= 13 && c.Suit >= 0 && c.Suit <= 3 && Enum.IsDefined(typeof(CardKind), c.Kind) && c.Strength >= 0 && c.Strength <= 200);
}

public sealed class GemGameFactory : ICoinGameFactory, IPersistentCoinGameFactory
{
    // Construct a jewel game from the supplied deterministic seed.
    public ICoinGame Create(int seed) => new GemGame(seed);
    // Replay the saved accepted swaps and reject a garden that cannot be reproduced.
    public ICoinGame? Restore(GemRun run)
    {
        if(run.Seed<0||run.Swaps==null||run.Swaps.Count>20)return null;
        var game=new GemGame(run.Seed);foreach(var move in run.Swaps)if(move==null||!game.Play(move.First,move.Second))return null;
        return game;
    }
}
