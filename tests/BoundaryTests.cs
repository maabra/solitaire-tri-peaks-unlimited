using MeadowPeaks;
static class BoundaryTests
{
 public static void Run(Action<bool,string> check)
 {
  var profile=new Progress{Coins=int.MaxValue,Wilds=int.MaxValue};
  var wallet=new Wallet(profile);wallet.Credit(75);check(wallet.Balance==int.MaxValue,"Full wallet rewards saturate");
  var bag=new PowerupInventory(profile);bag.Add("wild",1);check(bag.Count("wild")==int.MaxValue,"Full helper rewards saturate");
  var rules=new TriPeaksRules();var deals=new SeededDealProvider(new PeakLayouts(),rules);
  var factory=new SolitaireFactory(deals,rules,deals);
  var session=new GameSession(factory,new GemGameFactory(),new MemoryProfileStore(profile),new StandardEconomyPolicy(),new PowerupCatalog(new IPowerup[]{new WildPowerup()}),new LocalGameClock(),new RandomSource());
  check(session.BuyPowerup("wild")==CommandResult.Unavailable&&session.Wallet.Balance==int.MaxValue,"Full helper purchase never spends coins");
  session.Profile.Garden=int.MaxValue;session.ClaimGarden();check(session.Wallet.Balance==int.MaxValue,"Large garden rewards remain bounded");
  var hand=new Hand(deals.Generate(12345,1),rules);var run=hand.Capture(12345,1,false) with{Score=int.MaxValue,Streak=int.MaxValue,BestStreak=int.MaxValue};
  var restored=factory.Restore(run)!;check(restored.Play(restored.Moves.First())&&restored.Score==int.MaxValue&&restored.Streak==int.MaxValue,"Restored maximum score and streak do not overflow");
 }
}

