namespace MeadowPeaks;
public sealed record WheelPrize(int Index,string Label,int Coins=0,string? Item=null);
public interface IPrizeWheelPolicy { IReadOnlyList<WheelPrize> Prizes { get; } /* Select the deterministic option represented by the supplied seed or combination index. */
    WheelPrize Choose(int seed); }
public sealed class GardenWheelPolicy:IPrizeWheelPolicy
{
    public IReadOnlyList<WheelPrize> Prizes {get;}=Array.AsReadOnly(new[]{new WheelPrize(0,"25 coins",25),new WheelPrize(1,"1 Wild",Item:"wild"),new WheelPrize(2,"50 coins",50),new WheelPrize(3,"1 Fresh cards",Item:"extra"),new WheelPrize(4,"100 coins",100),new WheelPrize(5,"1 Remove",Item:"remove"),new WheelPrize(6,"75 coins",75),new WheelPrize(7,"40 coins",40)});
    // Select the deterministic option represented by the supplied seed or combination index.
    public WheelPrize Choose(int seed)=>Prizes[(int)((uint)seed%8)];
}
public sealed class PrizeWheel
{
    readonly Progress profile;readonly Wallet wallet;readonly PowerupInventory inventory;readonly IRandomSource random;readonly IPrizeWheelPolicy policy;
    // Bind prize selection to shared wallet, inventory and persisted spin consumption.
    public PrizeWheel(Progress profile,Wallet wallet,PowerupInventory inventory,IRandomSource random,IPrizeWheelPolicy? policy=null){this.profile=profile;this.wallet=wallet;this.inventory=inventory;this.random=random;this.policy=policy??new GardenWheelPolicy();}
    // Derive available spins from earned garden milestones and persisted consumption.
    // Reopening the wheel must not create another free spin.
    public int Spins=>Math.Max(0,1+profile.Garden/5-profile.WheelSpinsUsed);
    public IReadOnlyList<WheelPrize> Prizes=>policy.Prizes;
    // Consume an available spin and grant its chosen prize once.
    public WheelPrize? Spin()
    {
        if(Spins==0)return null;
        var prize=policy.Choose(random.NextSeed());
        if(prize.Coins<0||prize.Index<0||prize.Index>=policy.Prizes.Count)throw new InvalidOperationException("Invalid wheel prize.");
        if(prize.Coins>0)wallet.Credit(prize.Coins);if(prize.Item!=null)inventory.Add(prize.Item,1);
        profile.WheelSpinsUsed++;return prize;
    }
}
