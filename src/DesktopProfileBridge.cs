using System.Text.Json;
namespace MeadowPeaks;
// Preserve the complete shared-engine schema, including new mutation/event fields.
// The page validates full gameplay state; this boundary validates the envelope.
internal static class DesktopProfileBridge
{
    // Detect the shared browser-engine save schema by its adventure-state envelope.
    public static bool Shared(string json)
    {
        using var data=JsonDocument.Parse(json);
        return data.RootElement.TryGetProperty("Adventure",out var a)&&a.ValueKind==JsonValueKind.Object;
    }
    // Reject invalid bridge payloads before accepting them as desktop progress.
    public static void Validate(string json)
    {
        if(json.Length>64*1024*1024)throw new InvalidDataException("Save is too large.");
        using var data=JsonDocument.Parse(json);var p=data.RootElement;
        if(p.TryGetProperty("Hearts",out var hearts)&&hearts.GetInt32() is <0 or >999)throw new InvalidDataException("Invalid hearts.");
        if(p.GetProperty("Version").GetInt32()!=1||p.GetProperty("Coins").GetInt32()<0||p.GetProperty("Unlocked").GetInt32() is <1 or >480000)throw new InvalidDataException("Invalid adventure save.");
        if(!Shared(json))ProfileCodec.Decode(json);
    }
    // Read and validate the shared UI save, retaining its warning/recovery behavior.
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
    // Persist the validated full browser profile through the desktop save bridge.
    public static void Save(string json,string? path=null)
    {
        Validate(json);path??=JsonProfileStore.DefaultPath;Directory.CreateDirectory(Path.GetDirectoryName(path)!);string temp=path+".tmp";File.WriteAllText(temp,json);
        if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
    }
}
