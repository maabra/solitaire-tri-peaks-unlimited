using System.Text.Json;
namespace MeadowPeaks;
public static class ProfileCodec
{
    public static Progress Decode(string json)
    {
        var profile=JsonSerializer.Deserialize<Progress>(json);
        if(!ProfileValidator.Valid(profile))throw new InvalidDataException("Invalid saved adventure.");
        return profile!;
    }
}
