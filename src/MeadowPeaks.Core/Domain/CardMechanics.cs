namespace MeadowPeaks;

// Optional capability: replacement hands need not implement adventure mechanics.
public interface IAdventureHand
{
    bool Failed { get; }
    string CardStatus(int index);
    string BlockReason(int index);
}
public sealed record MechanicState(int Turns = 0, int Clears = 0, bool HasKey = false, bool Failed = false);

// Stateless policy. All mutable state lives in snapshots, so saves and undo agree.
public interface ICardMechanics
{
    bool Allows(Card card, MechanicState state);
    string Status(Card card, MechanicState state);
    MechanicState Advance(MechanicState state, Deal deal, IReadOnlyList<bool> removed, Card? played);
    int Bonus(Card played, Card previous);
}
public sealed class CardMechanics : ICardMechanics
{
    public bool Allows(Card card, MechanicState state) => card.Kind switch
    {
        CardKind.Ice => state.Clears >= card.Strength,
        CardKind.Lock => state.HasKey,
        _ => true
    };
    public string Status(Card card, MechanicState state) => card.Kind switch
    {
        CardKind.Ice => state.Clears < card.Strength ? $"ICE {card.Strength - state.Clears}" : "THAWED",
        CardKind.Key => "KEY",
        CardKind.Lock => state.HasKey ? "UNLOCKED" : "LOCKED",
        CardKind.Bomb => $"BOMB {Math.Max(0, card.Strength - state.Turns)}",
        CardKind.Gift => "+3 CARDS",
        CardKind.Up => "UP +2",
        CardKind.Down => "DOWN +2",
        _ => ""
    };
    public MechanicState Advance(MechanicState state, Deal deal, IReadOnlyList<bool> removed, Card? played)
    {
        var next = state with { Turns = state.Turns + 1, Clears = state.Clears + (played == null ? 0 : 1), HasKey = state.HasKey || played?.Kind == CardKind.Key };
        return next with { Failed = deal.Cards.Where((c, i) => !removed[i]).Any(c => c.Kind == CardKind.Bomb && c.Strength <= next.Turns) };
    }
    public int Bonus(Card played, Card previous) => played.Kind switch
    {
        CardKind.Gift => 3,
        CardKind.Up when previous.Rank != 0 && played.Rank == previous.Rank % 13 + 1 => 2,
        CardKind.Down when previous.Rank != 0 && played.Rank == (previous.Rank + 11) % 13 + 1 => 2,
        _ => 0
    };
}

public sealed class AdventureDealProvider : IDealProvider
{
    readonly IDealProvider source;
    public AdventureDealProvider(IDealProvider source) => this.source = source;
    public Deal Generate(int seed, int level)
    {
        var deal = source.Generate(seed, level);
        var cards = deal.Cards.ToArray();
        int clears = 0; bool key = false; Card previous = deal.Waste;
        for (int turn = 0; turn < deal.Solution.Count; turn++)
        {
            int i = deal.Solution[turn].Index;
            if (i < 0) { previous = deal.Stock[deal.Solution.Take(turn + 1).Count(s => s.Index < 0) - 1]; continue; }
            var kind = CardKind.Normal; int strength = 0;
            if (level >= 20 && clears == 1) { kind = CardKind.Key; key = true; }
            else if (level >= 20 && key && clears > 4 && clears % 7 == 0) kind = CardKind.Lock;
            else if (level >= 35 && clears % 8 == 4) { kind = CardKind.Bomb; strength = turn + 5; }
            else if (level >= 10 && clears >= 3 && clears % 5 == 3) { kind = CardKind.Ice; strength = Math.Max(1, clears - 1); }
            else if (level >= 5 && clears % 7 == 2) kind = CardKind.Gift;
            else if (level >= 50 && clears % 4 == 0) kind = cards[i].Rank == previous.Rank % 13 + 1 ? CardKind.Up : CardKind.Down;
            cards[i] = cards[i] with { Kind = kind, Strength = strength };
            previous = cards[i]; clears++;
        }
        return new Deal { Layout = deal.Layout, Cards = cards, Stock = deal.Stock, Waste = deal.Waste, Solution = deal.Solution, Formation = AdventureLayouts.NameFor(level) };
    }
}
