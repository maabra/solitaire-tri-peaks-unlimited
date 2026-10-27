namespace MeadowPeaks;

// On-demand combinatorial formations with asymmetric anchors and 4-8 rows.
// Every campaign index selects a different occupancy pattern.
public sealed class AdventureLayouts : ILayoutProvider
{
    public const int PatternCount = 480000;
    public static readonly string[] Names = {
        "Butterfly", "Crown", "Moon bridge", "Diamond", "Ribbon", "Twin gardens", "Three towers", "Waterfall",
        "Sunrise", "Crescent", "Wings", "Valley", "Castle", "Lanterns", "Wave", "Stepping stones",
        "Lotus", "Mountain pass", "Windmill", "Star trail", "Honeycomb", "River bend", "Orchard", "Cloud bank",
        "Archway", "Firefly trail", "Coral reef", "Snowflake", "Flower basket", "Rolling hills", "Kite", "Festival"
    };
    // Choose a deterministic layout family and compile its explicit covering relationships.
    public List<Slot> Create(int level)
    {
        if (level < 5) return new PeakLayouts().Create(level);
        int depth = DepthFor(level), index = Index(level);
        // A combinatorial rank selects 11 of 22 optional positions. The fixed
        // asymmetric anchors prevent both translated and mirrored duplicates.
        // 22 choose 11 = 705,432: enough for a one-to-one campaign permutation.
        var fixedCells = new HashSet<int>();
        for (int row = 0; row < depth; row++) { fixedCells.Add(row * 10 + 4); fixedCells.Add(row * 10 + 5); }
        fixedCells.UnionWith(new[] { 0, 1, 10, (depth-1)*10, (depth-1)*10+9 });
        var optional = Enumerable.Range(0, depth*10).Where(c => !fixedCells.Contains(c) && c != 9)
            .OrderBy(c => Math.Abs(c % 10 - 4.5) + (c * 37 % 11) * .35)
            .ThenBy(c => c).Take(22).OrderBy(c => c).ToArray();
        long rank = (index * 48271L + 12345) % Choose(22,11);
        int remaining = 11;
        for (int n = 0; n < optional.Length && remaining > 0; n++)
        {
            long ways = Choose(optional.Length - n - 1, remaining - 1);
            if (rank < ways) { fixedCells.Add(optional[n]); remaining--; }
            else rank -= ways;
        }
        return fixedCells.OrderBy(c => c).Select(c => new Slot(.5f + c%10 + (c/10%2)*.5f, c/10)).ToList();
    }
    // Calculate the binomial count used to rank deterministic occupancy combinations.
    static long Choose(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        long value = 1;
        for (int j = 1; j <= k; j++) value = value * (n-j+1) / j;
        return value;
    }
    // Map the level into its retained deterministic layout-pattern index.
    static int Index(int level) => (int)(((long)Math.Max(5, level) - 5) % PatternCount);
    // Choose the supported formation depth for this part of the campaign.
    public static int DepthFor(int level) => level < 300 ? 4 : 4 + (level - 5) % (level < 700 ? 2 : level < 1300 ? 3 : level < 2100 ? 4 : 5);
    // Name the selected formation family and deterministic variation.
    public static string NameFor(int level) => level < 5 ? "Classic peaks" : $"{Names[Index(level) % Names.Length]} {Index(level) / Names.Length + 1}";

    // Produce a stable geometry signature for uniqueness and regression checks.
    public static string Fingerprint(IEnumerable<Slot> slots)
    {
        var points = slots.Select(s => (X: (int)Math.Round(s.X * 4), s.Row)).ToArray();
        // Normalize the formation coordinates used in geometry comparison.
        string Key(bool mirror)
        {
            int min = points.Min(p => mirror ? -p.X : p.X);
            return string.Join(";", points.Select(p => (X: (mirror ? -p.X : p.X) - min, p.Row)).OrderBy(p => p.Row).ThenBy(p => p.X).Select(p => $"{p.X},{p.Row}"));
        }
        var forward = Key(false); var reverse = Key(true);
        return string.CompareOrdinal(forward, reverse) < 0 ? forward : reverse;
    }
}
