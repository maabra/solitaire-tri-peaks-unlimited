namespace MeadowPeaks;

public record Card(int Rank, int Suit);
public record Slot(float X, int Row);
public record Step(int Index); // -1 draws from stock
public sealed class Deal
{
    public IReadOnlyList<Slot> Layout { get; init; } = Array.Empty<Slot>();
    public IReadOnlyList<Card> Cards { get; init; } = Array.Empty<Card>();
    public IReadOnlyList<Card> Stock { get; init; } = Array.Empty<Card>();
    public Card Waste { get; init; } = new(1, 0);
    public IReadOnlyList<Step> Solution { get; init; } = Array.Empty<Step>();
}
