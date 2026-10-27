using System.Text.Json;
namespace MeadowPeaks;
public static class ProfileCodec
{
    // Decoding includes migration and validation, not just JSON deserialization. All callers
    // receive the current campaign shape or an error rather than a partially trusted profile.
    public static Progress Decode(string json)
    {
        var profile=JsonSerializer.Deserialize<Progress>(json);
        profile=CampaignMigration.Apply(profile ?? throw new InvalidDataException("Invalid saved adventure."));
        return profile!;
    }
}
