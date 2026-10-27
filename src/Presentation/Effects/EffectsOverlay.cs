using System.ComponentModel;
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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float ViewScale { get; set; } = 1;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PointF ViewOffset { get; set; }
    // Create a transparent native surface dedicated to decorations above game controls.
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
    // Keep the decorative overlay from intercepting normal game input.
    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x84) { message.Result = new IntPtr(-1); return; }
        if (message.Msg == 0x21) { message.Result = new IntPtr(3); return; }
        base.WndProc(ref message);
    }
    // Align visibility and bounds with the owner while respecting reduced motion.
    public void Follow(Form owner, bool enabled)
    {
        if (!enabled || scene.ReducedMotion || !owner.Visible || owner.WindowState == FormWindowState.Minimized || !owner.ContainsFocus) { Hide(); return; }
        var bounds = new Rectangle(owner.PointToScreen(Point.Empty), owner.ClientSize);
        if (Bounds != bounds) Bounds = bounds;
        if (!Visible) Show(owner);
        RenderNativeFrame();
    }
    // Suppress ordinary background painting for the layered transparent surface.
    protected override void OnPaintBackground(PaintEventArgs e) { }
    // Leave rendering to the layered-frame path instead of ordinary control painting.
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
    [StructLayout(LayoutKind.Sequential)] struct NativePoint { public int X, Y; /* Store the point in the unmanaged structure layout required by Windows drawing APIs. */
    public NativePoint(int x, int y) { X = x; Y = y; } }
    [StructLayout(LayoutKind.Sequential)] struct NativeSize { public int Width, Height; /* Store frame dimensions in the unmanaged structure layout required by Windows drawing APIs. */
    public NativeSize(int w, int h) { Width = w; Height = h; } }
    [StructLayout(LayoutKind.Sequential, Pack = 1)] struct BlendFunction { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
    // Acquire the Windows drawing context used when updating the layered surface.
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr window);
    // Release a Windows drawing context acquired for the overlay frame.
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr window, IntPtr dc);
    // Create the temporary memory drawing context for compositing a frame.
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    // Select the frame bitmap into the temporary native drawing context.
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
    // Release the temporary native bitmap handle after compositing.
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
    // Release the temporary memory drawing context after uploading a frame.
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    // Submit the premultiplied-alpha frame to the transparent Windows overlay.
    [DllImport("user32.dll", SetLastError = true)] static extern bool UpdateLayeredWindow(IntPtr window, IntPtr destinationDC, ref NativePoint destination, ref NativeSize size, IntPtr sourceDC, ref NativePoint source, uint colorKey, ref BlendFunction blend, uint flags);
    // Release the overlay's owned drawing surface before disposing the form.
    protected override void Dispose(bool disposing) { if (disposing) surface?.Dispose(); base.Dispose(disposing); }
}
