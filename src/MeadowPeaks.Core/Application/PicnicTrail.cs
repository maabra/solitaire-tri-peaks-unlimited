namespace MeadowPeaks;

public sealed class PicnicProgress
{
    public string Day { get; set; } = "";
    public int Wins { get; set; }
    public int Clears { get; set; }
    public int GemCoins { get; set; }
    public int Claimed { get; set; }
}
public sealed record PicnicGoal(string Id, string Icon, int Current, int Target, int Coins, bool Claimed)
{
    public bool Ready => Current>=Target && !Claimed;
}
public interface IPicnicPolicy { /* Describe daily goals and their progress from the supplied picnic counters. */
    IReadOnlyList<PicnicGoal> Goals(PicnicProgress progress); }
public sealed class StandardPicnicPolicy : IPicnicPolicy
{
    // Build the standard daily goals from today's recorded wins, clears and jewel earnings.
    public IReadOnlyList<PicnicGoal> Goals(PicnicProgress p) => new[] {
        new PicnicGoal("wins","\U0001f3c6",p.Wins,2,40,(p.Claimed&1)!=0),
        new PicnicGoal("clears","\u2660",p.Clears,24,50,(p.Claimed&2)!=0),
        new PicnicGoal("gems","\u25c6",p.GemCoins,80,60,(p.Claimed&4)!=0) };
}
public sealed class PicnicTrail
{
    readonly Progress profile; readonly IGameClock clock; readonly Wallet wallet;
    readonly PowerupInventory inventory; readonly PowerupCatalog helpers; readonly IPicnicPolicy policy;
    // Bind daily task policy to the shared clock, wallet and helper inventory.
    public PicnicTrail(Progress profile, IGameClock clock, Wallet wallet, PowerupInventory inventory, PowerupCatalog helpers, IPicnicPolicy? policy=null)
    { this.profile=profile; this.clock=clock; this.wallet=wallet; this.inventory=inventory; this.helpers=helpers; this.policy=policy ?? new StandardPicnicPolicy();
        var goals=this.policy.Goals(new());
        if(goals.Count!=3 || goals.Select(g=>g.Id).Distinct().Count()!=3 || goals.Any(g=>string.IsNullOrWhiteSpace(g.Id) || g.Target<=0 || g.Coins<=0)) throw new ArgumentException("Picnic policies need three stable, positive goals.",nameof(policy));
    }
    // Reset daily task counters only when the injected local calendar day changes.
    void Today() { string day=clock.Today.ToString("yyyy-MM-dd"); if(profile.Picnic.Day!=day) profile.Picnic=new PicnicProgress { Day=day }; }
    public IReadOnlyList<PicnicGoal> Goals { get { Today(); return policy.Goals(profile.Picnic); } }
    // Record a completed hand toward today's daily goals.
    public void Win() { Today(); profile.Picnic.Wins=Math.Min(1000000,profile.Picnic.Wins+1); }
    // Record accepted card clears toward today's daily goals.
    public void Clear(int count) { Today(); profile.Picnic.Clears=Math.Min(1000000,profile.Picnic.Clears+Math.Max(0,count)); }
    // Record coins actually earned from accepted jewel moves.
    public void Gems(int coins) { Today(); profile.Picnic.GemCoins=(int)Math.Min(1000000,(long)profile.Picnic.GemCoins+Math.Max(0,coins)); }
    public string? LastGift { get; private set; }
    // Validate goal completion and save its claimed marker before allowing another claim.
    public int Claim(string id)
    {
        LastGift=null;
        var goals=Goals; int index=goals.ToList().FindIndex(g=>g.Id==id);
        if(index<0 || !goals[index].Ready) return 0;
        var goal=goals[index]; wallet.Credit(goal.Coins); profile.Picnic.Claimed|=1<<index;
        if(Goals.All(g=>g.Claimed) && helpers.All.Count>0)
        {
            var useful=helpers.All.Where(p=>p.Id is "wild" or "extra" or "scout" or "lucky" or "peek").ToArray();
            var choices=useful.Length>0 ? useful : helpers.All.ToArray();
            LastGift=choices[(int)(clock.Today.Ticks/TimeSpan.TicksPerDay%choices.Length)].Id;
            inventory.Add(LastGift,1);
        }
        return goal.Coins;
    }
}
