using MeadowPeaks;

var rules = new TriPeaksRules();
var deals = new SeededDealProvider(new PeakLayouts(), rules);
var hands = new SolitaireFactory(deals, rules, deals);
int assertions = 0;
void Check(bool condition, string description)
{
    assertions++; if (!condition) throw new Exception(description);
}
Check(rules.Adjacent(1, 13) && rules.Adjacent(13, 1), "Ace King wrap");
Check(rules.Adjacent(1, 2) && !rules.Adjacent(2, 4), "Rank adjacency");
Check(rules.Adjacent(0, 8), "Wild adjacency");
var fingerprints = new HashSet<string>();
var campaignBaseline = new System.Text.StringBuilder();
var watch = System.Diagnostics.Stopwatch.StartNew();
for (int level = 1; level <= MeadowCampaign.TotalLevels; level++)
{
    int seed = 104729 + level * 997;
    var deal = deals.Generate(seed, level);
    if (level <= 2000)
    {
    foreach (var c in deal.Cards.Concat(deal.Stock).Append(deal.Waste)) campaignBaseline.Append(c.Rank).Append(':').Append(c.Suit).Append(',');
    foreach (var slot in deal.Layout) campaignBaseline.Append(slot.X.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(':').Append(slot.Row).Append(',');
    campaignBaseline.Append('\n');
    }
    Check(deal.Cards.Concat(deal.Stock).Append(deal.Waste).Distinct().Count() == 52, $"Level {level}: unique deck");
    Check(deal.Cards.Count + deal.Stock.Count + 1 == 52, $"Level {level}: complete deck");
    Check(fingerprints.Add(string.Join(",", deal.Cards.Select(c => c.Rank * 4 + c.Suit))), $"Level {level}: distinct deal");
    var replay = deals.Generate(seed, level);
    Check(deal.Cards.SequenceEqual(replay.Cards) && deal.Stock.SequenceEqual(replay.Stock), "Determinism");
    var hand = new Hand(deal, rules);
    int startCovered = Enumerable.Range(0, hand.Removed.Count).First(i => !rules.Exposed(deal.Layout, hand.Removed, i));
    Check(!hand.Play(startCovered), "Covered card cannot play");
    foreach (var step in deal.Solution)
        Check(step.Index == -1 ? hand.Draw() : hand.Play(step.Index), $"Level {level}: legal solution step {step.Index}");
    Check(hand.Won, $"Level {level}: solvable");
    Check(!hand.Draw() && !hand.Wild() && !hand.Remove(0), "Finished hand rejects mutations");
}
// Recorded from the shipped, pre-refactor generator, including every stock order.
string baselineHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(campaignBaseline.ToString())));
Check(baselineHash == "9E4489B3ACA9B8CA930885ED018778E066801FCC75E9B0FDC80E065FEBEF7159", "All campaign boards and stock orders match the original release");
Console.WriteLine($"All 4,000 campaign deals are distinct, deterministic, use 52 unique cards and have legal winning solutions ({watch.ElapsedMilliseconds}ms).");
for (int n = 1; n <= 100; n++)
{
    var h = new Hand(deals.Generate(170003 + n * 7919, n), rules);
    foreach (var s in h.Deal.Solution) Check(s.Index < 0 ? h.Draw() : h.Play(s.Index), "Endless legal solution");
    Check(h.Won, "Endless clears");
}
{
    var h = new Hand(deals.Generate(105726, 1), rules);
    var original = h.Waste; var move = h.Moves.First();
    Check(h.Play(move), "Play exposed move"); Check(h.Undo(), "Undo play");
    Check(h.Waste == original && !h.Removed[move] && h.Score == 0 && h.FreeUndos == 2, "Undo restores state");
    int count = h.Stock.Count; Check(h.Draw() && h.Undo() && h.Stock.Count == count, "Undo draw");
    Check(!h.Extra(12), "Cannot add cards to nonempty stock");
    Check(h.Wild() && !h.CanUndo && h.Moves.Any(), "Wild clears history and permits moves");
    Check(!h.Wild(), "Cannot waste repeated Wild");
    Check(h.Play(h.Moves.First()) && h.Undo() && h.Waste.Rank == 0, "Undo after wild");
    while (h.Draw()) { }
    Check(h.Extra(44) && h.Stock.Count == 5 && !h.CanUndo, "Extra stock");
    int exposed = Enumerable.Range(0, h.Removed.Count).First(i => rules.Exposed(h.Deal.Layout, h.Removed, i));
    Check(h.Remove(exposed) && h.Removed[exposed] && !h.CanUndo, "Targeted remove");
    var restored = hands.Restore(h.Capture(105726, 1, false));
    Check(restored != null && restored.Stock.SequenceEqual(h.Stock) && restored.Removed.SequenceEqual(h.Removed) && restored.Waste == h.Waste, "In-progress restore");
    Check(hands.Restore(h.Capture(105726, 1, false) with { Stock = new List<Card> { null! } }) == null, "Reject corrupt active hand");
}
for (int seed = 0; seed < 100; seed++)
{
    var gems = new GemGame(seed);
    Check(gems.Matches().Count == 0 && gems.Hint() != null, "Playable initial gem board");
    Check(!gems.Play(7, 8) && gems.Moves == 20, "No swapping across row boundary");
    Check(!gems.Play(-1, 5), "Invalid gem coordinate");
    for (int move = 0; move < 20; move++)
    {
        var hint = gems.Hint(); Check(hint != null, "Always a legal gem move");
        var pair = hint!.Value;
        int before = gems.Earned;
        Check(gems.Play(pair.Item1, pair.Item2), "Hint makes valid match");
        Check(gems.LastAnimation.First().Kind == GemFrameKind.Swap && gems.LastAnimation.Last().Kind == GemFrameKind.Drop, "Visual history starts at swap and ends after a cascade");
        Check(gems.LastAnimation.Where(f => f.Kind == GemFrameKind.Match).Sum(f => f.Matched.Count * 2 * f.Cascade) == gems.LastGain, "Animated cascade rewards agree with wallet payout");
        Check(gems.LastAnimation.All(f => f.Cells.Count == 64 && f.Cells.All(c => c >= 0 && c < 6)), "Animation frames contain independent valid boards");
        Check(gems.Earned > before && gems.LastGain >= 6 && gems.Moves == 19 - move, "Gem payouts and move accounting");
        Check(gems.Matches().Count == 0 && gems.Cells.All(c => c >= 0 && c < 6), "Cascades fully resolved");
    }
    Check(!gems.Play(0, 1) && gems.Moves == 0, "Finished gem round locked");
}
Console.WriteLine("100 full match-three rounds passed: matching, cascades, hints, moves, payouts and dead-board recovery.");
string dir = Path.Combine(Path.GetTempPath(), "MeadowPeaks-test-" + Guid.NewGuid());
Directory.CreateDirectory(dir);
try
{
    string file = Path.Combine(dir, "progress.json");
    var p = new Progress(); var wallet = new Wallet(p); var store = new JsonProfileStore(file); Check(!wallet.Spend(1000) && p.Coins == 300, "Insufficient coins");
    Check(!wallet.Spend(-1) && wallet.Spend(75) && p.Coins == 225, "Wallet deductions");
    p.Unlocked = 101; p.Stars[100] = 3; store.Save(p); p.Coins = 190; store.Save(p);
    var loaded = store.Load(out var warning);
    Check(loaded.Coins == 190 && loaded.Unlocked == 101 && loaded.Stars[100] == 3 && warning == "", "Save roundtrip");
    File.WriteAllText(file, "broken save");
    loaded = store.Load(out warning);
    Check(loaded.Coins == 225 && warning.Contains("backup"), "Backup recovery");
    store.Save(loaded); Check(File.Exists(file + ".bak"), "Recovered save retains backup");
    File.WriteAllText(file, "{}"); loaded = store.Load(out warning); Check(loaded.Unlocked == 1, "Partial profile defaults");
    loaded.Garden = 5; loaded.GardenClaimed = 1; store.Save(loaded);
    loaded = store.Load(out warning); Check(loaded.Garden == 5 && loaded.GardenClaimed == 1 && warning == "", "Garden gift survives reload");
    File.WriteAllText(file, "{\"Coins\":-2}"); loaded = store.Load(out warning); Check(loaded.Coins >= 0 && warning.Contains("backup"), "Invalid wallet recovers backup");
}
finally { Directory.Delete(dir, true); }
ComponentSwapTests.Run(Check);
AdventureTests.Run(Check);
var campaign = new MeadowCampaign();
Check(campaign.Chapters.Count == 200 && campaign.LevelCount == 4000 && campaign.Chapters.Select(c => c.World).Distinct().Count() == 20, "Expanded campaign has 200 chapters and 20 worlds");
Check(campaign.Chapters.SelectMany(c => Enumerable.Range(c.FirstLevel, c.LastLevel - c.FirstLevel + 1)).SequenceEqual(Enumerable.Range(1, 4000)), "Chapters cover every level once without gaps");
Check(campaign.ForLevel(4000).Number == 200 && campaign.ForLevel(2001).Theme == 10, "New worlds and final chapter can be reached");
Console.WriteLine($"PASS: {assertions:N0} assertions. Gameplay, persistence, economy and component replacements verified.");
