using System.Drawing.Drawing2D;

namespace MeadowPeaks;
internal static class Art
{
    public static readonly Color Ink = Color.FromArgb(37, 66, 62), Cream = Color.FromArgb(255, 248, 226), Gold = Color.FromArgb(248, 196, 87), Teal = Color.FromArgb(30, 105, 91);
    public static GraphicsPath Round(RectangleF r, float radius = 16)
    {
        var p = new GraphicsPath(); float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p;
    }
    public static void Box(Graphics g, RectangleF r, Color color, float radius = 16)
    { using var p = Round(r, radius); using var b = new SolidBrush(color); g.FillPath(b, p); }
    public static void Text(Graphics g, string text, float x, float y, float size, Color color, bool bold = false, float width = 0, bool center = false)
    {
        using var font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(color);
        using var format = new StringFormat { Alignment = center ? StringAlignment.Center : StringAlignment.Near, LineAlignment = StringAlignment.Near };
        if (width > 0) g.DrawString(text, font, brush, new RectangleF(x, y, width, 1000), format); else g.DrawString(text, font, brush, x, y);
    }
    static void Hill(Graphics g, Color color, params PointF[] points)
    {
        using var b = new SolidBrush(color); using var p = new GraphicsPath();
        p.AddCurve(points, .7f); p.AddLine(points[^1], new(1280, 800)); p.AddLine(new PointF(1280, 800), new(0, 800)); p.CloseFigure(); g.FillPath(b, p);
    }
    public static void Landscape(Graphics g, int region)
    {
        var palette = WorldPalette.For(region);
        using var bg = new LinearGradientBrush(new Point(0, 0), new Point(0, 800), palette.Sky, palette.Horizon); g.FillRectangle(bg, 0, 0, 1280, 800);
        using var sun = new SolidBrush(Color.FromArgb(160, 255, 249, 212)); g.FillEllipse(sun, 970, 100, palette.Night ? 90 : 150, palette.Night ? 90 : 150);
        if (palette.Night)
        {
            for (int i = 0; i < 50; i++) FestiveArt.Star(g, 100 + i * 137 % 1130, 93 + i * 79 % 270, 1.5f + i % 3, Color.FromArgb(160, 255, 246, 200));
            if (region == 18)
            {
                using var aurora = new Pen(Color.FromArgb(50, 143, 242, 206), 25); g.DrawBezier(aurora, new(90, 190), new(420, 30), new(700, 390), new(1140, 170));
            }
        }
        else
        {
            using var clouds = new SolidBrush(Color.FromArgb(90, Color.White));
            for (int i = 0; i < 6; i++) { int x = i * 245 - 50, y = 110 + i % 3 * 44; g.FillEllipse(clouds, x, y, 150, 30); g.FillEllipse(clouds, x + 42, y - 15, 65, 40); }
        }
        if (region is 0 or 11 or 15 or 19)
        {
            Color[] rainbow = { Color.FromArgb(244, 161, 157), Color.FromArgb(252, 199, 132), Color.FromArgb(251, 229, 151), Color.FromArgb(169, 215, 173), Color.FromArgb(160, 198, 224), Color.FromArgb(196, 168, 219) };
            for (int i = 0; i < rainbow.Length; i++) { using var pen = new Pen(Color.FromArgb(115, rainbow[i]), 9); g.DrawArc(pen, 775 + i * 8, 164 + i * 8, 377 - i * 16, 240 - i * 16, 180, 180); }
        }
        Hill(g, WorldPalette.Mix(palette.Horizon, palette.Grass, .35f), new(0, 410), new(200, 260), new(420, 370), new(650, 235), new(850, 355), new(1050, 255), new(1280, 390));
        Hill(g, WorldPalette.Mix(palette.Horizon, palette.Grass, .65f), new(0, 430), new(250, 490), new(500, 385), new(800, 480), new(1060, 390), new(1280, 460));
        Hill(g, palette.Grass, new(0, 620), new(220, 535), new(480, 640), new(750, 545), new(1030, 600), new(1280, 510));
        if (palette.Coast || region == 1)
        {
            using var water = new SolidBrush(Color.FromArgb(115, 121, 199, 216)); g.FillEllipse(water, -80, 476, 1500, 145);
            using var ripple = new Pen(Color.FromArgb(110, Color.White), 2);
            for (int i = 0; i < 12; i++) g.DrawArc(ripple, i * 125 - 30, 520 + i % 3 * 17, 130, 15, 0, 160);
        }
        Hill(g, WorldPalette.Mix(palette.Grass, palette.Snow ? Color.White : Color.FromArgb(35, 88, 73), palette.Snow ? .35f : .55f), new(0, 710), new(220, 660), new(520, 730), new(840, 650), new(1090, 720), new(1280, 665));
        for (int i = 0; i < 18; i++)
        {
            float x = i * 81 - 30, y = 530 + (i * 37) % 170, size = .55f + i % 3 * .25f;
            if (palette.Snow)
            {
                using var trunk = new Pen(Color.FromArgb(114, 138, 143), 5); g.DrawLine(trunk, x, y, x, y + 50 * size);
                using var snow = new SolidBrush(Color.FromArgb(236, 247, 246)); g.FillPolygon(snow, new[] { new PointF(x, y - 48 * size), new PointF(x - 30 * size, y + 22 * size), new PointF(x + 30 * size, y + 22 * size) });
            }
            else Tree(g, x, y, size);
            if (region is 2 or 7 or 10 or 14)
            {
                using var fruit = new SolidBrush(palette.Accent); for (int j = 0; j < 5; j++) g.FillEllipse(fruit, x - 21 + j * 9, y - 17 + j % 2 * 15, 10, 10);
            }
        }
        Box(g, new(1090, 485, 88, 67), Color.FromArgb(247, 227, 186), 3);
        using var roof = new SolidBrush(WorldPalette.Mix(palette.Accent, Color.FromArgb(140, 85, 73), .45f)); g.FillPolygon(roof, new[] { new Point(1077, 489), new Point(1132, 445), new Point(1190, 489) });
        Box(g, new(1123, 514, 20, 38), Color.FromArgb(73, 108, 92), 8); Box(g, new(1101, 503, 14, 17), Gold, 2); Box(g, new(1151, 503, 14, 17), Gold, 2);
        for (int i = 0; i < 38; i++) FestiveArt.Flower(g, 22 + i * 181 % 1250, 719 + i % 4 * 17, 4 + i % 4, i % 3 == 0 ? Color.FromArgb(249, 237, 212) : palette.Accent);
    }
    public static void Tree(Graphics g, float x, float y, float scale)
    {
        using var trunk = new SolidBrush(Color.FromArgb(69, 101, 77)); g.FillRectangle(trunk, x - 3 * scale, y, 6 * scale, 45 * scale);
        using var leaves = new SolidBrush(Color.FromArgb(47, 105, 83)); g.FillEllipse(leaves, x - 30 * scale, y - 40 * scale, 60 * scale, 72 * scale);
        using var light = new SolidBrush(Color.FromArgb(62, 125, 96)); g.FillEllipse(light, x - 27 * scale, y - 41 * scale, 42 * scale, 57 * scale);
    }
    public static string Rank(int rank) => rank switch { 0 => "★", 1 => "A", 11 => "J", 12 => "Q", 13 => "K", _ => rank.ToString() };
    public static string Suit(int suit) => new[] { "♠", "♥", "♦", "♣" }[suit % 4];
    public static Color SuitColor(int suit) => suit switch { 1 => Color.FromArgb(184, 64, 74), 2 => Color.FromArgb(59, 103, 160), 3 => Color.FromArgb(41, 127, 87), _ => Ink };
    public static void Card(Graphics g, RectangleF r, Card card, bool face, bool hint, bool hover)
    {
        Box(g, new(r.X + 1, r.Y + 5, r.Width, r.Height), Color.FromArgb(55, 0, 30, 25), 9);
        var paper = card.Kind switch { CardKind.Ice => Color.FromArgb(216, 240, 255), CardKind.Bomb => Color.FromArgb(255, 224, 217), CardKind.Key or CardKind.Lock => Color.FromArgb(255, 240, 190), CardKind.Gift => Color.FromArgb(225, 243, 210), CardKind.Up or CardKind.Down => Color.FromArgb(237, 225, 250), _ => Cream };
        Box(g, r, face ? paper : Color.FromArgb(44, 110, 97), 9);
        if (face && card.Kind == CardKind.Ice)
        {
            using var frost = new Pen(Color.FromArgb(110, 110, 180, 230), 2);
            g.DrawLine(frost, r.X + r.Width * .7f, r.Y + 4, r.X + r.Width * .45f, r.Y + r.Height * .7f);
            g.DrawLine(frost, r.X + r.Width * .53f, r.Y + r.Height * .4f, r.Right - 3, r.Y + r.Height * .5f);
        }
        using var outline = new Pen(hint ? Gold : face ? Color.FromArgb(220, 211, 184) : Color.FromArgb(138, 183, 156), hint ? 3 : 1);
        using var path = Round(new(r.X + 2, r.Y + 2, r.Width - 4, r.Height - 4), 8); g.DrawPath(outline, path);
        if (!face)
        {
            using var pen = new Pen(Color.FromArgb(65, 171, 206, 169), 1);
            for (float x = r.X + 10; x < r.Right - 8; x += 12) g.DrawLine(pen, x, r.Y + 8, x, r.Bottom - 8);
            Text(g, "✦", r.X, r.Y + r.Height * .30f, r.Width * .45f, Color.FromArgb(179, 206, 169), true, r.Width, true);
        }
        else
        {
            var color = card.Rank == 0 ? Color.FromArgb(173, 117, 37) : SuitColor(card.Suit);
            Text(g, Rank(card.Rank), r.X + 9, r.Y + 4, r.Width * .30f, color, true);
            Text(g, card.Rank == 0 ? "✦" : Suit(card.Suit), r.X, r.Y + r.Height * .36f, r.Width * .42f, color, false, r.Width, true);
            if (hover) { using var b = new SolidBrush(Color.FromArgb(28, 255, 211, 104)); using var p = Round(r, 9); g.FillPath(b, p); }
        }
    }
    public static void DrawGem(Graphics g, RectangleF r, int kind)
    {
        Color[] colors = { Color.FromArgb(212, 86, 108), Color.FromArgb(79, 151, 201), Color.FromArgb(151, 110, 186), Color.FromArgb(231, 173, 62), Color.FromArgb(83, 166, 128), Color.FromArgb(231, 129, 76) };
        using var p = new GraphicsPath(); float cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
        if (kind == 0) p.AddPolygon(new[] { new PointF(cx, r.Y), new PointF(r.Right, cy), new PointF(cx, r.Bottom), new PointF(r.X, cy) });
        else if (kind == 1) p.AddEllipse(r);
        else if (kind == 2) p.AddPolygon(new[] { new PointF(cx, r.Y), new PointF(r.Right, r.Bottom), new PointF(r.X, r.Bottom) });
        else if (kind == 3) { for (int i = 0; i < 10; i++) { double a = i * Math.PI / 5 - Math.PI / 2; float radius = r.Width * (i % 2 == 0 ? .5f : .25f); var point = new PointF(cx + (float)Math.Cos(a) * radius, cy + (float)Math.Sin(a) * radius); if (i == 0) p.StartFigure(); else p.AddLine(p.GetLastPoint(), point); if (i == 0) p.AddLine(point, point); } p.CloseFigure(); }
        else if (kind == 4) p.AddRectangle(r);
        else { p.AddPolygon(Enumerable.Range(0, 6).Select(i => new PointF(cx + (float)Math.Cos(i * Math.PI / 3) * r.Width / 2, cy + (float)Math.Sin(i * Math.PI / 3) * r.Height / 2)).ToArray()); }
        using var b = new LinearGradientBrush(r, ControlPaint.Light(colors[kind], .3f), colors[kind], 70); g.FillPath(b, p);
        using var pen = new Pen(Color.FromArgb(170, Color.White), 2); g.DrawPath(pen, p);
        using var shine = new SolidBrush(Color.FromArgb(95, Color.White)); g.FillEllipse(shine, r.X + r.Width * .25f, r.Y + r.Height * .18f, r.Width * .22f, r.Height * .15f);
    }}
