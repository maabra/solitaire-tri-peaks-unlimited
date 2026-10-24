using System.Text.Json;
namespace MeadowPeaks;
// Preserve the complete shared-engine schema, including new mutation/event fields.
// The page validates full gameplay state; this boundary validates the envelope.
internal static class DesktopProfileBridge
{
    public static bool Shared(string json)
    {
        using var data=JsonDocument.Parse(json);
        return data.RootElement.TryGetProperty("Adventure",out var a)&&a.ValueKind==JsonValueKind.Object;
    }
    public static void Validate(string json)
    {
        if(json.Length>64*1024*1024)throw new InvalidDataException("Save is too large.");
        using var data=JsonDocument.Parse(json);var p=data.RootElement;
        if(p.GetProperty("Version").GetInt32()!=1||p.GetProperty("Coins").GetInt32()<0||p.GetProperty("Unlocked").GetInt32() is <1 or >480000)throw new InvalidDataException("Invalid adventure save.");
        if(!Shared(json))ProfileCodec.Decode(json);
    }
    public static string? Load(out string warning,string? path=null)
    {
        warning="";
        path??=JsonProfileStore.DefaultPath;
        foreach(string file in new[]{path,path+".bak"})
        {
            if(!File.Exists(file))continue;
            try{string json=File.ReadAllText(file);Validate(json);if(!Shared(json))return null;if(file.EndsWith(".bak"))warning="Recovered your backup adventure.";return json;}catch{warning="A damaged save was found. Its original file is preserved.";}
        }
        return null;
    }
    public static void Save(string json,string? path=null)
    {
        Validate(json);path??=JsonProfileStore.DefaultPath;Directory.CreateDirectory(Path.GetDirectoryName(path)!);string temp=path+".tmp";File.WriteAllText(temp,json);
        if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
    }
}
