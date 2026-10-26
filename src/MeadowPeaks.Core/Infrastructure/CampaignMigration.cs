namespace MeadowPeaks;

public static class CampaignMigration
{
    public static Progress Apply(Progress profile)
    {
        if (!ProfileValidator.Valid(profile, true)) throw new InvalidDataException("Invalid saved adventure.");
        int cap = MeadowCampaign.TotalLevels;
        if (profile.Unlocked > cap || profile.Stars.Keys.Any(n => n > cap) || profile.Scores.Keys.Any(n => n > cap))
            profile.LegacyCampaign ??= new(profile.Unlocked, new(profile.Stars), new(profile.Scores), new(profile.Favorites), new(profile.ChapterGifts));
        profile.Unlocked = Math.Min(cap, profile.Unlocked);
        profile.HighestPlayedLevel = Math.Min(cap, profile.HighestPlayedLevel);
        profile.Stars = profile.Stars.Where(kv => kv.Key <= cap).ToDictionary(kv => kv.Key, kv => kv.Value);
        profile.Scores = profile.Scores.Where(kv => kv.Key <= cap).ToDictionary(kv => kv.Key, kv => kv.Value);
        profile.Favorites.RemoveWhere(n => n > cap);
        profile.ChapterGifts.RemoveWhere(n => n > cap / 20);
        if (profile.Active is { Endless: false } run && run.Level > cap) profile.Active = run with { Endless = true };
        profile.CampaignEdition = 2;
        return profile;
    }
}
