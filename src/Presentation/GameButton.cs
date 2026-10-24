using System.Drawing.Drawing2D;

namespace MeadowPeaks;

internal sealed class GameButton : Button
{
    public Color Fill { get; set; }
    public Color Fore { get; set; }
    public Card? CardValue { get; set; }
    public bool Face { get; set; } = true;
    public bool Hint { get; set; }
    public int Gem { get; set; } = -1;
    bool hover;
    readonly IGameArtwork artwork;
    public GameButton(IGameArtwork artwork)
    {
        this.artwork = artwork;
        Fill = artwork.Cream; Fore = artwork.Ink;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent; FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; Cursor = Cursors.Hand;
        MouseEnter += (_, _) => { hover = true; Invalidate(); }; MouseLeave += (_, _) => { hover = false; Invalidate(); };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new RectangleF(1, 1, Width - 3, Height - 7);
        if (CardValue != null) artwork.DrawCard(g, r, CardValue, Face, Hint || Focused, hover);
        else if (Gem >= 0)
        {
            Art.Box(g, r, Hint || Focused ? Color.FromArgb(180, artwork.Gold) : Color.FromArgb(75, 255, 255, 255), 12);
            artwork.DrawGem(g, new(r.X + r.Width * .19f, r.Y + r.Height * .17f, r.Width * .62f, r.Height * .62f), Gem);
        }
        else
        {
            Art.Box(g, new(r.X, r.Y + 4, r.Width, r.Height), Color.FromArgb(40, 0, 30, 20), 14);
            Art.Box(g, r, !Enabled ? Color.FromArgb(167, 184, 165) : hover ? ControlPaint.Light(Fill, .13f) : Fill, 14);
            using var font = new Font("Segoe UI", Math.Max(11, Height * .27f), FontStyle.Bold, GraphicsUnit.Pixel);
            TextRenderer.DrawText(g, Text, font, Rectangle.Round(r), Enabled ? Fore : Color.FromArgb(88, 111, 99), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            if (Focused) { using var p = new Pen(artwork.Gold, 2); using var path = Art.Round(new(4, 4, Width - 10, Height - 14), 11); g.DrawPath(p, path); }
        }
    }
}
