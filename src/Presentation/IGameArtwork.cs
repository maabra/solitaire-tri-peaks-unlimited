namespace MeadowPeaks;

// Windows-only rendering contract. A replacement may draw images, vectors, or
// another visual theme without changing the game session or save format.
public interface IGameArtwork
{
    Color Ink { get; }
    Color Cream { get; }
    Color Gold { get; }
    Color Teal { get; }
    // Paint scenery through an injected renderer without coupling game rules to a graphics implementation.
    void DrawLandscape(Graphics graphics, int region);
    // Render a card's face/back and highlight state within the caller's bounds.
    void DrawCard(Graphics graphics, RectangleF bounds, Card card, bool face, bool hint, bool hover);
    // Render one jewel using the injected artwork implementation.
    void DrawGem(Graphics graphics, RectangleF bounds, int kind);
}

public sealed class MeadowArtwork : IGameArtwork
{
    public Color Ink => Art.Ink;
    public Color Cream => Art.Cream;
    public Color Gold => Art.Gold;
    public Color Teal => Art.Teal;
    // Delegate native scenery drawing to the story landscape renderer.
    public void DrawLandscape(Graphics graphics, int region) => StoryLandscape.Draw(graphics, region);
    // Delegate native card drawing to the shared story card artwork.
    public void DrawCard(Graphics graphics, RectangleF bounds, Card card, bool face, bool hint, bool hover) => StoryCardArt.Draw(graphics, bounds, card, face, hint, hover);
    // Delegate jewel drawing to the native art primitives.
    public void DrawGem(Graphics graphics, RectangleF bounds, int kind) => Art.DrawGem(graphics, bounds, kind);
}
