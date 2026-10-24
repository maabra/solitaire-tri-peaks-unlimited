namespace MeadowPeaks;

// Each strategy has a stable inventory ID and only starts a timed hand effect.
public sealed class TemporaryPowerup : IPowerup
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Price { get; }
    public bool RequiresTarget => false;
    public TemporaryPowerup(string id, string name, string description, int price)
    { Id = id; Name = name; Description = description; Price = price; }
    public bool CanUse(ISolitaireHand hand)
    {
        if (hand.Won || hand is IAdventureHand { Failed: true } || hand is not ITemporaryHand timed || timed.Effects.HelperCooldown > 0) return false;
        bool Has(CardKind kind) => hand.Deal.Cards.Where((c, i) => !hand.Removed[i]).Any(c => c.Kind == kind);
        return Id switch {
            "lucky" => timed.Effects.LuckySeconds <= 0,
            "thaw" => timed.Effects.ThawSeconds <= 0 && Has(CardKind.Ice),
            "shield" => timed.Effects.ShieldSeconds <= 0 && Has(CardKind.Bomb),
            "scout" => hand.Moves.Any(), _ => false };
    }
    public bool Apply(ISolitaireHand hand, int? target, int seed) => CanUse(hand) && ((ITemporaryHand)hand).UseEffect(Id);
}
