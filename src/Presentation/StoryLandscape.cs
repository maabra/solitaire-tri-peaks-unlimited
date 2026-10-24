using System.Reflection;
namespace MeadowPeaks;
internal static class StoryLandscape
{
    static readonly Lazy<Bitmap?> Background=new(()=>
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("MeadowPeaks.StoryMeadow.png");
        if(stream==null)return null;using var image=Image.FromStream(stream);return new Bitmap(image);
    });
    public static void Draw(Graphics g,int region)
    {
        if(Background.Value is not {} image){Art.Landscape(g,region);return;}
        g.DrawImage(image,new Rectangle(0,0,1280,800));
        var palette=WorldPalette.For(region);
        using var tint=new SolidBrush(Color.FromArgb(palette.Night?100:region%20==0?12:35,palette.Sky));g.FillRectangle(tint,0,0,1280,800);
    }
}
