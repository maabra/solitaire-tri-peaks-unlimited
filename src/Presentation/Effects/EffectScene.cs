using System.Drawing.Drawing2D;

namespace MeadowPeaks;

public interface IVisualEffect
{
    bool Finished { get; }
    // Advance an effect's lifetime using elapsed seconds rather than frame count.
    void Advance(float seconds);
    // Paint the current effect state without changing game rules or rewards.
    void Draw(Graphics graphics, IGameArtwork artwork);
}

// Effects are presentation objects. They never change a hand, wallet, or save.
public sealed class EffectScene
{
    readonly List<IVisualEffect> effects = new();
    readonly Random random = new();
    public double Time { get; private set; }
    public bool ReducedMotion { get; set; }
    public bool Ambient { get; set; } = true;
    public int Theme { get; set; }
    public int Count => effects.Count;
    // Advance effect lifetimes and remove expired presentation objects.
    public void Advance(float seconds)
    {
        Time += seconds;
        if (ReducedMotion) { effects.Clear(); return; }
        foreach (var effect in effects) effect.Advance(seconds);
        effects.RemoveAll(e => e.Finished);
    }
    // Remove active decorations when changing or resetting the presentation.
    public void Clear() => effects.Clear();
    // Register a decorative effect only when the current motion policy permits it.
    public void Add(IVisualEffect effect) { if (!ReducedMotion) { if (effects.Count >= 220) effects.RemoveAt(0); effects.Add(effect); } }
    // Create a card-flight decoration between the supplied endpoints.
    public void Fly(Card card, PointF from, PointF to) => Add(new FlyingCard(card, from, to));
    // Create a jewel-flight decoration between the supplied endpoints.
    public void FlyGem(int kind, PointF from, PointF to) => Add(new FlyingGem(kind, from, to));
    // Create a floating text decoration at the supplied position.
    public void Cheer(string text, PointF center, Color? color = null) => Add(new FloatingCheer(text, center, color ?? Art.Teal));
    // Emit a bounded group of decorative particles around the supplied origin.
    public void Burst(PointF origin, Color color, int count = 16)
    {
        for (int i = 0; i < count; i++)
        {
            double a = random.NextDouble() * Math.PI * 2; float speed = 35 + random.Next(95);
            Add(new Spark(origin, new((float)Math.Cos(a) * speed, (float)Math.Sin(a) * speed - 35), color, .55f + (float)random.NextDouble() * .5f, 3 + random.Next(6), i % 3));
        }
    }
    // Visualize a coin reward with a bounded number of decorative flights.
    public void Coins(int amount, PointF origin)
    {
        for (int i = 0; i < Math.Clamp(amount / 12, 4, 12); i++) Add(new FlyingCoin(origin, new(1120, 49), i * .045f));
    }
    // Replace the current effects with the victory particle presentation.
    public void Celebrate()
    {
        Clear();
        Color[] colors = { Art.Gold, Color.FromArgb(255, 151, 181), Color.FromArgb(144, 220, 200), Color.FromArgb(182, 162, 240), Color.FromArgb(250, 223, 125) };
        for (int i = 0; i < 130; i++) Add(new Spark(new(random.Next(90, 1190), random.Next(-280, 80)), new(random.Next(-38, 39), random.Next(70, 140)), colors[i % colors.Length], 2.7f + (float)random.NextDouble(), random.Next(4, 9), i % 3, 10));
        Burst(new(470, 290), Art.Gold, 18); Burst(new(810, 290), Color.HotPink, 18);
    }
    // Paint active effects while honoring reduced motion.
    public void Draw(Graphics g, IGameArtwork artwork)
    {
        if (ReducedMotion) return;
        if (Ambient)
        {
            for (int i = 0; i < 12; i++)
            {
                float x = (float)((i * 113 + Time * (7 + i % 3)) % 1320) - 20;
                float y = 570 + i % 5 * 39 + (float)Math.Sin(Time * .8 + i) * 16;
                using var halo = new SolidBrush(Color.FromArgb(18, 255, 236, 156)); g.FillEllipse(halo, x - 7, y - 7, 18, 18);
                using var dot = new SolidBrush(Color.FromArgb(120 + (int)(70 * Math.Sin(Time * 2 + i)), 255, 241, 170)); g.FillEllipse(dot, x, y, 4, 4);
            }
            for (int i = 0; i < 3; i++) FestiveArt.Butterfly(g, 71 + i * 553 + (float)Math.Sin(Time * .32 + i) * 25, 230 + i * 118 + (float)Math.Sin(Time + i) * 22, (float)Time + i, i % 2 == 0 ? Color.FromArgb(250, 181, 110) : Color.FromArgb(216, 163, 231), .7f);
        }
        foreach (var effect in effects) effect.Draw(g, artwork);
    }
}

internal abstract class TimedEffect : IVisualEffect
{
    protected float age;
    protected readonly float duration;
    protected float Progress => Math.Clamp(age / duration, 0, 1);
    public bool Finished => age >= duration;
    // Set the decorative effect's lifetime independently of game state.
    protected TimedEffect(float duration) { this.duration = duration; }
    // Advance the presentation age used to derive normalized effect progress.
    public void Advance(float seconds) => age += seconds;
    // Paint the derived effect's current presentation frame.
    public abstract void Draw(Graphics g, IGameArtwork artwork);
}

