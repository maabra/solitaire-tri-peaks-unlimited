using System.Drawing.Drawing2D;
namespace MeadowPeaks;
internal static class LuckyWheelArt
{
    static readonly Color[] colors={Color.FromArgb(229,96,170),Color.FromArgb(119,112,212),Color.FromArgb(75,201,219),Color.FromArgb(140,211,87),Color.FromArgb(235,136,113),Color.FromArgb(180,94,212),Color.FromArgb(93,202,184),Color.FromArgb(217,148,77)};
    public static void Draw(Graphics g,PointF center,float radius,float angle,IReadOnlyList<WheelPrize> prizes)
    {
        var state=g.Save();g.TranslateTransform(center.X,center.Y);g.RotateTransform(angle);
        var r=new RectangleF(-radius,-radius,radius*2,radius*2);
        for(int i=0;i<8;i++){using var fill=new LinearGradientBrush(r,ControlPaint.Light(colors[i],.35f),ControlPaint.Dark(colors[i],.08f),45);g.FillPie(fill,r.X,r.Y,r.Width,r.Height,-112.5f+i*45,45);}
        using var edge=new Pen(Color.FromArgb(255,216,101),10);g.DrawEllipse(edge,r);
        for(int i=0;i<8;i++){var label=g.Save();g.RotateTransform(i*45);string text=prizes[i].Item switch {"wild"=>"Wild +1","extra"=>"Cards +1","remove"=>"Remove +1",_=>prizes[i].Coins+" coins"};Art.Text(g,text,-52,-radius+27,14,Color.White,true,104,true);g.Restore(label);}
        using(var stud=new SolidBrush(Color.FromArgb(255,245,181)))for(int i=0;i<8;i++){double a=(i*45-112.5)*Math.PI/180;float x=(float)Math.Cos(a)*radius,y=(float)Math.Sin(a)*radius;g.FillEllipse(stud,x-4,y-4,8,8);}
        using var gold=new SolidBrush(Color.FromArgb(255,211,81));g.FillEllipse(gold,-27,-27,54,54);g.Restore(state);
        using var pointer=new SolidBrush(Color.FromArgb(255,242,187));g.FillPolygon(pointer,new[]{new PointF(center.X-14,center.Y-radius-14),new PointF(center.X+14,center.Y-radius-14),new PointF(center.X,center.Y-radius+20)});
    }
}
