using System.Security.Cryptography;
using System.Text;

namespace MeadowPeaks;
public sealed record DealReference(int Level, int Seed, bool Endless, string Generation = "MP16");
public static class DealCodes
{
    const string Protocol = "MP16";
    public static string Encode(DealReference deal)
    {
        if (deal.Level<1 || deal.Level>=int.MaxValue || deal.Seed<0 || !deal.Endless && deal.Level>MeadowCampaign.TotalLevels) throw new ArgumentOutOfRangeException(nameof(deal));
        if(deal.Generation is not ("MP16" or "MP18"))throw new ArgumentException("Unknown deal generation.");
        string body=$"{deal.Generation}-{(deal.Endless ? "E" : "C")}-{deal.Level}-{deal.Seed}";
        return body+"-"+Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(body))).Substring(0,8);
    }
    public static bool TryParse(string? code, out DealReference? deal)
    {
        deal=null; if(code==null || code.Length>80) return false;
        var parts=code.Trim().ToUpperInvariant().Split('-');
        if(parts.Length!=5 || parts[0] is not ("MP16" or "MP18") || parts[1] is not ("C" or "E") || !int.TryParse(parts[2],out int level) || !int.TryParse(parts[3],out int seed) || level<1 || level>=int.MaxValue || seed<0 || parts[1]=="C" && level>MeadowCampaign.TotalLevels) return false;
        var parsed=new DealReference(level,seed,parts[1]=="E",parts[0]);
        if(Encode(parsed)!=string.Join("-",parts)) return false;
        deal=parsed; return true;
    }
}
