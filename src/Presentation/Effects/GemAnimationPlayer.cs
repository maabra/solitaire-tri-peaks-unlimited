namespace MeadowPeaks;

internal sealed class GemAnimationPlayer
{
    readonly IReadOnlyList<GemAnimationFrame> frames;
    readonly double started;
    public int FrameIndex { get; private set; } = -1;
    public float Progress { get; private set; }
    public bool Finished { get; private set; }
    public GemAnimationFrame Current => frames[FrameIndex];
    public GemAnimationPlayer(IReadOnlyList<GemAnimationFrame> frames, double started) { this.frames = frames; this.started = started; }
    public bool Advance(double time)
    {
        double elapsed = time - started; int previous = FrameIndex;
        for (int i = 0; i < frames.Count; i++)
        {
            double duration = frames[i].Kind == GemFrameKind.Match ? .24 : .2;
            if (elapsed < duration) { FrameIndex = i; Progress = (float)(elapsed / duration); return previous != i; }
            elapsed -= duration;
        }
        Finished = true; return false;
    }
}
