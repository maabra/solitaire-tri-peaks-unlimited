using System.Drawing.Drawing2D;
namespace MeadowPeaks;
internal static class HandmadeArt
{
    public static void Label(Graphics g,string text,Font font,RectangleF rectangle,Color color)
    {
        using var brush=new SolidBrush(color);
        using var format=new StringFormat {Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter};
        g.DrawString(text,font,brush,rectangle,format);
    }
    public static void Button(Graphics g,RectangleF r,Color color,bool enabled,float hover,bool medallion)
    {
        float radius=medallion ? Math.Min(r.Width,r.Height)*.35f : 13;
        var baseColor=enabled ? WorldPalette.Mix(color,Color.White,hover*.15f) : WorldPalette.Mix(color,Color.FromArgb(159,171,144),.25f);
        Art.Box(g,new(r.X,r.Y+4,r.Width,r.Height),Color.FromArgb(90,55,69,40),radius);
        using var path=Art.Round(r,radius);
        using var paint=new LinearGradientBrush(r,ControlPaint.Light(baseColor,.25f),ControlPaint.Dark(baseColor,.05f),90);
        g.FillPath(paint,path);
        using var edge=new Pen(Color.FromArgb(160,111,115,75),1.5f);g.DrawPath(edge,path);
        using var inner=Art.Round(new(r.X+3,r.Y+3,r.Width-6,r.Height-6),Math.Max(3,radius-3));
        using var highlight=new Pen(Color.FromArgb(165,255,255,238),1);g.DrawPath(highlight,inner);
        using var fiber=new Pen(Color.FromArgb(17,95,84,51),1);
        for(int i=0;i<3;i++)g.DrawBezier(fiber,r.X+9,r.Y+10+i*9,r.X+r.Width*.3f,r.Y+7+i*9,r.X+r.Width*.7f,r.Y+14+i*9,r.Right-9,r.Y+10+i*9);
        if(medallion) {using var pin=new SolidBrush(Color.FromArgb(110,129,103,52));g.FillEllipse(pin,r.X+r.Width/2-2,r.Y+5,4,4);}
    }
}
