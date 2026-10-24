namespace MeadowPeaks;

internal static class ProfileValidator
{
    public static bool Valid(Progress? p) => p != null && p.Version == 1 && p.Coins >= 0 && p.Unlocked >= 1 && p.Unlocked <= MeadowCampaign.TotalLevels && p.EndlessDeal >= 1 && p.EndlessWins >= 0 && p.ChapterGifts != null && p.ChapterGifts.All(n => n >= 1 && n <= MeadowCampaign.TotalLevels / 20) && p.Items != null && p.Items.All(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value >= 0) && p.Stars != null && p.Scores != null && p.Stars.All(kv => kv.Key >= 1 && kv.Key <= MeadowCampaign.TotalLevels && kv.Value >= 1 && kv.Value <= 3) && p.Scores.All(kv => kv.Key >= 1 && kv.Key <= MeadowCampaign.TotalLevels && kv.Value >= 0) && p.Wilds >= 0 && p.Extras >= 0 && p.Hammers >= 0 && p.Garden >= 0 && p.GardenClaimed >= 0 && p.GardenClaimed <= p.Garden;
}
