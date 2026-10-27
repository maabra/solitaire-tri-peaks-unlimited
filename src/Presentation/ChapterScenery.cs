namespace MeadowPeaks;
internal static class ChapterScenery
{
    static readonly Color[] colors={Color.FromArgb(246,212,123),Color.FromArgb(219,150,88),Color.FromArgb(216,239,255),Color.FromArgb(24,38,79),Color.FromArgb(170,122,196),Color.FromArgb(96,183,195),Color.FromArgb(214,191,124),Color.FromArgb(237,179,207)};
    // Paint deterministic chapter decoration using its palette and chapter identifier.
    public static void Draw(Graphics g,int chapter)
    {
        int biome=(chapter-1)%8;long seed=chapter;/* Advance the scenery-only deterministic generator without affecting deal randomness. */
    double Next(){seed=seed*48271%2147483647;return seed/2147483647.0;}
        if(chapter>1){using var tint=new SolidBrush(Color.FromArgb(biome==3?135:biome==2?85:35,colors[biome]));g.FillRectangle(tint,0,0,1280,800);}
        using var ink=new SolidBrush(biome==2?Color.FromArgb(190,Color.White):Color.FromArgb(200,colors[biome]));using var star=new Pen(Color.FromArgb(230,255,236,174),1.5f);
        for(int i=0;i<(biome==2?100:biome==3?65:32);i++)
        {
            float x=(float)Next()*1280,y=(float)Next()*800,size=2+(float)Next()*5;
            if(biome!=2&&biome!=3)x=i%2==1?1035+(float)Next()*220:(float)Next()*245;
            if(biome==3){g.DrawLine(star,x-3,y,x+3,y);g.DrawLine(star,x,y-3,x,y+3);}
            else if(biome==2)g.FillEllipse(ink,x-size*.5f,y-size*.5f,size,size);
            else{var state=g.Save();g.TranslateTransform(x,y);g.RotateTransform((float)Next()*180);g.FillEllipse(ink,-size,-size*.5f,size*2,size);g.Restore(state);}
        }
    }
}
