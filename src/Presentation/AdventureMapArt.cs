using System.Drawing.Drawing2D;
namespace MeadowPeaks;
internal static class AdventureMapArt
{
    static readonly Lazy<Bitmap> background=new(()=>new Bitmap(typeof(AdventureMapArt).Assembly.GetManifestResourceStream("MeadowPeaks.AdventureVillage.png")!));
    public static readonly PointF[] Route={new(330,650),new(430,632),new(530,650),new(630,620),new(720,575),new(750,510),new(670,475),new(570,490),new(470,470),new(390,425),new(355,360),new(420,310),new(520,320),new(620,340),new(720,310),new(820,270),new(875,210),new(775,190),new(670,215),new(560,190)};
    // Draw native adventure scenery behind the map controls.
    public static void Landscape(Graphics g){g.DrawImage(background.Value,new RectangleF(0,0,1280,800));}
    // Draw the native map road using its ordered stop coordinates.
    public static void Trail(Graphics g)
    {
        using var shadow=new Pen(Color.FromArgb(120,88,118,55),47){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var sand=new Pen(Color.FromArgb(243,217,156),37){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var stitches=new Pen(Color.FromArgb(197,171,114),2){DashPattern=new float[]{2,5},LineJoin=LineJoin.Round};
        g.DrawCurve(shadow,Route,.5f);g.DrawCurve(sand,Route,.5f);g.DrawCurve(stitches,Route,.5f);
    }
    // Paint the chapter label panel and its decorative trim.
    public static void Ribbon(Graphics g,RectangleF r,string title,string caption)
    {
        Art.Box(g,new(r.X-2,r.Y+5,r.Width+4,r.Height),Color.FromArgb(140,109,41,58),12);
        using var shape=Art.Round(r,12);using var fill=new LinearGradientBrush(r,Color.FromArgb(247,119,104),Color.FromArgb(200,48,66),90);g.FillPath(fill,shape);
        using var rim=new Pen(Color.FromArgb(255,188,142),3);g.DrawPath(rim,shape);
        Art.Text(g,caption,r.X,r.Y+7,10,Color.FromArgb(255,235,188),true,r.Width,true);
        Art.Text(g,title,r.X,r.Y+24,25,Color.White,true,r.Width,true);
    }
    // Draw a level stop with its current state and earned-star decoration.
    public static void Node(Graphics g,RectangleF r,Color fill,bool enabled,bool current)
    {
        var oval=new RectangleF(r.X+3,r.Y+3,r.Width-6,r.Height-8);
        using var shadow=new SolidBrush(current?Color.FromArgb(180,139,52):Color.FromArgb(72,117,140));g.FillEllipse(shadow,oval.X,oval.Y+7,oval.Width,oval.Height);
        using var paint=new LinearGradientBrush(oval,ControlPaint.Light(fill,.4f),fill,90);g.FillEllipse(paint,oval);
        using var edge=new Pen(current?Color.FromArgb(255,242,174):enabled?Color.FromArgb(224,245,247):Color.FromArgb(164,199,207),5);g.DrawEllipse(edge,oval);
        using var glint=new Pen(Color.FromArgb(130,255,255,220),2);g.DrawArc(glint,oval.X+6,oval.Y+6,oval.Width-12,oval.Height-12,200,100);
    }
    // Draw the native map activity icon in its supplied bounds.
    public static void Icon(Graphics g,RectangleF r,string kind)
    {
        float x=r.X+r.Width/2,y=r.Y+r.Height*.40f,s=r.Width*.28f;
        if(kind=="wheel")
        {
            using var gold=new SolidBrush(Color.FromArgb(255,205,65));g.FillEllipse(gold,x-s,y-s,s*2,s*2);
            using var spoke=new Pen(Color.FromArgb(185,60,116),5);for(int i=0;i<8;i++){double a=i*Math.PI/4;g.DrawLine(spoke,x,y,x+(float)Math.Cos(a)*s,y+(float)Math.Sin(a)*s);}
            g.FillEllipse(gold,x-7,y-7,14,14);using var white=new SolidBrush(Color.White);g.FillPolygon(white,new[]{new PointF(x-6,y-s-5),new PointF(x+6,y-s-5),new PointF(x,y-s+10)});
        }
        else if(kind=="gems")
        {
            using var gem=new SolidBrush(Color.FromArgb(63,212,235));g.FillPolygon(gem,new[]{new PointF(x-s,y-5),new PointF(x-s*.6f,y-s),new PointF(x+s*.6f,y-s),new PointF(x+s,y-5),new PointF(x,y+s)});
            using var light=new Pen(Color.FromArgb(196,255,252),3);g.DrawLines(light,new[]{new PointF(x-s*.6f,y-s),new PointF(x,y+s),new PointF(x+s*.6f,y-s)});
        }
        else if(kind=="endless")
        {
            using var sea=new SolidBrush(Color.FromArgb(88,196,231));g.FillEllipse(sea,x-s,y-s,s*2,s*2);using var land=new SolidBrush(Color.FromArgb(164,219,99));g.FillPolygon(land,new[]{new PointF(x-s*.7f,y-s*.3f),new PointF(x,y-s*.7f),new PointF(x+s*.4f,y),new PointF(x+s*.7f,y+s*.7f),new PointF(x-s*.3f,y+s*.3f)});
        }
        else
        {
            Art.Box(g,new(x-s,y-s,s*2,s*1.7f),Color.FromArgb(167,102,187),5);using var paper=new Pen(Color.FromArgb(255,228,173),3);g.DrawLine(paper,x,y-s+3,x,y+s*.6f);g.DrawLine(paper,x-s*.7f,y-6,x-5,y-3);g.DrawLine(paper,x+5,y-3,x+s*.7f,y-6);
        }
    }
}
