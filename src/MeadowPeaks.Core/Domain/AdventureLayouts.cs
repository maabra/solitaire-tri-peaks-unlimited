namespace MeadowPeaks;

// A catalog of actual occupancy patterns, rather than offsets of a few templates.
// Mirrored and horizontally translated duplicates are rejected at catalog creation.
public sealed class AdventureLayouts : ILayoutProvider
{
    public const int PatternCount = 4096;
    public static readonly string[] Names = {
        "Butterfly", "Crown", "Moon bridge", "Diamond", "Ribbon", "Twin gardens", "Three towers", "Waterfall",
        "Sunrise", "Crescent", "Wings", "Valley", "Castle", "Lanterns", "Wave", "Stepping stones",
        "Lotus", "Mountain pass", "Windmill", "Star trail", "Honeycomb", "River bend", "Orchard", "Cloud bank",
        "Archway", "Firefly trail", "Coral reef", "Snowflake", "Flower basket", "Rolling hills", "Kite", "Festival"
    };
    static readonly Lazy<IReadOnlyList<Slot[]>> catalog = new(BuildCatalog);
    public List<Slot> Create(int level) => level < 5 ? new PeakLayouts().Create(level) : catalog.Value[Index(level)].ToList();
    static int Index(int level) => (int)(((long)Math.Max(5, level) - 5) % PatternCount);
    public static string NameFor(int level) => level < 5 ? "Classic peaks" : $"{Names[Index(level) % Names.Length]} {Index(level) / Names.Length + 1}";

    static Slot Position(int cell) => new(.5f + cell % 10 + (cell / 10 % 2) * .5f, cell / 10);
    static bool Neighbors(int a, int b)
    {
        var x = Position(a); var y = Position(b);
        return x.Row == y.Row ? Math.Abs(x.X - y.X) < 1.01f : Math.Abs(x.Row - y.Row) == 1 && Math.Abs(x.X - y.X) < .6f;
    }
    static readonly int[][] neighbors = Enumerable.Range(0, 40).Select(a => Enumerable.Range(0, 40).Where(b => a != b && Neighbors(a, b)).ToArray()).ToArray();
    static double Weight(int cell, int family)
    {
        var p = Position(cell); double x = (p.X - 5) / 4.5, y = p.Row / 3.0;
        // Different envelopes bias growth toward arches, lobes, branching paths,
        // diamonds and asymmetric terraces. Growth itself supplies holes and forks.
        double target = (family % 8) switch
        {
            0 => Math.Abs(x) * .8,
            1 => 1 - Math.Abs(x),
            2 => .25 + .65 * x * x,
            3 => .5 - Math.Abs(Math.Abs(x) - .5),
            4 => .5 + Math.Sin(x * 5 + family) * .35,
            5 => (x + 1) / 2,
            6 => .5 + Math.Cos(x * 9) * .4,
            _ => .5
        };
        double distance = Math.Abs(y - target);
        double lobes = (family / 8) switch { 0 => 1, 1 => .45 + Math.Abs(x), 2 => 1.45 - Math.Abs(x), _ => .7 + .6 * (x + 1) / 2 };
        return .15 + Math.Exp(-distance * 3.5) * lobes;
    }
    static IReadOnlyList<Slot[]> BuildCatalog()
    {
        var result = new List<Slot[]>(); var seen = new HashSet<string>();
        var rng = new Random(0x5045414B);
        for (int attempt = 0; result.Count < PatternCount && attempt < 500000; attempt++)
        {
            int family = result.Count % Names.Length;
            int target = rng.Next(18, 31);
            var cells = new HashSet<int> { rng.Next(40) };
            while (cells.Count < target)
            {
                var frontier = cells.SelectMany(c => neighbors[c]).Where(c => !cells.Contains(c)).Distinct().OrderBy(c => c).ToArray();
                double total = frontier.Sum(c => Weight(c, family)), pick = rng.NextDouble() * total;
                int chosen = frontier[^1];
                foreach (int c in frontier) { pick -= Weight(c, family); if (pick <= 0) { chosen = c; break; } }
                cells.Add(chosen);
            }
            var slots = cells.OrderBy(c => c).Select(Position).ToArray();
            if (Enumerable.Range(0, 4).Any(r => slots.Count(s => s.Row == r) < 2)) continue;
            int upper = slots.Count(s => s.Row < 3);
            int covered = slots.Count(s => s.Row < 3 && slots.Any(t => t.Row == s.Row + 1 && Math.Abs(t.X - s.X) < .6f));
            if (covered < upper * .65 || slots.Count(s => !slots.Any(t => t.Row == s.Row + 1 && Math.Abs(t.X - s.X) < .6f)) > 13) continue;
            string fingerprint = Fingerprint(slots);
            if (!seen.Add(fingerprint)) continue;
            result.Add(slots);
        }
        if (result.Count != PatternCount) throw new InvalidOperationException("Formation catalog could not be completed.");
        return result;
    }
    public static string Fingerprint(IEnumerable<Slot> slots)
    {
        var points = slots.Select(s => (X: (int)Math.Round(s.X * 2), s.Row)).ToArray();
        string Key(bool mirror)
        {
            int min = points.Min(p => mirror ? -p.X : p.X);
            return string.Join(";", points.Select(p => (X: (mirror ? -p.X : p.X) - min, p.Row)).OrderBy(p => p.Row).ThenBy(p => p.X).Select(p => $"{p.X},{p.Row}"));
        }
        var forward = Key(false); var reverse = Key(true);
        return string.CompareOrdinal(forward, reverse) < 0 ? forward : reverse;
    }
}
