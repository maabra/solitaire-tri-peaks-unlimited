namespace MeadowPeaks;

public sealed class PeakLayouts : ILayoutProvider
{
    // Build the original peak formation used by the legacy deal generator.
    public List<Slot> Create(int level)
    {
        int variant = level < 4 ? 1 : (level / 7) % 3;
        var slots = new List<Slot>();
        if (variant == 1)
        {
            for (int row = 0; row < 4; row++)
                for (int peak = 0; peak < 2; peak++)
                    for (int j = 0; j <= row; j++) slots.Add(new(2 + peak * 4 - row * .5f + j, row));
        }
        else if (variant == 2)
        {
            for (int row = 0; row < 4; row++)
                for (int j = 0; j < 5 + row; j++) slots.Add(new(2.5f - row * .5f + j, row));
        }
        else
        {
            for (int row = 0; row < 3; row++)
                for (int peak = 0; peak < 3; peak++)
                    for (int j = 0; j <= row; j++) slots.Add(new(2 + peak * 3 - row * .5f + j, row));
            for (int j = 0; j < 10; j++) slots.Add(new(.5f + j, 3));
        }
        return slots;
    }
}
