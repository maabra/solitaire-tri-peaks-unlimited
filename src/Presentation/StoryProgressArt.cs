namespace MeadowPeaks;

// Presentation-only preview of the existing Economy star thresholds.
internal static class StoryProgressArt
{
    public static void Draw(Graphics g, RectangleF bounds, int bestStreak, int cardsLeft)
    {
        int stars=bestStreak>=9?3:bestStreak>=5?2:1;
        Art.Box(g,new(bounds.X+9,bounds.Y+12,bounds.Width-18,16),Color.FromArgb(76,60,87),8);
        float fill=(bounds.Width-22)*Math.Min(1,bestStreak/9f);
        if(fill>0)Art.Box(g,new(bounds.X+11,bounds.Y+14,fill,12),Color.FromArgb(104,203,117),6);
        for(int i=0;i<3;i++)FestiveArt.Star(g,bounds.X+14+i*(bounds.Width-28)/2,bounds.Y+18,14,i<stars?Color.FromArgb(255,204,64):Color.FromArgb(139,127,150));
        Art.Text(g,$"{cardsLeft} left  |  streak 5 / 9",bounds.X,bounds.Y+42,11,Color.FromArgb(36,76,67),true,bounds.Width,true);
    }
}
