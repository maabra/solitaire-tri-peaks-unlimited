namespace MeadowPeaks;
internal sealed class DealingCard:IVisualEffect
{
    readonly Card card;readonly bool face;readonly PointF from,to;readonly float angle,delay;float age;
    public bool Finished=>age>=delay+.43f;
    // Capture a card flight's endpoints, pose and delay for presentation-only playback.
    public DealingCard(Card card,bool face,PointF from,PointF to,float angle,float delay){this.card=card;this.face=face;this.from=from;this.to=to;this.angle=angle;this.delay=delay;}
    // Advance the deal animation's age using elapsed seconds.
    public void Advance(float seconds)=>age+=seconds;
    // Interpolate the card's entrance pose after its delay and draw through the artwork service.
    public void Draw(Graphics g,IGameArtwork artwork)
    {
        if(age<delay)return;float t=Math.Clamp((age-delay)/.43f,0,1),p=1-MathF.Pow(1-t,3);
        var state=g.Save();g.TranslateTransform(from.X+(to.X-from.X)*p,from.Y+(to.Y-from.Y)*p);g.RotateTransform(-8+(angle+8)*p);
        float flip=face?Math.Max(.08f,Math.Abs((t-.65f)/.35f)):1;g.ScaleTransform(Math.Min(1,flip),1);
        artwork.DrawCard(g,new(-38,-53,73,99),card,face&&t>.65f,false,false);g.Restore(state);
    }
}
