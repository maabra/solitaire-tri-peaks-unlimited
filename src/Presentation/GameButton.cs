using System.Drawing.Drawing2D;

namespace MeadowPeaks;

internal sealed class GameButton : Control
{
    public double RevealAt { get; set; } = -1;
    public bool MapNode { get; set; }
    public bool CurrentNode { get; set; }
    public string MenuIcon { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public Color Fill { get; set; }
    public Color Fore { get; set; }
    public Card? CardValue { get; set; }
    public float CardAngle { get; set; }
    public SizeF CardSize { get; set; }
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
        if (now<RevealAt+.1 || entry < 1 || previous != entry || Math.Abs(oldHover - hoverAmount) > .005f || (Hint && !reduced) || MatchGlow > 0 || VisualOffset != PointF.Empty) Invalidate();
    }
    bool hover;
    readonly IGameArtwork artwork;
    public GameButton(IGameArtwork artwork)
    {
        this.artwork = artwork;
        Fill = artwork.Cream; Fore = artwork.Ink;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent; Cursor = Cursors.Hand; TabStop=true; AccessibleRole=AccessibleRole.PushButton;
        SetStyle(ControlStyles.Selectable|ControlStyles.ResizeRedraw,true);
        MouseEnter += (_, _) => { hover = true; Invalidate(); }; MouseLeave += (_, _) => { hover = false; Invalidate(); };
    }
    public void PerformClick() { if(Enabled&&now>=RevealAt) OnClick(EventArgs.Empty); }
    protected override void OnClick(EventArgs e){if(now>=RevealAt)base.OnClick(e);}
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if(e.Button==MouseButtons.Left) {Focus();pressed=true;Invalidate();} }
    protected override void OnMouseUp(MouseEventArgs e) { pressed=false;Invalidate();base.OnMouseUp(e); }
    protected override void OnKeyDown(KeyEventArgs e) { base.OnKeyDown(e); if(e.KeyCode is Keys.Enter or Keys.Space) {PerformClick();e.Handled=true;e.SuppressKeyPress=true;} }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Enter or Keys.Space || base.IsInputKey(keyData);
    bool pressed;
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Reuse the scene image rather than repainting the entire landscape
        // behind each of the 64 animated gem controls on every frame.
        if (Backdrop != null) e.Graphics.DrawImage(Backdrop, ClientRectangle, Bounds, GraphicsUnit.Pixel);
        else base.OnPaintBackground(e);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if(now<RevealAt)return;
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(VisualOffset.X, VisualOffset.Y);
        var r = new RectangleF(1, 1, Width - 3, Height - 7);
        if (!reducedMotion && entry < 1)
        {
            float reveal = Math.Max(.04f, MathF.Sin(entry * MathF.PI / 2));
            g.TranslateTransform(Width / 2f, Height / 2f + (1 - entry) * 12);
            g.ScaleTransform(reveal, .86f + .14f * entry); g.TranslateTransform(-Width / 2f, -Height / 2f);
        }
        if (CardValue != null)
        {
            if(CardSize.Width>0)
            {
                g.TranslateTransform(Width/2f,Height/2f);g.RotateTransform(CardAngle);
                r=new(-CardSize.Width/2,-CardSize.Height/2,CardSize.Width-3,CardSize.Height-7);
            }
            artwork.DrawCard(g, r, CardValue, Face, Hint || Focused, hover);
        }
        else if (Gem >= 0)
        {
            Art.Box(g, r, MatchGlow > 0 ? Color.FromArgb(170 + (int)(70 * MatchGlow), Color.White) : Hint || Focused ? Color.FromArgb(180, artwork.Gold) : Color.FromArgb(75, 255, 255, 255), 12);
            if (!HideGem) artwork.DrawGem(g, new(r.X + r.Width * .19f, r.Y + r.Height * .17f, r.Width * .62f, r.Height * .62f), Gem);
        }
        else
        {
            if(pressed) g.TranslateTransform(0,2);
            if(MapNode)AdventureMapArt.Node(g,r,Fill,Enabled,CurrentNode);else HandmadeArt.Button(g,r,Fill,Enabled,hoverAmount,false);
            if(MenuIcon.Length>0)AdventureMapArt.Icon(g,r,MenuIcon);
            using var font = new Font(Text.Any(char.IsSurrogate) ? "Segoe UI Emoji" : "Segoe UI", Math.Max(11, Height * .27f), Text.Any(char.IsSurrogate) ? FontStyle.Regular : FontStyle.Bold, GraphicsUnit.Pixel);
            if(MenuIcon.Length>0){using var small=new Font("Segoe UI",Math.Max(9,Height*.14f),FontStyle.Bold,GraphicsUnit.Pixel);HandmadeArt.Label(g,Text,small,new(0,Height*.73f,Width,Height*.22f),Color.FromArgb(255,245,214));}
            else if(Subtitle.Length==0) HandmadeArt.Label(g,Text,font,r,MapNode ? Color.White : Enabled ? Fore : Color.FromArgb(77,98,88));
            else
            {
                HandmadeArt.Label(g,Text,font,new(5,3,Width-10,Height*.53f),Enabled ? Fore : Color.FromArgb(77,98,88));
                using var small=new Font("Segoe UI",Math.Max(10,Height*.19f),FontStyle.Regular,GraphicsUnit.Pixel);
                HandmadeArt.Label(g,Subtitle,small,new(6,Height*.50f,Width-12,Height*.34f),Color.FromArgb(55,92,79));
            }
            if (Focused) { using var p = new Pen(artwork.Gold, 2); using var path = Art.Round(new(4, 4, Width - 10, Height - 14), 11); g.DrawPath(p, path); }
        }
        if (CardValue != null && Face && CardBadge.Length > 0)
        {
            var tint = CardValue.Kind == CardKind.Ice ? Color.SteelBlue : CardValue.Kind == CardKind.Bomb || CardValue.Kind==CardKind.ColorSeal && CardValue.Strength==1 ? Color.Firebrick : artwork.Teal;
            Art.Box(g, new(r.X + 3, r.Bottom - r.Height * .25f, r.Width - 6, r.Height * .23f), tint, 4);
            Art.Text(g, CardBadge, r.X + 2, r.Bottom - r.Height * .23f, Math.Max(8, r.Width * .125f), Color.White, true, r.Width - 4, true);
        }
        if (CardValue != null && !Face && CardBadge.Length > 0)
        {
            var clock = new RectangleF(r.X+5,r.Y+r.Height*.38f,r.Width-10,r.Height*.27f);
            Art.Box(g,clock,artwork.Cream,6);
            using var timerFont=new Font("Segoe UI",Math.Max(11,r.Width*.22f),FontStyle.Bold,GraphicsUnit.Pixel);
            HandmadeArt.Label(g,CardBadge,timerFont,clock,artwork.Ink);
        }
        if (Hint && !reducedMotion)
        {
            using var pen = new Pen(Color.FromArgb(85 + (int)(70 * Math.Sin(now * 3)), artwork.Gold), 2);
            using var outline = Art.Round(new(3, 3, Width - 8, Height - 11), 10); g.DrawPath(pen, outline);
        }
    }
}
