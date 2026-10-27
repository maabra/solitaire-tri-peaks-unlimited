namespace MeadowPeaks;

public interface IProfileStore
{
    // Load a detached profile and report recovery problems through the warning output.
    Progress Load(out string warning);
    // Persist the complete supplied profile through the selected storage implementation.
    void Save(Progress profile);
}

public interface IGameClock { DateTime Today { get; } DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
public interface IRandomSource { /* Supply a seed through the injected random source so game logic need not own global randomness. */
    int NextSeed(); }

public record WinContext(int BestStreak, int StockRemaining, bool Replay);
public record WinReward(int Stars, int Coins, int ChapterBonus = 0, string? ChapterName = null, int BonusCoins = 0, string? BonusItem = null);

public interface IEconomyPolicy
{
    int UndoCost { get; }
    int DailyGift { get; }
    int GardenGift { get; }
    int GardenInterval { get; }
    int ChapterGift => 150;
    int ChapterWilds => 1;
    // Calculate a win payout from its context without directly crediting the wallet.
    WinReward Reward(WinContext context);
}

public interface IPowerup
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    int Price { get; }
    bool RequiresTarget { get; }
    // Check helper eligibility without consuming inventory or changing the hand.
    bool CanUse(ISolitaireHand hand);
    // Apply this helper only to a valid hand/target; the session owns inventory and payment.
    bool Apply(ISolitaireHand hand, int? target, int seed);
}

public enum CommandResult { Applied, Unavailable, InsufficientCoins, MissingItem }
