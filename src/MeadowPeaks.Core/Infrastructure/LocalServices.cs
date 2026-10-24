using System.Text.Json;

namespace MeadowPeaks;

public sealed class LocalGameClock : IGameClock { public DateTime Today => DateTime.Today; }
public sealed class RandomSource : IRandomSource { public int NextSeed() => Random.Shared.Next(); }

// Useful for smoke tests, previews, or a game session that must not touch disk.
public sealed class MemoryProfileStore : IProfileStore
{
    string data;
    public MemoryProfileStore(Progress? initial = null) { data = JsonSerializer.Serialize(initial ?? new Progress()); }
    public Progress Load(out string warning)
    {
        warning = "";
        return JsonSerializer.Deserialize<Progress>(data)!;
    }
    public void Save(Progress profile) => data = JsonSerializer.Serialize(profile);
}
