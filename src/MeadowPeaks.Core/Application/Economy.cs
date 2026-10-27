namespace MeadowPeaks;

public sealed class StandardEconomyPolicy : IEconomyPolicy
{
    public int UndoCost => 15;
    public int DailyGift => 75;
    public int GardenGift => 80;
    public int GardenInterval => 5;
    public int ChapterGift => 150;
    public int ChapterWilds => 1;

    // Replay rewards are intentionally smaller than first-clear rewards. Star quality is
    // based on the best streak, so later draws do not erase an earlier strong run.
    public WinReward Reward(WinContext context)
    {
        int stars = context.BestStreak >= 9 ? 3 : context.BestStreak >= 5 ? 2 : 1;
        return new(stars, (context.Replay ? 30 : 100) + context.StockRemaining * 2 + stars * 10);
    }
}

public sealed class Wallet
{
    readonly Progress profile;
    // Bind wallet operations to the profile's saved balance.
    public Wallet(Progress profile) { this.profile = profile; }
    public int Balance => profile.Coins;
    // Reject invalid or unaffordable costs before deducting coins.
    public bool Spend(int amount)
    {
        if (amount < 0 || Balance < amount) return false;
        profile.Coins -= amount;
        return true;
    }
    // Add a non-negative reward while respecting the supported wallet cap.
    public void Credit(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        profile.Coins = (int)Math.Min(int.MaxValue, (long)profile.Coins + amount);
    }
}

public sealed class PowerupInventory
{
    readonly Progress profile;
    // Bind helper quantities to their saved profile representation.
    public PowerupInventory(Progress profile) { this.profile = profile; }

    // Keep the original save fields for backward compatibility. New item IDs
    // live in Items, so adding another strategy needs no new profile field.
    public int Count(string id) => id switch
    {
        "wild" => profile.Wilds,
        "extra" => profile.Extras,
        "remove" => profile.Hammers,
        _ => profile.Items.GetValueOrDefault(id)
    };

    // Apply a bounded inventory change without allowing negative or overflowing quantities.
    public void Add(string id, int amount)
    {
        long next = (long)Count(id) + amount;
        int count = (int)Math.Min(int.MaxValue, next);
        if (next < 0) throw new InvalidOperationException("Cannot consume an unavailable helper.");
        switch (id)
        {
            case "wild": profile.Wilds = count; break;
            case "extra": profile.Extras = count; break;
            case "remove": profile.Hammers = count; break;
            default: profile.Items[id] = count; break;
        }
    }
}
