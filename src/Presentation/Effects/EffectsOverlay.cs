using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MeadowPeaks;

// An owned, click-through window lets sprites travel above native controls.
// It never takes focus, appears in the taskbar, or intercepts game input.
internal sealed class EffectsOverlay : Form
{
    readonly EffectScene scene;
    readonly IGameArtwork artwork;
    Bitmap? surface;
    public float ViewScale { get; set; } = 1;
    public PointF ViewOffset { get; set; }
    public EffectsOverlay(EffectScene scene, IGameArtwork artwork)
    {
        this.scene = scene; this.artwork = artwork;
        FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false;
        DoubleBuffered = true; StartPosition = FormStartPosition.Manual;
    }
    protected override bool ShowWithoutActivation => true;
    protected override CreateParams CreateParams
    {
        get { var p = base.CreateParams; p.ExStyle |= 0x20 | 0x80 | 0x80000 | 0x08000000; return p; }
    }
    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x84) { message.Result = new IntPtr(-1); return; }
        if (message.Msg == 0x21) { message.Result = new IntPtr(3); return; }
        base.WndProc(ref message);
    }
    public void Follow(Form owner, bool enabled)
    {
        if (!enabled || scene.ReducedMotion || !owner.Visible || owner.WindowState == FormWindowState.Minimized || !owner.ContainsFocus) { Hide(); return; }
        var bounds = new Rectangle(owner.PointToScreen(Point.Empty), owner.ClientSize);
        if (Bounds != bounds) Bounds = bounds;
        if (!Visible) Show(owner);
        RenderNativeFrame();
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e) { }

    // A per-pixel alpha surface avoids color-key fringes around fading particles.
    internal bool RenderNativeFrame()
    {
        if (Width <= 0 || Height <= 0) return true;
        if (surface?.Size != Size) { surface?.Dispose(); surface = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb); }
        using (var graphics = Graphics.FromImage(surface))
        {
            graphics.Clear(Color.Transparent); graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(ViewOffset.X, ViewOffset.Y); graphics.ScaleTransform(ViewScale, ViewScale);
            scene.Draw(graphics, artwork);
        }
        IntPtr screen = GetDC(IntPtr.Zero), memory = CreateCompatibleDC(screen), bitmap = IntPtr.Zero, previous = IntPtr.Zero;
        try
        {
            bitmap = surface.GetHbitmap(Color.FromArgb(0)); previous = SelectObject(memory, bitmap);
            var destination = new NativePoint(Left, Top); var source = new NativePoint(0, 0); var size = new NativeSize(Width, Height);
            var blend = new BlendFunction { SourceConstantAlpha = 255, AlphaFormat = 1 };
            return UpdateLayeredWindow(Handle, screen, ref destination, ref size, memory, ref source, 0, ref blend, 2);
        }
        finally
        {
            if (previous != IntPtr.Zero) SelectObject(memory, previous);
            if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
            if (memory != IntPtr.Zero) DeleteDC(memory);
            if (screen != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screen);
        }
    }
    [StructLayout(LayoutKind.Sequential)] struct NativePoint { public int X, Y; public NativePoint(int x, int y) { X = x; Y = y; } }
    [StructLayout(LayoutKind.Sequential)] struct NativeSize { public int Width, Height; public NativeSize(int w, int h) { Width = w; Height = h; } }
    [StructLayout(LayoutKind.Sequential, Pack = 1)] struct BlendFunction { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr window);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr window, IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    [DllImport("user32.dll", SetLastError = true)] static extern bool UpdateLayeredWindow(IntPtr window, IntPtr destinationDC, ref NativePoint destination, ref NativeSize size, IntPtr sourceDC, ref NativePoint source, uint colorKey, ref BlendFunction blend, uint flags);
    protected override void Dispose(bool disposing) { if (disposing) surface?.Dispose(); base.Dispose(disposing); }
}
