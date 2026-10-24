using System.Drawing.Drawing2D;
namespace MeadowPeaks;
internal static class StoryCardArt
{
    public static void Draw(Graphics g,RectangleF r,Card c,bool face,bool hint,bool hover)
    {
        Art.Card(g,r,c,face,hint,hover);
        using var inset=Art.Round(new(r.X+4,r.Y+4,r.Width-8,r.Height-8),6);
        if(!face)
        {
            using var blue=new LinearGradientBrush(r,Color.FromArgb(95,196,227),Color.FromArgb(31,121,180),65);g.FillPath(blue,inset);
            using var weave=new Pen(Color.FromArgb(55,226,251,255),1);
            for(float x=r.X-r.Height;x<r.Right;x+=9)g.DrawLine(weave,Math.Max(r.X+5,x),r.Bottom-5,Math.Min(r.Right-5,x+r.Height),r.Y+5);
            using var edge=new Pen(Color.FromArgb(220,239,255,249),2);g.DrawPath(edge,inset);
            using var frame=new Pen(Color.FromArgb(170,255,225,147),1.3f);g.DrawEllipse(frame,r.X+r.Width*.24f,r.Y+r.Height*.31f,r.Width*.52f,r.Height*.38f);
            FestiveArt.Flower(g,r.X+r.Width*.5f,r.Y+r.Height*.46f,r.Width*.16f,Color.FromArgb(238,248,237));
        }
        else
        {
            var color=Art.SuitColor(c.Suit);
            using var shine=new Pen(Color.FromArgb(210,255,255,255),1.5f);g.DrawPath(shine,inset);
            using var small=new Font("Segoe UI",Math.Max(8,r.Width*.15f),FontStyle.Bold,GraphicsUnit.Pixel);
            HandmadeArt.Label(g,Art.Suit(c.Suit),small,new(r.X+5,r.Y+r.Height*.23f,r.Width*.3f,r.Height*.16f),color);
            var saved=g.Save();g.TranslateTransform(r.Right-8,r.Bottom-10);g.RotateTransform(180);
            HandmadeArt.Label(g,Art.Rank(c.Rank),small,new(0,0,r.Width*.22f,r.Height*.17f),color);g.Restore(saved);
            if(c.Rank>=11)
            {
                using var gold=new SolidBrush(Color.FromArgb(224,180,71));
                float x=r.X+r.Width*.5f,y=r.Y+r.Height*.57f,s=r.Width*.21f;
                g.FillPolygon(gold,new[]{new PointF(x-s,y-s*.5f),new PointF(x-s*.7f,y+s*.5f),new PointF(x+s*.7f,y+s*.5f),new PointF(x+s,y-s*.5f),new PointF(x+s*.35f,y-s*.1f),new PointF(x,y-s),new PointF(x-s*.35f,y-s*.1f)});
            }
        }
        if(c.Kind==CardKind.Lock)
        {
            using var chain=new Pen(Color.FromArgb(220,205,164,67),Math.Max(2,r.Width*.055f));
            g.DrawLine(chain,r.X+5,r.Bottom-10,r.Right-5,r.Y+11);
            float x=r.X+r.Width*.33f,y=r.Y+r.Height*.39f,w=r.Width*.36f,h=r.Height*.28f;
            using var shackle=new Pen(Color.FromArgb(245,251,223,148),Math.Max(3,r.Width*.07f));g.DrawArc(shackle,x+3,y-h*.6f,w-6,h*1.1f,180,180);
            Art.Box(g,new(x,y,w,h),Color.FromArgb(220,174,63),4);using var hole=new SolidBrush(Color.FromArgb(83,70,52));g.FillEllipse(hole,x+w*.38f,y+h*.22f,w*.24f,h*.28f);g.FillRectangle(hole,x+w*.43f,y+h*.42f,w*.14f,h*.3f);
        }
        if(face && c.Kind==CardKind.Bomb)
        {
            using var fuse=new Pen(Color.FromArgb(189,105,42),2);g.DrawBezier(fuse,r.X+r.Width*.55f,r.Y+r.Height*.49f,r.Right-4,r.Y+r.Height*.2f,r.Right-5,r.Y+r.Height*.55f,r.Right-8,r.Y+r.Height*.25f);
            FestiveArt.Star(g,r.Right-8,r.Y+r.Height*.25f,r.Width*.09f,Color.OrangeRed);
        }
    }
}
