namespace MeadowPeaks;

public interface IProfileStore
{
    Progress Load(out string warning);
    void Save(Progress profile);
}

public interface IGameClock { DateTime Today { get; } }
public interface IRandomSource { int NextSeed(); }

public record WinContext(int BestStreak, int StockRemaining, bool Replay);
public record WinReward(int Stars, int Coins, int ChapterBonus = 0, string? ChapterName = null);

public interface IEconomyPolicy
{
    int UndoCost { get; }
    int DailyGift { get; }
    int GardenGift { get; }
    int GardenInterval { get; }
    int ChapterGift => 150;
    int ChapterWilds => 1;
    WinReward Reward(WinContext context);
}

public interface IPowerup
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    int Price { get; }
    bool RequiresTarget { get; }
    bool CanUse(ISolitaireHand hand);
    bool Apply(ISolitaireHand hand, int? target, int seed);
}

public enum CommandResult { Applied, Unavailable, InsufficientCoins, MissingItem }
