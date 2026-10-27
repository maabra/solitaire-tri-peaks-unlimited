namespace MeadowPeaks;

public record Chapter(int Number, string Name, string World, string Invitation, int Theme, int FirstLevel, int LastLevel);

public interface ICampaignCatalog
{
    int LevelCount { get; }
    int LevelsPerChapter { get; }
    IReadOnlyList<Chapter> Chapters { get; }
    // Resolve the chapter containing a supported campaign level.
    Chapter ForLevel(int level);
}

public sealed class MeadowCampaign : ICampaignCatalog
{
    public const int TotalLevels = Content.CampaignContent.TotalLevels;
    public int LevelCount => TotalLevels;
    public int LevelsPerChapter => Content.CampaignContent.LevelsPerChapter;
    public IReadOnlyList<Chapter> Chapters { get; }
    // Map a level into the bounded original chapter collection.
    public Chapter ForLevel(int level) => Chapters[Math.Clamp((level - 1) / LevelsPerChapter, 0, Chapters.Count - 1)];

    // Build chapter descriptors from the canonical titles and illustrated campaign cap.
    public MeadowCampaign()
    {
        string[] worlds = { "Willow Meadow", "Moonlit Lake", "Amber Orchard", "Snowdrop Valley", "Fernwood Trail", "Cloudberry Hills", "Lavender Coast", "Golden Grove", "Starlight Garden", "Homeward Peaks", "Cherryblossom Bay", "Sunflower Fields", "Coral Cove", "Firefly Forest", "Peach Picnic", "Rainbow Springs", "Bluebell Woods", "Honeycomb Hollow", "Aurora Ridge", "Celebration Island" };
        string[] invitations = { "Every little step is something lovely.", "Follow a butterfly. Find a little wonder.", "Make room for a small, happy moment.", "A little sunshine goes a long way.", "The meadow saved you a beautiful seat.", "Let your worries drift away like petals.", "Your next adventure begins with one card.", "Plant a wish and watch it grow.", "Wonderful things come one card at a time.", "Look how far your little journey has grown!" };
        Chapters = Array.AsReadOnly(Enumerable.Range(0, Content.CampaignContent.ChapterCount).Select(i =>
            new Chapter(i + 1, Content.CampaignContent.ChapterTitles[i], Content.CampaignContent.ChapterTitles[i],
                invitations[i % invitations.Length], i % worlds.Length,
                i * LevelsPerChapter + 1, (i + 1) * LevelsPerChapter)).ToArray());
    }
}
