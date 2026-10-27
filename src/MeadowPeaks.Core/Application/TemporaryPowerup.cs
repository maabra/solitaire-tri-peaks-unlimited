namespace MeadowPeaks;

// Each strategy has a stable inventory ID and only starts a timed hand effect.
public sealed class TemporaryPowerup : IPowerup
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Price { get; }
    public bool RequiresTarget => false;
    // Define one timed helper's identity, price and associated effect.
    public TemporaryPowerup(string id, string name, string description, int price)
    { Id = id; Name = name; Description = description; Price = price; }
    // Require the timed-hand capability and a relevant active-board situation.
    public bool CanUse(ISolitaireHand hand)
    {
        if (hand.Won || hand is IAdventureHand { Failed: true } || hand is not ITemporaryHand timed || timed.Effects.HelperCooldown > 0) return false;
        // Look for the mechanic or condition needed to make this timed helper useful.
        bool Has(CardKind kind) => hand.Deal.Cards.Where((c, i) => !hand.Removed[i]).Any(c => c.Kind == kind);
        return Id switch {
            "lucky" => timed.Effects.LuckySeconds <= 0,
            "thaw" => timed.Effects.ThawSeconds <= 0 && Has(CardKind.Ice),
            "shield" => timed.Effects.ShieldSeconds <= 0 && Has(CardKind.Bomb),
            "keypass" => timed.Effects.KeySeconds <= 0 && Has(CardKind.Lock),
            "prune" => timed.Effects.PruneSeconds <= 0 && Has(CardKind.Vine),
            "phase" => timed.Effects.PhaseSeconds <= 0 && Has(CardKind.Gate),
            "peek" => timed.Effects.PeekSeconds <= 0 && hand.Stock.Count > 0,
            "scout" => hand.Moves.Any(), _ => false };
    }
    // Activate the effect through the timed-hand capability after eligibility checks.
    public bool Apply(ISolitaireHand hand, int? target, int seed) => CanUse(hand) && ((ITemporaryHand)hand).UseEffect(Id);
}
