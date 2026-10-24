namespace MeadowPeaks;

// Real overlapping card formations. Pixel positions, rotations, and the cover
// graph travel with the saved board; the renderer never invents game rules.
public sealed class StoryLayouts : ILayoutProvider
{
    public static readonly string[] Names={"Three peaks","Royal fan","Moon ring","Twin fans","Butterfly","River ribbon","Diamond loop","Starburst","Five towers","Double arch","Crown","Braided trail","Sunrise","Eight towers","Hourglass","Serpent"};
    public static string NameFor(int level)=>$"{Names[(level-1)%Names.Length]} {(level-1)/Names.Length+1}";
    public List<Slot> Create(int level)
    {
        int family=(level-1)%16,variation=(level-1)/16;
        var points=new List<(double X,double Y,double Angle)>();
        void Add(double x,double y,double angle=0)=>points.Add((x,y,angle));
        void Stack(double x,double y,int count,double lean=0){for(int i=0;i<count;i++)Add(x+lean*i,y+i*21,lean*1.5);}
        void Arc(double cx,double cy,double rx,double ry,int count,double from,double to,bool split=true)
        {
            var arc=Enumerable.Range(0,count).Select(i=>from+(to-from)*i/(count-1)).ToArray();
            // Center first, ends last, so both ends of a fan can open.
            foreach(double a in split?arc.OrderBy(a=>Math.Abs(a)):arc.AsEnumerable()){double t=a*Math.PI/180;Add(cx+rx*Math.Sin(t),cy-ry*Math.Cos(t),a);}
        }
        double bend=(variation%5-2)*5;
        if(family==0)
        {
            var classic=new PeakLayouts().Create(21);
            foreach(var s in classic)Add(78+s.X*81,48+s.Row*57);
        }
        else if(family is 1 or 10 or 12)
        {
            Arc(500,170,300+bend,112,16,-68,68);
            int stacks=family==12?5:3;
            for(int i=0;i<stacks;i++)Stack(90+i*820.0/(stacks-1),195,stacks==5?3:4,(i%2==0?1:-1)*(variation%3));
        }
        else if(family is 2 or 6 or 14)
        {
            int n=20;
            for(int i=0;i<n;i++){
                double t=(i*360.0/n-90)*Math.PI/180,x,y;
                if(family==6){double a=i*4.0/n;int edge=(int)a;double f=a-edge;var corners=new[]{(500d,46d),(770d,155d),(500d,263d),(230d,155d),(500d,46d)};x=corners[edge].Item1+(corners[edge+1].Item1-corners[edge].Item1)*f;y=corners[edge].Item2+(corners[edge+1].Item2-corners[edge].Item2)*f;}
                else {x=500+(family==14?Math.Sin(t*2):Math.Cos(t))*(230+bend);y=155+Math.Sin(t)*100;}
                Add(x,y,t*180/Math.PI+90);
            }
            Stack(92,100,4);Stack(908,100,4);
        }
        else if(family is 3 or 4 or 9)
        {
            Arc(282,157,176,95,12,-65,65);Arc(718,157,176,95,12,-65,65);
            Stack(110,207,3);Stack(500,207,3);Stack(890,207,3);
        }
        else if(family is 5 or 11 or 15)
        {
            for(int i=0;i<26;i++){double t=i/25.0;double wave=t*Math.PI*(family==15?3:2);Add(85+t*830,155+Math.Sin(wave)*(70+bend),Math.Cos(wave)*25);}
            if(family==11){Stack(150,210,3);Stack(850,210,3);}
        }
        else if(family==7)
        {
            for(int arm=0;arm<6;arm++)for(int i=0;i<5;i++){double a=(arm*60-90)*Math.PI/180;Add(500+Math.Cos(a)*(48+i*48),155+Math.Sin(a)*(24+i*20),arm*60);}
        }
        else
        {
            int stacks=family==13?8:5+variation%3;
            for(int i=0;i<stacks;i++)Stack(80+i*840.0/(stacks-1),80+(i%2)*28,4+(i<variation%3?1:0),(i%2==0?1:-1)*(2+variation%4));
        }
        // Rotated-rectangle SAT: only genuine intersections block a card.
        bool Overlap(int a,int b)
        {
            var p=points[a];var q=points[b];double pa=p.Angle*Math.PI/180,qa=q.Angle*Math.PI/180;
            foreach(double axis in new[]{pa,pa+Math.PI/2,qa,qa+Math.PI/2}){
                double projection=Math.Abs((q.X-p.X)*Math.Cos(axis)+(q.Y-p.Y)*Math.Sin(axis));
                double radius=35*(Math.Abs(Math.Cos(pa-axis))+Math.Abs(Math.Cos(qa-axis)))+49*(Math.Abs(Math.Sin(pa-axis))+Math.Abs(Math.Sin(qa-axis)));
                if(projection>=radius-3)return false;
            }return true;
        }
        return points.Select((p,i)=>new Slot(i%10,i/10){DrawX=(float)p.X,DrawY=(float)p.Y,Angle=(float)p.Angle,BlockedBy=Enumerable.Range(i+1,points.Count-i-1).Where(j=>Overlap(i,j)).ToArray()}).ToList();
    }
}
