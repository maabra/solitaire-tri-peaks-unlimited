using System.Drawing.Drawing2D;
namespace MeadowPeaks;
internal sealed class GemMatchLinks : TimedEffect
{
    readonly HashSet<int> matched;
    // Capture matched cell identities for a short-lived connecting highlight.
    public GemMatchLinks(IEnumerable<int> cells):base(.48f){matched=cells.ToHashSet();}
    // Draw links between the captured matched cells without changing jewel state.
    public override void Draw(Graphics g,IGameArtwork artwork)
    {
        // Convert a jewel index to the native grid's drawing center.
        PointF Center(int i)=>new(462+i%8*64,218+i/8*60);
        int alpha=(int)(235*Math.Sin(Math.PI*Progress));
        using var halo=new Pen(Color.FromArgb(alpha/2,artwork.Gold),14){StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var beam=new Pen(Color.FromArgb(alpha,Color.White),5){StartCap=LineCap.Round,EndCap=LineCap.Round};
        foreach(int i in matched){foreach(int j in new[]{i+1,i+8})if(matched.Contains(j)&&(j==i+8||j/8==i/8)){g.DrawLine(halo,Center(i),Center(j));g.DrawLine(beam,Center(i),Center(j));}var p=Center(i);g.DrawEllipse(beam,p.X-22,p.Y-22,44,44);}
    }
}
