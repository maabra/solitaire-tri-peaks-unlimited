using System.Text.Json;

namespace MeadowPeaks;

public sealed class JsonProfileStore : IProfileStore
{
    readonly string path;
    public JsonProfileStore(string path) { this.path = path; }
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeadowPeaks", "progress.json");
    public Progress Load(out string warning)
    {
        warning = "";
        if (!File.Exists(path)) return new();
        // Try the main file before its backup, but preserve both if neither validates.
        // A failed load should not destroy the only copy of a player's adventure.
        foreach (var candidate in new[] { path, path + ".bak" })
        {
            try
            {
                var p = JsonSerializer.Deserialize<Progress>(File.ReadAllText(candidate));
                p = CampaignMigration.Apply(p ?? throw new InvalidDataException());
                if (candidate != path) warning = "Recovered progress from the backup save.";
                return p!;
            }
            catch (Exception e) when (e is IOException || e is JsonException || e is InvalidDataException || e is UnauthorizedAccessException) { }
        }
        warning = "The save could not be read. A new profile is open; the old files are preserved until you make progress.";
        return new();
    }
    public void Save(Progress profile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        // Serialize to a sibling temporary file before replacing the live save. The existing
        // valid save is copied to the backup separately, so corrupt data cannot replace recovery.
        string temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
        // Only a valid previous save may replace the recovery backup.
        if (File.Exists(path))
        {
            try { var old = JsonSerializer.Deserialize<Progress>(File.ReadAllText(path)); if (ProfileValidator.Valid(old, true)) File.Copy(path, path + ".bak", true); }
            catch (JsonException) { }
        }
        File.Move(temp, path, true);
    }
}
