namespace MeadowPeaks;

public sealed class TriPeaksRules : ISolitaireRules
{
    public bool Exposed(IReadOnlyList<Slot> layout, IReadOnlyList<bool> removed, int i) => !removed[i] &&
        !layout.Where((s, j) => !removed[j] && s.Row == layout[i].Row + 1 && Math.Abs(s.X - layout[i].X) < .6f).Any();
    public bool Adjacent(int a, int b) => a == 0 || b == 0 || Math.Abs(a - b) == 1 || Math.Abs(a - b) == 12;

}
