namespace MeadowPeaks;

public enum GemFrameKind { Swap, Match, Drop }
public record GemAnimationFrame(GemFrameKind Kind, IReadOnlyList<int> Cells, IReadOnlyList<int> Matched, int Cascade, int First = -1, int Second = -1);

// Optional visual history; alternative ICoinGame implementations need not animate.
public interface IAnimatedCoinGame
{
    IReadOnlyList<GemAnimationFrame> LastAnimation { get; }
}
