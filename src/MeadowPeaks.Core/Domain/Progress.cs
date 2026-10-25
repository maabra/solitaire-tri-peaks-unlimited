namespace MeadowPeaks;

public sealed class Progress
{
    public int Hearts { get; set; } = 3;
    public int GemHeartProgress { get; set; }
    public int WheelSpinsUsed { get; set; }
    public double FreshDealCooldown { get; set; }
    public long GardenRefreshAt { get; set; }
    public GemRun? ActiveGarden { get; set; }
    public HashSet<CardKind> SeenMechanics { get; set; } = new();
    public int HighestPlayedLevel { get; set; }
    public PicnicProgress Picnic { get; set; } = new();
    public HashSet<int> Favorites { get; set; } = new();
    public int Version { get; set; } = 1;
    public int Coins { get; set; } = 300;
    public int Unlocked { get; set; } = 1;
    public Dictionary<int, int> Stars { get; set; } = new();
    public Dictionary<int, int> Scores { get; set; } = new();
    public int EndlessDeal { get; set; } = 1;
    public int EndlessWins { get; set; }
    public int Wilds { get; set; } = 2;
    public int Extras { get; set; } = 2;
    public int Hammers { get; set; } = 1;
    public bool Sound { get; set; }
    public bool Hints { get; set; } = true;
    public bool ReducedMotion { get; set; }
    public string Daily { get; set; } = "";
    public int Garden { get; set; }
    public int GardenClaimed { get; set; }
    public SavedRun? Active { get; set; }
    public Dictionary<string, int> Items { get; set; } = new();
    public HashSet<int> ChapterGifts { get; set; } = new();
}
