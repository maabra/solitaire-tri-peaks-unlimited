namespace MeadowPeaks;

internal sealed class GemAnimationPlayer
{
    readonly IReadOnlyList<GemAnimationFrame> frames;
    readonly double started;
    public int FrameIndex { get; private set; } = -1;
    public float Progress { get; private set; }
    public bool Finished { get; private set; }
    public GemAnimationFrame Current => frames[FrameIndex];
    // Bind immutable recorded cascade frames to their presentation start time.
    public GemAnimationPlayer(IReadOnlyList<GemAnimationFrame> frames, double started) { this.frames = frames; this.started = started; }
    // Choose the current recorded cascade phase from elapsed presentation time.
    public bool Advance(double time)
    {
        double elapsed = time - started; int previous = FrameIndex;
        for (int i = 0; i < frames.Count; i++)
        {
            double duration = frames[i].Kind == GemFrameKind.Match ? .48 : .30;
            if (elapsed < duration) { FrameIndex = i; Progress = (float)(elapsed / duration); return previous != i; }
            elapsed -= duration;
        }
        Finished = true; return false;
    }
}
