using System.Drawing.Drawing2D;

namespace MeadowPeaks;

internal sealed class GameButton : Button
{
    public string Subtitle { get; set; } = "";
    public Color Fill { get; set; }
    public Color Fore { get; set; }
    public Card? CardValue { get; set; }
    public string CardBadge { get; set; } = "";
    public bool Face { get; set; } = true;
    public bool Hint { get; set; }
    public int Gem { get; set; } = -1;
    public PointF VisualOffset { get; set; }
    public float MatchGlow { get; set; }
    public bool HideGem { get; set; }
    public Bitmap? Backdrop { get; set; }
    double entryTime = -10, now;
    float entry = 1, hoverAmount;
    bool reducedMotion;
    public void BeginEntrance(double time, float delay = 0) { if (reducedMotion) return; entryTime = time + delay; entry = 0; }
    public void Advance(double time, bool reduced)
    {
        now = time; reducedMotion = reduced;
        float previous = entry, oldHover = hoverAmount;
        entry = reduced ? 1 : Math.Clamp((float)(time - entryTime) / .36f, 0, 1);
        hoverAmount = reduced ? (hover ? 1 : 0) : hoverAmount + ((hover ? 1 : 0) - hoverAmount) * .24f;
        if (entry < 1 || previous != entry || Math.Abs(oldHover - hoverAmount) > .005f || (Hint && !reduced) || MatchGlow > 0 || VisualOffset != PointF.Empty) Invalidate();
    }
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
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Reuse the scene image rather than repainting the entire landscape
        // behind each of the 64 animated gem controls on every frame.
        if (Backdrop != null) e.Graphics.DrawImage(Backdrop, ClientRectangle, Bounds, GraphicsUnit.Pixel);
        else base.OnPaintBackground(e);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(VisualOffset.X, VisualOffset.Y);
        var r = new RectangleF(1, 1, Width - 3, Height - 7);
        if (!reducedMotion && entry < 1)
        {
            float reveal = Math.Max(.04f, MathF.Sin(entry * MathF.PI / 2));
            g.TranslateTransform(Width / 2f, Height / 2f + (1 - entry) * 12);
            g.ScaleTransform(reveal, .86f + .14f * entry); g.TranslateTransform(-Width / 2f, -Height / 2f);
        }
        if (CardValue != null) artwork.DrawCard(g, r, CardValue, Face, Hint || Focused, hover);
        else if (Gem >= 0)
        {
            Art.Box(g, r, MatchGlow > 0 ? Color.FromArgb(170 + (int)(70 * MatchGlow), Color.White) : Hint || Focused ? Color.FromArgb(180, artwork.Gold) : Color.FromArgb(75, 255, 255, 255), 12);
            if (!HideGem) artwork.DrawGem(g, new(r.X + r.Width * .19f, r.Y + r.Height * .17f, r.Width * .62f, r.Height * .62f), Gem);
        }
        else
        {
            Art.Box(g, new(r.X, r.Y + 4, r.Width, r.Height), Color.FromArgb(40, 0, 30, 20), 14);
            Art.Box(g, r, !Enabled ? WorldPalette.Mix(Fill, Color.FromArgb(170, 187, 169), .4f) : WorldPalette.Mix(Fill, Color.White, hoverAmount * .23f), 14);
            using var font = new Font(Text.Any(char.IsSurrogate) ? "Segoe UI Emoji" : "Segoe UI", Math.Max(11, Height * .27f), Text.Any(char.IsSurrogate) ? FontStyle.Regular : FontStyle.Bold, GraphicsUnit.Pixel);
            if(Subtitle.Length==0) TextRenderer.DrawText(g, Text, font, Rectangle.Round(r), Enabled ? Fore : Color.FromArgb(88,111,99), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            else
            {
                TextRenderer.DrawText(g,Text,font,new Rectangle(5,4,Width-10,(int)(Height*.51f)),Enabled ? Fore : Color.FromArgb(77,98,88),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
                using var small=new Font("Segoe UI",Math.Max(10,Height*.19f),FontStyle.Regular,GraphicsUnit.Pixel);
                TextRenderer.DrawText(g,Subtitle,small,new Rectangle(6,(int)(Height*.51f),Width-12,(int)(Height*.33f)),Color.FromArgb(55,92,79),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            }
            if (Focused) { using var p = new Pen(artwork.Gold, 2); using var path = Art.Round(new(4, 4, Width - 10, Height - 14), 11); g.DrawPath(p, path); }
        }
        if (CardValue != null && Face && CardBadge.Length > 0)
        {
            var tint = CardValue.Kind == CardKind.Ice ? Color.SteelBlue : CardValue.Kind == CardKind.Bomb || CardValue.Kind==CardKind.ColorSeal && CardValue.Strength==1 ? Color.Firebrick : artwork.Teal;
            Art.Box(g, new(r.X + 3, r.Bottom - r.Height * .25f, r.Width - 6, r.Height * .23f), tint, 4);
            Art.Text(g, CardBadge, r.X + 2, r.Bottom - r.Height * .23f, Math.Max(8, r.Width * .125f), Color.White, true, r.Width - 4, true);
        }
        if (Hint && !reducedMotion)
        {
            using var pen = new Pen(Color.FromArgb(85 + (int)(70 * Math.Sin(now * 3)), artwork.Gold), 2);
            using var outline = Art.Round(new(3, 3, Width - 8, Height - 11), 10); g.DrawPath(pen, outline);
        }
    }
}
