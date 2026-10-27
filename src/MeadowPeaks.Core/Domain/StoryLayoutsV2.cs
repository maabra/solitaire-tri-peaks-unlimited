namespace MeadowPeaks;
// MP19 adds scattered islands and tilted packets without changing MP18 deals.
public sealed class StoryLayoutsV2:ILayoutProvider
{
    public static readonly string[] AddedNames={"Five islands","Six islands","Seven islands","Eight islands","Clockwise spiral","Petal spiral","Open spiral","Tilted spiral","Diamond packets","Five bouquets","Six bouquets","Seven bouquets","Double river","Crossing ribbons","Rolling ribbons","Twin serpents"};
    // Map the level to its deterministic formation name without changing historical family ordering.
    public static string NameFor(int level){int family=(level-1)%32;return family<16?StoryLayouts.NameFor(level):$"{AddedNames[family-16]} {(level-1)/32+1}";}
    // Build historical scattered islands and tilted packets while retaining earlier families.
    public List<Slot> Create(int level)
    {
        int family=(level-1)%32,variation=(level-1)/32;if(family<16)return new StoryLayouts().Create(level);
        double bend=((variation*48271L+12345)%32749)/32749.0*24-12;
        var points=new List<(double X,double Y,double Angle)>();/* Append an authored point to the current formation before its covering graph is compiled. */
    void Add(double x,double y,double a)=>points.Add((x,y,a));
        if(family<20)
        {
            int n=family-11;
            for(int i=0;i<n;i++)for(int j=0;j<4;j++){double tilt=(i%2==0?-1:1)*(14+(family-16)*7)+bend*.3;Add(85+i*830.0/(n-1)+j*3,105+(i%2)*55+j*24+bend*Math.Sin(i),tilt);}
        }
        else if(family<24)
        {
            for(int i=0;i<28;i++){double t=i/27.0,a=t*Math.PI*(2.1+(family-20)*.18)-Math.PI/2,rx=65+t*(250+bend),ry=25+t*80;Add(500+Math.Cos(a)*rx,155+Math.Sin(a)*ry,a*180/Math.PI+90+(family-20)*9);}
        }
        else if(family<28)
        {
            int n=family-20,depth=n==4?6:n==5?5:4;
            for(int i=0;i<n;i++)for(int j=0;j<depth;j++)Add(90+i*820.0/(n-1)+(j%2==0?-5:5),105+Math.Sin(i*1.7)*45+j*20+bend,(i%2==0?1:-1)*(30+j*2)+(family-24)*3);
        }
        else
        {
            for(int lane=0;lane<2;lane++)for(int i=0;i<14;i++){double t=i/13.0,w=t*Math.PI*(1.5+(family-28)*.4);Add(85+t*830,90+lane*130+Math.Sin(w+lane*.8)*(28+bend*.3),Math.Cos(w)*(18+(family-28)*5));}
        }
        // Compare authored card footprints when deriving the cover graph.
        bool Overlap(int a,int b){var p=points[a];var q=points[b];double pa=p.Angle*Math.PI/180,qa=q.Angle*Math.PI/180;foreach(double axis in new[]{pa,pa+Math.PI/2,qa,qa+Math.PI/2})if(Math.Abs((q.X-p.X)*Math.Cos(axis)+(q.Y-p.Y)*Math.Sin(axis))>=35*(Math.Abs(Math.Cos(pa-axis))+Math.Abs(Math.Cos(qa-axis)))+49*(Math.Abs(Math.Sin(pa-axis))+Math.Abs(Math.Sin(qa-axis)))-3)return false;return true;}
        return points.Select((p,i)=>new Slot(i%10,i/10){DrawX=(float)p.X,DrawY=(float)p.Y,Angle=(float)p.Angle,BlockedBy=Enumerable.Range(i+1,points.Count-i-1).Where(j=>Overlap(i,j)).ToArray()}).ToList();
    }
}
