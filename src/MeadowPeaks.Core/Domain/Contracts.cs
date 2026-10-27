namespace MeadowPeaks;

public interface ISolitaireRules
{
    // Accept neighboring ranks, including the Ace-King wrap and the supported Wild sentinel.
    bool Adjacent(int a, int b);
    // Check whether all cards covering this slot have been removed.
    bool Exposed(IReadOnlyList<Slot> layout, IReadOnlyList<bool> removed, int index);
}

public interface ILayoutProvider
{
    // Build slots and cover relationships for the requested level.
    List<Slot> Create(int level);
}

public interface IDealProvider
{
    // Produce a deterministic deal from the requested seed and level.
    Deal Generate(int seed, int level);
}
public interface IVersionedDealProvider : IDealProvider
{
    // Reproduce the deal algorithm identified by a saved generation tag.
    Deal GenerateVersion(int seed,int level,string generation);
}
public interface IVersionedSolitaireFactory
{
    // Construct a hand using the requested historical generation.
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
    // Check whether a card is uncovered before exposing it to interaction.
    bool IsExposed(int index);
    // Check a card's complete legality without changing the hand.
    bool CanPlay(int index);
    // Attempt a card interaction and report whether game state changed.
    bool Play(int index);
    // Attempt drawing the next stock card while respecting the hand's rules.
    bool Draw();
    // Restore a reversible action if the hand permits undo.
    bool Undo();
    // Apply the Wild waste effect when the hand permits it.
    bool Wild();
    // Add seeded rescue stock when the hand permits it.
    bool Extra(int seed);
    // Remove an eligible exposed target without ordinary rank matching.
    bool Remove(int index);
    // Return a detached saved run that includes the board needed for exact restoration.
    SavedRun Capture(int seed, int level, bool endless);
}

public interface ISolitaireFactory
{
    // Construct a new hand from the supplied deterministic seed and level.
    ISolitaireHand Create(int seed, int level);
    // Reconstruct a valid saved hand or reject an inconsistent run.
    ISolitaireHand? Restore(SavedRun run);
}

// Alternative coin games can keep the same board UI, or provide their own view.
public interface ICoinGame
{
    IReadOnlyList<int> Cells { get; }
    int Moves { get; }
    int Earned { get; }
    int LastGain { get; }
    // Return a legal matching swap without consuming a move.
    (int, int)? Hint();
    // Attempt the neighboring jewel swap and expose its actual earned gain.
    bool Play(int first, int second);
}

public interface ICoinGameFactory
{
    // Construct a deterministic coin game from the supplied seed.
    ICoinGame Create(int seed);
}
