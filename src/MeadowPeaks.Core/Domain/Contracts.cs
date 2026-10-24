namespace MeadowPeaks;

public interface ISolitaireRules
{
    bool Adjacent(int a, int b);
    bool Exposed(IReadOnlyList<Slot> layout, IReadOnlyList<bool> removed, int index);
}

public interface ILayoutProvider
{
    List<Slot> Create(int level);
}

public interface IDealProvider
{
    Deal Generate(int seed, int level);
}
public interface IVersionedDealProvider : IDealProvider
{
    Deal GenerateVersion(int seed,int level,string generation);
}
public interface IVersionedSolitaireFactory
{
    ISolitaireHand CreateVersion(int seed,int level,string generation);
}

public interface ISolitaireHand
{
    Deal Deal { get; }
    IReadOnlyList<bool> Removed { get; }
    IReadOnlyList<Card> Stock { get; }
    Card Waste { get; }
    int Score { get; }
    int Streak { get; }
    int BestStreak { get; }
    int FreeUndos { get; }
    bool Paid { get; set; }
    bool Won { get; }
    bool CanUndo { get; }
    IEnumerable<int> Moves { get; }
    bool IsExposed(int index);
    bool CanPlay(int index);
    bool Play(int index);
    bool Draw();
    bool Undo();
    bool Wild();
    bool Extra(int seed);
    bool Remove(int index);
    SavedRun Capture(int seed, int level, bool endless);
}

public interface ISolitaireFactory
{
    ISolitaireHand Create(int seed, int level);
    ISolitaireHand? Restore(SavedRun run);
}

// Alternative coin games can keep the same board UI, or provide their own view.
public interface ICoinGame
{
    IReadOnlyList<int> Cells { get; }
    int Moves { get; }
    int Earned { get; }
    int LastGain { get; }
    (int, int)? Hint();
    bool Play(int first, int second);
}

public interface ICoinGameFactory
{
    ICoinGame Create(int seed);
}