internal sealed class FlyingCard : TimedEffect
{
    readonly Card card;
    readonly PointF from, to;
    // Capture the immutable face and endpoints for a decorative card flight.
    public FlyingCard(Card card, PointF from, PointF to) : base(.5f) { this.card = card; this.from = from; this.to = to; }
    // Interpolate the decorative card's flight and size without moving the real tableau control.
    public override void Draw(Graphics g, IGameArtwork artwork)
    {
        float p = 1 - MathF.Pow(1 - Progress, 3), x = from.X + (to.X - from.X) * p, y = from.Y + (to.Y - from.Y) * p - MathF.Sin(Progress * MathF.PI) * 105;
        var saved = g.Save(); g.TranslateTransform(x, y); g.RotateTransform(MathF.Sin(Progress * MathF.PI) * 14);
        artwork.DrawCard(g, new(-38, -52, 76, 104), card, true, true, false); g.Restore(saved);
    }
}

internal sealed class FlyingGem : TimedEffect
{
    readonly int kind;
    readonly PointF from, to;
    // Capture the jewel kind and endpoints for a decorative reward flight.
    public FlyingGem(int kind, PointF from, PointF to) : base(.30f) { this.kind = kind; this.from = from; this.to = to; }
    // Paint the jewel along its normalized flight path.
    public override void Draw(Graphics g, IGameArtwork artwork)
    {
        float p = Progress * Progress * (3 - 2 * Progress), x = from.X + (to.X - from.X) * p, y = from.Y + (to.Y - from.Y) * p;
        var state=g.Save();g.SetClip(new RectangleF(425,180,530,500));artwork.DrawGem(g, new(x - 18, y - 18, 36, 36), kind);g.Restore(state);
    }
}

internal sealed class FlyingCoin : TimedEffect
{
    readonly PointF from, to;
    readonly float delay;
    // Capture a delayed decorative coin flight and its endpoints.
    public FlyingCoin(PointF from, PointF to, float delay) : base(.8f + delay) { this.from = from; this.to = to; this.delay = delay; }
    // Wait for the stagger delay, then paint the coin toward its destination.
    public override void Draw(Graphics g, IGameArtwork artwork)
    {
        if (age < delay) return;
        float p = Math.Clamp((age - delay) / .8f, 0, 1), ease = p * p;
        float x = from.X + (to.X - from.X) * ease, y = from.Y + (to.Y - from.Y) * ease - MathF.Sin(p * MathF.PI) * 65;
        float width = 8 + MathF.Abs(MathF.Cos(p * 13)) * 12;
        using var gold = new SolidBrush(Art.Gold); g.FillEllipse(gold, x - width / 2, y - 11, width, 22);
        using var pen = new Pen(Color.FromArgb(176, 120, 32), 2); g.DrawEllipse(pen, x - width / 2 + 2, y - 9, Math.Max(1, width - 4), 18);
    }
}

internal sealed class FloatingCheer : TimedEffect
{
    readonly string text;
    readonly PointF center;
    readonly Color color;
    // Capture the label, origin and color for a short-lived text effect.
    public FloatingCheer(string text, PointF center, Color color) : base(1.5f) { this.text = text; this.center = center; this.color = color; }
    // Paint the label's rising/fading presentation using its normalized lifetime.
    public override void Draw(Graphics g, IGameArtwork artwork)
    {
        float p = Progress, width = Math.Clamp(text.Length * 12 + 40, 140, 590), y = center.Y - p * 38;
        int alpha = (int)(255 * Math.Min(1, (1 - p) * 4));
        Art.Box(g, new(center.X - width / 2, y - 22, width, 44), Color.FromArgb(alpha, color), 22);
        Art.Text(g, text, center.X - width / 2, y - 15, 22, Color.FromArgb(alpha, Art.Cream), true, width, true);
    }
}

internal sealed class Spark : TimedEffect
{
    readonly PointF origin, velocity;
    readonly Color color;
    readonly float size, gravity;
    readonly int kind;
    // Capture the particle's velocity, color and lifetime for deterministic playback.
    public Spark(PointF origin, PointF velocity, Color color, float duration, float size, int kind, float gravity = 110) : base(duration)
    { this.origin = origin; this.velocity = velocity; this.color = color; this.size = size; this.kind = kind; this.gravity = gravity; }
    // Paint the particle at its age-derived position and opacity.
    public override void Draw(Graphics g, IGameArtwork artwork)
    {
        float x = origin.X + velocity.X * age, y = origin.Y + velocity.Y * age + gravity * age * age / 2;
        int alpha = (int)(255 * Math.Min(1, (1 - Progress) * 3)); var c = Color.FromArgb(alpha, color);
        if (kind == 0) FestiveArt.Star(g, x, y, size, c, age * 90);
        else { using var b = new SolidBrush(c); if (kind == 1) g.FillEllipse(b, x, y, size * 1.3f, size * .7f); else { var state = g.Save(); g.TranslateTransform(x, y); g.RotateTransform(age * 130); g.FillRectangle(b, -size / 2, -size, size, size * 2); g.Restore(state); } }
    }
}
