namespace MeadowPeaks;
public static class HelperDiscovery
{
    // Check progress and recorded mechanic encounters before revealing this helper.
    public static bool Discovered(Progress p,string id)=>id switch
    {
        "thaw"=>p.SeenMechanics.Contains(CardKind.Ice),"keypass"=>p.SeenMechanics.Contains(CardKind.Lock),
        "shield"=>p.SeenMechanics.Contains(CardKind.Bomb),"prune"=>p.SeenMechanics.Contains(CardKind.Vine),
        "phase"=>p.SeenMechanics.Contains(CardKind.Gate),"lucky"=>p.SeenMechanics.Contains(CardKind.Lucky),
        "scout"=>p.HighestPlayedLevel>=35,"peek"=>p.HighestPlayedLevel>=50,_=>true
    };
    // Check the live hand for an obstacle or state that the helper can address.
    public static bool Relevant(ISolitaireHand? hand,string id)
    {
        if(hand==null)return true;
        CardKind? target=id switch{"thaw"=>CardKind.Ice,"keypass"=>CardKind.Lock,"shield"=>CardKind.Bomb,"prune"=>CardKind.Vine,"phase"=>CardKind.Gate,_=>null};
        if(target==null)return true;
        return hand.Deal.Cards.Where((c,i)=>!hand.Removed[i]).Any(c=>c.Kind==target && (id!="thaw" || hand is not ITemporaryHand timed || c.Strength>timed.Effects.Clears));
    }
}
