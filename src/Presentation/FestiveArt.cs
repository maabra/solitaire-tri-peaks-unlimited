using System.Drawing.Drawing2D;

namespace MeadowPeaks;

internal static class FestiveArt
{
    // Draw a decorative star using the supplied size and color.
    public static void Star(Graphics g, float x, float y, float radius, Color color, float angle = 0)
    {
        var points = Enumerable.Range(0, 10).Select(i => { double a = (i * 36 + angle - 90) * Math.PI / 180; float r = radius * (i % 2 == 0 ? 1 : .43f); return new PointF(x + (float)Math.Cos(a) * r, y + (float)Math.Sin(a) * r); }).ToArray();
        using var b = new SolidBrush(color); g.FillPolygon(b, points);
    }
    // Draw a decorative blossom for native scenery and rewards.
    public static void Flower(Graphics g, float x, float y, float size, Color color)
    {
        using var stem = new Pen(Color.FromArgb(78, 141, 105), 2); g.DrawLine(stem, x, y, x - 2, y + size * 2.2f);
        using var petals = new SolidBrush(color);
        for (int i = 0; i < 5; i++) { float a = i * MathF.PI * 2 / 5; g.FillEllipse(petals, x + MathF.Cos(a) * size * .55f - size * .5f, y + MathF.Sin(a) * size * .55f - size * .5f, size, size); }
        using var center = new SolidBrush(Color.FromArgb(255, 211, 98)); g.FillEllipse(center, x - size * .3f, y - size * .3f, size * .6f, size * .6f);
    }
    // Draw a decorative butterfly at the current presentation pose.
    public static void Butterfly(Graphics g, float x, float y, float phase, Color color, float size = 1)
    {
        float flap = .25f + .75f * MathF.Abs(MathF.Sin(phase * 4));
        using var wing = new SolidBrush(color); using var light = new SolidBrush(ControlPaint.Light(color, .35f));
        g.FillEllipse(wing, x - 13 * flap * size, y - 8 * size, 13 * flap * size, 17 * size);
        g.FillEllipse(light, x + 1, y - 8 * size, 13 * flap * size, 17 * size);
        using var body = new Pen(Art.Ink, 2 * size); g.DrawLine(body, x, y - 5 * size, x, y + 10 * size);
    }
    // Draw the native mascot with the requested expression/pose.
    public static void Pip(Graphics g, float x, float y, float size = 1, bool party = false)
    {
        var saved = g.Save(); g.TranslateTransform(x, y); g.ScaleTransform(size, size);
        using var shadow = new SolidBrush(Color.FromArgb(40, 20, 70, 52)); g.FillEllipse(shadow, -47, 47, 94, 16);
        using var body = new SolidBrush(Color.FromArgb(250, 199, 91)); g.FillEllipse(body, -45, -35, 90, 90);
        using var belly = new SolidBrush(Color.FromArgb(255, 238, 185)); g.FillEllipse(belly, -30, -3, 60, 53);
        using var wing = new SolidBrush(Color.FromArgb(232, 161, 66)); g.FillEllipse(wing, -49, 4, 26, 40); g.FillEllipse(wing, 23, 4, 26, 40);
        using var white = new SolidBrush(Color.White); using var eye = new SolidBrush(Art.Ink);
        g.FillEllipse(white, -26, -20, 23, 29); g.FillEllipse(white, 4, -20, 23, 29);
        g.FillEllipse(eye, -17, -9, 8, 12); g.FillEllipse(eye, 10, -9, 8, 12);
        using var beak = new SolidBrush(Color.FromArgb(230, 123, 69)); g.FillPolygon(beak, new[] { new Point(-10, 9), new Point(11, 9), new Point(0, 21) });
        using var cheeks = new SolidBrush(Color.FromArgb(235, 142, 130)); g.FillEllipse(cheeks, -33, 7, 15, 8); g.FillEllipse(cheeks, 20, 7, 15, 8);
        using var feet = new Pen(Color.FromArgb(191, 119, 49), 5); g.DrawLine(feet, -19, 51, -26, 56); g.DrawLine(feet, 18, 51, 25, 56);
        if (party) { using var hat = new SolidBrush(Color.FromArgb(162, 124, 208)); g.FillPolygon(hat, new[] { new Point(-25, -28), new Point(20, -28), new Point(-3, -78) }); Star(g, -3, -79, 8, Art.Gold); }
        else { using var tuft = new Pen(Color.FromArgb(232, 161, 66), 5); g.DrawArc(tuft, -7, -49, 20, 24, 180, 160); }
        g.Restore(saved);
    }
}
