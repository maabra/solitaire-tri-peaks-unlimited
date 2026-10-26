using MeadowPeaks;
static class BoundaryTests
{
 public static void Run(Action<bool,string> check)
 {
  var legacy = new Progress { Unlocked = 480000, Coins = 1873, HighestPlayedLevel = 480000 };
  legacy.Stars[1] = 3; legacy.Stars[480000] = 2; legacy.Scores[480000] = 99;
  var migrated = ProfileCodec.Decode(System.Text.Json.JsonSerializer.Serialize(legacy));
  check(migrated.Unlocked == 10000 && migrated.Coins == 1873 && migrated.Stars.Count == 1 && migrated.LegacyCampaign!.Stars[480000] == 2, "Old campaign save preserves wallet and archives retired progress");
  check(MeadowPeaks.Content.CampaignContent.ChapterTitles.Distinct().Count() == 100 && new MeadowCampaign().ForLevel(10000).LastLevel == 10000, "Every campaign chapter has original art and the final boundary is exact");
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

