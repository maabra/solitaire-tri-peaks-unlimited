using System.Text.Json;

namespace MeadowPeaks;

public sealed class LocalGameClock : IGameClock { public DateTime Today => DateTime.Today; }
public sealed class RandomSource : IRandomSource { /* Supply a seed through the injected random source so game logic need not own global randomness. */
    public int NextSeed() => Random.Shared.Next(); }

// Useful for smoke tests, previews, or a game session that must not touch disk.
public sealed class MemoryProfileStore : IProfileStore
{
    string data;
    // Hold an isolated serialized profile for tests or replaceable in-memory storage.
    public MemoryProfileStore(Progress? initial = null) { data = JsonSerializer.Serialize(initial ?? new Progress()); }
    // Return a detached profile so callers cannot mutate stored state without saving.
    public Progress Load(out string warning)
    {
        warning = "";
        return JsonSerializer.Deserialize<Progress>(data)!;
    }
    // Store a detached snapshot rather than retaining the caller's mutable profile reference.
    public void Save(Progress profile) => data = JsonSerializer.Serialize(profile);
}
