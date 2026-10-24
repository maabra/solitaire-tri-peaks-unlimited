namespace MeadowPeaks;

public enum CardKind { Normal, Ice, Key, Lock, Bomb, Gift, Up, Down, Stone, Vine, Lantern, Lucky, Shield }
public record Card(int Rank, int Suit)
{
    public CardKind Kind { get; init; }
    public int Strength { get; init; }
}
public record Slot(float X, int Row);
public record Step(int Index); // -1 draws from stock
public sealed class Deal
{
    public DifficultyProfile? Difficulty { get; init; }
    public bool TemporaryRules { get; init; }
    public bool Bonus { get; init; }
    public string Formation { get; init; } = "Classic peaks";
    public IReadOnlyList<Slot> Layout { get; init; } = Array.Empty<Slot>();
    public IReadOnlyList<Card> Cards { get; init; } = Array.Empty<Card>();
    public IReadOnlyList<Card> Stock { get; init; } = Array.Empty<Card>();
    public Card Waste { get; init; } = new(1, 0);
    public IReadOnlyList<Step> Solution { get; init; } = Array.Empty<Step>();
}
