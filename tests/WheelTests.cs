using MeadowPeaks;
using System.Text.Json;
static class WheelTests
{
    public static void Run(Action<bool,string> check)
    {
        for(int i=0;i<8;i++)
        {
            var p=new Progress();var wallet=new Wallet(p);var bag=new PowerupInventory(p);var wheel=new PrizeWheel(p,wallet,bag,new Seeds(i));
            check(wheel.Spins==1,"Welcome wheel spin");var prize=wheel.Spin()!;
            check(prize.Index==i&&p.WheelSpinsUsed==1,"Selected segment consumes exactly one earned spin");
            check(wallet.Balance==300+prize.Coins,"Wheel coins match selected prize");
            if(prize.Item!=null)check(bag.Count(prize.Item)==(prize.Item=="remove"?2:3),"Wheel consumable credited once");
            int coins=wallet.Balance;check(wheel.Spin()==null&&wallet.Balance==coins,"Empty wheel cannot award a second prize");
            string file=Path.Combine(Path.GetTempPath(),"MeadowPeaks-wheel-"+Guid.NewGuid()+".json");
            var store=new JsonProfileStore(file);store.Save(p);var loaded=store.Load(out var warning);File.Delete(file);
            check(warning.Length==0&&loaded.WheelSpinsUsed==1,"Wheel counters survive validated save restore");
            var reopened=new PrizeWheel(loaded,new Wallet(loaded),new PowerupInventory(loaded),new Seeds(i));
            check(reopened.Spins==0&&reopened.Spin()==null,"Restart cannot reclaim welcome spin");
            loaded.Garden=4;check(reopened.Spins==0,"Four wins do not grant a spin");loaded.Garden=5;check(reopened.Spins==1&&reopened.Spin()!=null,"Five wins grant one spin");
            check(reopened.Spins==0,"Earned spin is consumed");
        }
        string invalid=Path.Combine(Path.GetTempPath(),"MeadowPeaks-wheel-"+Guid.NewGuid()+".json");
        File.WriteAllText(invalid,JsonSerializer.Serialize(new Progress{WheelSpinsUsed=2}));
        new JsonProfileStore(invalid).Load(out var invalidWarning);File.Delete(invalid);
        check(invalidWarning.Length>0,"Reject more consumed spins than earned");
        Console.WriteLine("PASS: all wheel prizes, earned spins, replay prevention and save restore.");
    }
    sealed class Seeds:IRandomSource {readonly int index;public Seeds(int index){this.index=index;}public int NextSeed()=>index;}
}
