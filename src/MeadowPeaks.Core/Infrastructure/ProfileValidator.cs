namespace MeadowPeaks;

internal static class ProfileValidator
{
    public static bool Valid(Progress? p) => p != null && p.Version == 1 && p.Coins >= 0 && p.Coins <= 100000000 && p.Unlocked >= 1 && p.Unlocked <= 2000 && p.EndlessDeal >= 1 && p.EndlessWins >= 0 && p.Items != null && p.Items.All(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value >= 0) && p.Stars != null && p.Scores != null && p.Stars.All(kv => kv.Key >= 1 && kv.Key <= 2000 && kv.Value >= 1 && kv.Value <= 3) && p.Scores.All(kv => kv.Key >= 1 && kv.Key <= 2000 && kv.Value >= 0) && p.Wilds >= 0 && p.Extras >= 0 && p.Hammers >= 0 && p.Garden >= 0 && p.GardenClaimed >= 0 && p.GardenClaimed <= p.Garden;
}
