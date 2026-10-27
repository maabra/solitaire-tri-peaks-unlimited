namespace MeadowPeaks;

public sealed record GemSwap(int First,int Second);
public sealed record GemRun(int Seed,List<GemSwap> Swaps);
public interface IPersistentCoinGame { /* Capture the seed and accepted actions needed to restore this coin game. */
    GemRun Capture(); }
public interface IPersistentCoinGameFactory { /* Restore a saved coin game only if its accepted action history replays. */
    ICoinGame? Restore(GemRun run); }
public sealed class GemGame : ICoinGame, IAnimatedCoinGame, IPersistentCoinGame
{
    readonly int seed;
    readonly List<GemSwap> swaps=new();
    // Persist the seed and accepted swaps so the garden can be replayed exactly.
    public GemRun Capture()=>new(seed,swaps.ToList());
    readonly List<GemAnimationFrame> frames = new();
    public IReadOnlyList<GemAnimationFrame> LastAnimation => frames.AsReadOnly();
    // Copy a visual snapshot for animation; later cascades must not mutate earlier recorded frames.
    void Frame(GemFrameKind kind, HashSet<int>? matched, int cascade, int first = -1, int second = -1) => frames.Add(new(kind, Array.AsReadOnly((int[])cells.Clone()), Array.AsReadOnly(matched?.ToArray() ?? Array.Empty<int>()), cascade, first, second));
    readonly int[] cells = new int[64];
    public IReadOnlyList<int> Cells => Array.AsReadOnly(cells);
    public int Moves { get; private set; } = 20;
    public int Earned { get; private set; }
    public int LastGain { get; private set; }
    readonly Random rng;
    // Create the seeded board and its initial move/reward counters.
    public GemGame(int seed) { this.seed=seed;rng = new Random(seed); Fresh(); }
    // Avoid free matches in the starting grid and retry boards with no legal swap.
    // The player should begin with a choice, rather than an automatic cascade or a dead end.
    void Fresh()
    {
        do
        {
            for (int i = 0; i < 64; i++)
            {
                do { cells[i] = rng.Next(6); }
                while ((i % 8 >= 2 && cells[i] == cells[i - 1] && cells[i] == cells[i - 2]) || (i >= 16 && cells[i] == cells[i - 8] && cells[i] == cells[i - 16]));
            }
        } while (Hint() == null);
    }
    // Collect the union of cells in horizontal and vertical runs.
    public HashSet<int> Matches()
    {
        var result = new HashSet<int>();
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 < 6 && cells[i] == cells[i + 1] && cells[i] == cells[i + 2]) { result.Add(i); result.Add(i + 1); result.Add(i + 2); }
            if (i < 48 && cells[i] == cells[i + 8] && cells[i] == cells[i + 16]) { result.Add(i); result.Add(i + 8); result.Add(i + 16); }
        }
        return result;
    }
    // Require orthogonal adjacency without allowing a wrap between rows.
    static bool Neighbors(int a, int b) => a >= 0 && b >= 0 && a < 64 && b < 64 && (Math.Abs(a / 8 - b / 8) + Math.Abs(a % 8 - b % 8) == 1);
    // Exchange the two cells; legality and rollback belong to the caller.
    void Swap(int a, int b) => (cells[a], cells[b]) = (cells[b], cells[a]);
    // Try candidate swaps temporarily and restore the board before returning a legal suggestion.
    public (int, int)? Hint()
    {
        for (int i = 0; i < 64; i++) foreach (int j in new[] { i + 1, i + 8 })
        {
            if (!Neighbors(i, j)) continue;
            Swap(i, j); bool match = Matches().Count > 0; Swap(i, j);
            if (match) return (i, j);
        }
        return null;
    }
    // Reject nonmatching swaps, then resolve cascades and record each visual phase and actual gain.
    public bool Play(int a, int b)
    {
        frames.Clear();
        if (Moves == 0 || !Neighbors(a, b)) return false;
        Swap(a, b); var matches = Matches();
        if (matches.Count == 0) { Swap(a, b); return false; }
        swaps.Add(new(a,b));
        Frame(GemFrameKind.Swap, null, 1, a, b);
        Moves--; LastGain = 0; int chain = 1;
        while (matches.Count > 0)
        {
            Frame(GemFrameKind.Match, matches, chain);
            LastGain += matches.Count * 2 * chain;
            for (int col = 0; col < 8; col++)
            {
                var kept = Enumerable.Range(0, 8).Select(row => row * 8 + col).Where(i => !matches.Contains(i)).Select(i => cells[i]).ToList();
                int missing = 8 - kept.Count;
                for (int row = 0; row < 8; row++) cells[row * 8 + col] = row < missing ? rng.Next(6) : kept[row - missing];
            }
            Frame(GemFrameKind.Drop, matches, chain);
            chain++; matches = Matches();
        }
        Earned += LastGain;
        if (Moves > 0 && Hint() == null) Fresh();
        return true;
    }
}
