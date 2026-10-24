namespace MeadowPeaks;

// Windows-only rendering contract. A replacement may draw images, vectors, or
// another visual theme without changing the game session or save format.
public interface IGameArtwork
{
    Color Ink { get; }
    Color Cream { get; }
    Color Gold { get; }
    Color Teal { get; }
    void DrawLandscape(Graphics graphics, int region);
    void DrawCard(Graphics graphics, RectangleF bounds, Card card, bool face, bool hint, bool hover);
    void DrawGem(Graphics graphics, RectangleF bounds, int kind);
}

public sealed class MeadowArtwork : IGameArtwork
{
    public Color Ink => Art.Ink;
    public Color Cream => Art.Cream;
    public Color Gold => Art.Gold;
    public Color Teal => Art.Teal;
    public void DrawLandscape(Graphics graphics, int region) => StoryLandscape.Draw(graphics, region);
    public void DrawCard(Graphics graphics, RectangleF bounds, Card card, bool face, bool hint, bool hover) => StoryCardArt.Draw(graphics, bounds, card, face, hint, hover);
    public void DrawGem(Graphics graphics, RectangleF bounds, int kind) => Art.DrawGem(graphics, bounds, kind);
}
