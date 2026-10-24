namespace MeadowPeaks;

public sealed class StandardEconomyPolicy : IEconomyPolicy
{
    public int UndoCost => 15;
    public int DailyGift => 75;
    public int GardenGift => 80;
    public int GardenInterval => 5;
    public int ChapterGift => 150;
    public int ChapterWilds => 1;

    public WinReward Reward(WinContext context)
    {
        int stars = context.BestStreak >= 9 ? 3 : context.BestStreak >= 5 ? 2 : 1;
        return new(stars, (context.Replay ? 30 : 100) + context.StockRemaining * 2 + stars * 10);
    }
}

public sealed class Wallet
{
    readonly Progress profile;
    public Wallet(Progress profile) { this.profile = profile; }
    public int Balance => profile.Coins;
    public bool Spend(int amount)
    {
        if (amount < 0 || Balance < amount) return false;
        profile.Coins -= amount;
        return true;
    }
    public void Credit(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        profile.Coins = checked(profile.Coins + amount);
    }
}

public sealed class PowerupInventory
{
    readonly Progress profile;
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

    public void Add(string id, int amount)
    {
        int count = checked(Count(id) + amount);
        if (count < 0) throw new InvalidOperationException("Cannot consume an unavailable helper.");
        switch (id)
        {
            case "wild": profile.Wilds = count; break;
            case "extra": profile.Extras = count; break;
            case "remove": profile.Hammers = count; break;
            default: profile.Items[id] = count; break;
        }
    }
}
