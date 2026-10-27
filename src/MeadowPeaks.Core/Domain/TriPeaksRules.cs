namespace MeadowPeaks;

public sealed class TriPeaksRules : ISolitaireRules
{
    // Explicit cover links take priority over geometric overlap. Authored fans and rotated
    // formations cannot reliably infer blocking from row and horizontal distance alone.
    public bool Exposed(IReadOnlyList<Slot> layout, IReadOnlyList<bool> removed, int i)
    {
        if (removed[i]) return false;
        var slot = layout[i];
        if(slot.BlockedBy!=null) return slot.BlockedBy.All(j=>j>=0 && j<removed.Count && removed[j]);
        for (int j = 0; j < layout.Count; j++)
            if (!removed[j] && layout[j].Row == slot.Row + 1 && Math.Abs(layout[j].X-slot.X) < .6f) return false;
        return true;
    }
    public bool Adjacent(int a, int b) => a == 0 || b == 0 || Math.Abs(a - b) == 1 || Math.Abs(a - b) == 12;

}
