namespace MeadowPeaks;

public sealed class SeededDealProvider : IDealProvider
{
    readonly ILayoutProvider layouts;
    readonly ISolitaireRules rules;
    // Bind the replaceable layout and adjacency policies used to construct solvable deals.
    public SeededDealProvider(ILayoutProvider layouts, ISolitaireRules rules)
    {
        this.layouts = layouts;
        this.rules = rules;
    }
    // Construct a legal removal path first, then fill it with a unique 52-card deck.
    // Every published seed carries a witness solution, verified by the test runner.
    public Deal Generate(int seed, int level)
    {
        var rng = new Random(seed);
        var layout = layouts.Create(level);
        for (int attempt = 0; attempt < 10000; attempt++)
        {
            var available = Enumerable.Range(0, 52).Select(i => new Card(i % 13 + 1, i / 13)).ToList();
            // Choose and remove a card from the remaining deck so the same physical card is not dealt twice.
            Card Take(List<Card> choices) { var c = choices[rng.Next(choices.Count)]; available.Remove(c); return c; }
            var waste = Take(available.ToList());
            var current = waste;
            var cards = new Card[layout.Count];
            var removed = new bool[layout.Count];
            var stock = new List<Card>();
            var solution = new List<Step>();
            bool failed = false;
            int interval = level < 10 ? 8 : level < 100 ? 6 : 4;
            for (int n = 0; n < cards.Length; n++)
            {
                if (n > 0 && n % interval == 0)
                {
                    current = Take(available.ToList()); stock.Add(current); solution.Add(new(-1));
                }
                var choices = available.Where(c => rules.Adjacent(c.Rank, current.Rank)).ToList();
                if (choices.Count == 0) { failed = true; break; }
                var exposed = Enumerable.Range(0, cards.Length).Where(i => rules.Exposed(layout, removed, i)).ToArray();
                int index = exposed[rng.Next(exposed.Length)];
                current = Take(choices); cards[index] = current; removed[index] = true; solution.Add(new(index));
            }
            if (failed) continue;
            // Remaining stock is shuffled, after the draws used by the guaranteed path.
            while (available.Count > 0) stock.Add(Take(available.ToList()));
            return new() { Layout = layout, Cards = cards.ToList(), Stock = stock, Waste = waste, Solution = solution };
        }
        throw new InvalidOperationException("Could not construct deal.");
    }
}
