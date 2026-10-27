namespace MeadowPeaks;

public sealed class WildPowerup : IPowerup
{
    public string Id => "wild";
    public string Name => "\u2605 Wild";
    public string Description => "Any exposed card plays\non a Wild. Keep your streak.";
    public int Price => 75;
    public bool RequiresTarget => false;
    // Check helper eligibility without consuming inventory or changing the hand.
    public bool CanUse(ISolitaireHand hand) => !hand.Won && hand is not IAdventureHand { Failed: true } && hand.Waste.Rank != 0;
    // Apply this helper only to a valid hand/target; the session owns inventory and payment.
    public bool Apply(ISolitaireHand hand, int? target, int seed) => CanUse(hand) && hand.Wild();
}

public sealed class ExtraCardsPowerup : IPowerup
{
    public string Id => "extra";
    public string Name => "\u2660 +5 cards";
    public string Description => "Five rescue cards. Keep until used.\nUse when stock is empty.";
    public int Price => 60;
    public bool RequiresTarget => false;
    // Check helper eligibility without consuming inventory or changing the hand.
    public bool CanUse(ISolitaireHand hand) => !hand.Won && hand is not IAdventureHand { Failed: true } && hand.Stock.Count == 0;
    // Apply this helper only to a valid hand/target; the session owns inventory and payment.
    public bool Apply(ISolitaireHand hand, int? target, int seed) => CanUse(hand) && hand.Extra(seed);
}

public sealed class RemovePowerup : IPowerup
{
    public string Id => "remove";
    public string Name => "\U0001f528 Remove";
    public string Description => "Choose any exposed card\nand remove it from the board.";
    public int Price => 90;
    public bool RequiresTarget => true;
    // Check helper eligibility without consuming inventory or changing the hand.
    public bool CanUse(ISolitaireHand hand) => !hand.Won && hand is not IAdventureHand { Failed: true };
    // Apply this helper only to a valid hand/target; the session owns inventory and payment.
    public bool Apply(ISolitaireHand hand, int? target, int seed) => CanUse(hand) && target.HasValue && hand.Remove(target.Value);
}

public sealed class PowerupCatalog
{
    readonly Dictionary<string, IPowerup> byId;
    public IReadOnlyList<IPowerup> All { get; }

    // Validate stable nonempty helper identifiers and nonnegative prices before indexing the catalog.
    public PowerupCatalog(IEnumerable<IPowerup> powerups)
    {
        var items = powerups.ToArray();
        if (items.Any(p => string.IsNullOrWhiteSpace(p.Id) || p.Price < 0)) throw new ArgumentException("Helpers need stable IDs and non-negative prices.");
        byId = items.ToDictionary(p => p.Id, StringComparer.Ordinal);
        All = Array.AsReadOnly(items);
    }
    // Look up a helper by its stable saved identifier rather than its display label.
    public IPowerup? Find(string id) => byId.GetValueOrDefault(id);
}
