namespace MeadowPeaks;

public sealed record DifficultyProfile(int Tier, CardKind Focus, int HazardBudget,
    int IceLead, int KeysRequired, int BombSlack, int ArmorHits, int VineRun, int ReserveTrim)
{
    public string Badge => $"{Focus switch { CardKind.Ice => "\u2744", CardKind.Lock => "\U0001f512", CardKind.Bomb => "\U0001f4a3", CardKind.Stone => "\u25c6", CardKind.Vine => "\U0001f33f", CardKind.Gift => "\U0001f381", CardKind.Up => "\u2191\u2193", CardKind.Lantern => "\U0001f526", CardKind.Lucky => "\U0001f340", CardKind.Shield => "\U0001f6e1", CardKind.Gate => "\u25d0", CardKind.ColorSeal => "\u2665\u2660", _ => "\u2660" }} {Tier}";
}
public interface IDifficultyProgression
{
    // Describe the mechanic focus and bounded obstacle pressure for this level.
    DifficultyProfile ForLevel(int level);
    // Report whether the requested mechanic has been introduced by this level.
    bool Unlocked(CardKind kind, int level);
}

// Progression describes level rules, never permanent player upgrades.
public sealed class CampaignProgression : IDifficultyProgression
{
    // Map obstacle introductions to the shared content catalog. Pacing can change without
    // rewriting the underlying card rules or the algorithms used by historical deals.
    public static int Introduction(CardKind kind) => kind switch
    {
        CardKind.Gift => 50, CardKind.Ice => 100, CardKind.Key or CardKind.Lock => 200,
        CardKind.Bomb => 300, CardKind.Up or CardKind.Down => 400, CardKind.Stone => 500,
        CardKind.Lantern => 600, CardKind.Lucky => 700, CardKind.Vine => 800, CardKind.Shield => 900,
        CardKind.Gate => 2000, CardKind.ColorSeal => 2300,
        _ => 1
    };
    // Check whether the requested mechanic belongs in this level's generated rule set.
    public bool Unlocked(CardKind kind, int level) => level >= Introduction(kind);
    // Calculate bounded hazard pressure and select a focus among already introduced mechanics.
    public DifficultyProfile ForLevel(int level)
    {
        int tier = Math.Max(0, level / 100);
        CardKind focus = tier < 10 ? tier switch {
            0 => CardKind.Gift, 1 => CardKind.Ice, 2 => CardKind.Lock, 3 => CardKind.Bomb,
            4 => CardKind.Up, 5 => CardKind.Stone, 6 => CardKind.Lantern, 7 => CardKind.Lucky,
            8 => CardKind.Vine, _ => CardKind.Shield
        } : new[] { CardKind.Ice, CardKind.Lock, CardKind.Bomb, CardKind.Stone, CardKind.Vine, CardKind.Normal }[(tier-10)%6];
        if (tier == 20) focus = CardKind.Gate;
        if (tier == 23) focus = CardKind.ColorSeal;
        if (tier >= 24) focus = new[] { CardKind.Ice, CardKind.Lock, CardKind.Bomb, CardKind.Stone, CardKind.Vine, CardKind.Gate, CardKind.ColorSeal, CardKind.Normal }[(tier-24)%8];
        // Increase this difficulty component only after its introduction tier, then at six-tier intervals.
        int Steps(int first) => tier < first ? 0 : 1 + (tier-first)/6;
        // Bounded upgrades preserve a reasonable difficulty ceiling. Once a
        // component reaches its cap, later tiers focus on different combinations.
        return new(tier, focus, Math.Min(10, tier/2+1),
            Math.Max(0, 3-Steps(10)), Math.Min(3, 1+Steps(11)),
            Math.Max(2, 6-Steps(12)), Math.Min(4, 2+Steps(13)),
            Math.Min(3, 2+Steps(14)), Math.Min(8, Steps(15)));
    }
}
