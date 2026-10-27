namespace MeadowPeaks;
public sealed partial class GameWindow
{
    WheelPrize? wheelPrize;double wheelStarted=-100;float wheelAngle;bool wheelSpinning;
    // Request the session's saved prize before beginning the native spin animation.
    void SpinWheel()
    {
        if(wheelSpinning)return;var result=session.SpinWheel();if(result==null)return;
        wheelPrize=result;wheelStarted=effects.Time;wheelSpinning=!profile.ReducedMotion;wheelAngle=profile.ReducedMotion?-result.Index*45:0;
        Chime(SoundCue.Coins);Build();
    }
    // Advance the native wheel toward the already selected prize segment.
    void AdvanceWheel()
    {
        if(!wheelSpinning||wheelPrize==null)return;
        float t=Math.Clamp((float)(effects.Time-wheelStarted)/4.2f,0,1);wheelAngle=(1800-wheelPrize.Index*45)*(1-MathF.Pow(1-t,4));
        if(t>=1){wheelSpinning=false;if(overlay=="wheel"){Build();effects.Burst(new(640,390),artwork.Gold,28);}}
        if(overlay=="wheel"){backdropDirty=true;Invalidate();}
    }
    // Draw the wheel at its current presentation angle and label its prizes.
    void PaintWheel(Graphics g)
    {
        AdventureMapArt.Ribbon(g,new(315,142,650,75),"Lucky meadow wheel","A LITTLE SPIN, A LOVELY SURPRISE");
        LuckyWheelArt.Draw(g,new(640,390),142,wheelAngle,session.Wheel.Prizes);
        Art.Text(g,wheelSpinning?"A lovely surprise is on its way...":wheelPrize!=null?"You found "+wheelPrize.Label+"!":"Coins and one-use helpers await.",320,540,20,artwork.Ink,true,640,true);
        Art.Text(g,"One welcome spin. Earn another every 5 wins.",320,654,15,artwork.Teal,true,640,true);
    }
}
