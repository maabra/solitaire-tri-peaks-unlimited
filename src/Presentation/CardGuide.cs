namespace MeadowPeaks;

internal sealed record CardGuideEntry(CardKind Kind,string Name,string Icon,int Unlock,string Rule);
internal static class CardGuide
{
    public static readonly CardGuideEntry[] Entries={
        new(CardKind.Normal,"Normal card","\u2191\u2193",1,"Play one rank above or below the pile. Aces connect Kings and Twos."),
        new(CardKind.Gift,"Stock gift","+3",50,"Clear it to add three cards to the end of your stock."),
        new(CardKind.Ice,"Ice","\u2744",100,"Clear other cards to melt its counter. Drawing does not melt ice."),
        new(CardKind.Lock,"Keys and locks","\U0001f511",200,"Collect the required keys first. Then match the locked card by rank."),
        new(CardKind.Bomb,"Move-count bomb","\U0001f4a3",300,"Clear before its fuse reaches zero. Draws, clears and armor hits use fuse."),
        new(CardKind.Up,"Direction bonus","\u2191+2",400,"Play in the arrow direction to add two stock cards. King to Ace is up."),
        new(CardKind.Stone,"Armor","\u25c6",500,"Each legal tap removes a layer. The displayed count tells you hits left."),
        new(CardKind.Lantern,"Lantern","\U0001f526",600,"Clear it to highlight matching cards for four seconds."),
        new(CardKind.Lucky,"Clover","\U0001f340",700,"Clear it for six seconds of +1 stock card per card cleared."),
        new(CardKind.Vine,"Vines","\U0001f33f",800,"Build the shown clear streak without drawing. Then play this card."),
        new(CardKind.Shield,"Shield","\U0001f6e1",900,"Clear it to pause bomb fuses for six seconds."),
        new(CardKind.Gate,"Sun / moon gate","\u25d0",2000,"Match the board phase. Every successful move switches sun and moon."),
        new(CardKind.ColorSeal,"Color seal","\u2665",2300,"Match rank AND pile color: heart means red, spade means black.")
    };
    public static string Describe(ISolitaireHand hand,int index)
    {
        var card=hand.Deal.Cards[index];
        var kind=card.Kind==CardKind.Key ? CardKind.Lock : card.Kind==CardKind.Down ? CardKind.Up : card.Kind;
        var entry=Entries.First(e=>e.Kind==kind);
        string detail=card.Kind switch {
            CardKind.Key=>"Clear this key to help open locked cards.",
            CardKind.ColorSeal=>card.Strength==1 ? "Needs a red card on the pile, or a Wild, as well as a matching rank." : "Needs a black card on the pile, or a Wild, as well as a matching rank.",
            _=>entry.Rule };
        return $"{Art.Rank(card.Rank)} - {entry.Name}\n{detail}";
    }
}
public sealed partial class GameWindow
{
    string PowerupCaption(IPowerup helper)
    {
        if(hand==null) return helper.Description.Replace("\n"," ");
        if(hand is IAdventureHand {Failed:true}) return "Undo or replay this deal";
        if(hand is ITemporaryHand timed && timed.Effects.HelperCooldown>0) return $"Ready in {Math.Ceiling(timed.Effects.HelperCooldown)}s";
        if(!helper.CanUse(hand)) return helper.Id switch {
            "extra"=>"Use when stock is empty", "wild"=>"Wild already on the pile", "thaw"=>"Needs ice / may be active", "shield"=>"Needs a bomb / may be active",
            "keypass"=>"Needs a lock / may be active", "prune"=>"Needs vines / may be active", "phase"=>"Needs a gate / may be active", "peek"=>"Needs stock / may be active", _=>"Not available right now" };
        if(session.Inventory.Count(helper.Id)==0) return $"Buy in shop - {helper.Price} coins";
        return helper.Id switch {
            "wild"=>"Any rank once - 12s", "extra"=>"Five rescue cards - 10s", "remove"=>"Choose an exposed card",
            "lucky"=>"+1 stock per clear - 10s", "thaw"=>"Ignore ice - 8s", "shield"=>"Pause bomb fuses - 10s", "scout"=>"Show matches - 5s",
            "keypass"=>"Ignore locks - 6s", "prune"=>"Ignore vines - 8s", "phase"=>"Open phase gates - 6s", "peek"=>"See next 3 draws - 6s", _=>"One-use helper" };
    }
    void PaintCardGuide(Graphics g)
    {
        Art.Text(g,"Know your cards",260,170,34,artwork.Ink,true,760,true);
        Art.Text(g,$"Page {mechanicsPage+1} of 2 - new mechanics arrive gradually",280,217,16,artwork.Teal,false,720,true);
        var entries=CardGuide.Entries.Skip(mechanicsPage*8).Take(8).ToArray();
        for(int i=0;i<entries.Length;i++)
        {
            var e=entries[i];float x=262+i%2*379,y=250+i/2*82;
            Art.Box(g,new(x,y,365,76),Color.FromArgb(234,239,218),12);
            artwork.DrawCard(g,new(x+9,y+7,43,60),new Card(6,1){Kind=e.Kind,Strength=e.Kind==CardKind.Stone?2:1},true,false,false);
            Art.Box(g,new(x+11,y+46,39,17),artwork.Teal,3);Art.Text(g,e.Icon,x+11,y+47,11,Color.White,true,39,true);
            Art.Text(g,e.Name,x+64,y+6,17,artwork.Ink,true);
            Art.Text(g,e.Unlock==1 ? "Basics" : $"Level {e.Unlock:N0}",x+250,y+10,11,artwork.Teal,false,104,true);
            Art.Text(g,e.Rule,x+64,y+30,13,artwork.Teal,false,288);
        }
    }
    void PaintHowToPlay(Graphics g)
    {
        Art.Text(g,"Clear the peaks, one card at a time",250,173,31,artwork.Ink,true,780,true);
        Art.Text(g,"Pick an exposed card one rank higher or lower than the pile.",280,225,18,artwork.Teal,false,720,true);
        artwork.DrawCard(g,new(441,268,74,103),new(4,1),true,true,false);
        artwork.DrawCard(g,new(603,268,74,103),new(5,0),true,false,false);
        artwork.DrawCard(g,new(765,268,74,103),new(6,3),true,true,false);
        Art.Text(g,"\u2192",533,300,32,artwork.Teal,true);
        Art.Text(g,"\u2190",700,300,32,artwork.Teal,true);
        Art.Text(g,"4 plays",425,383,17,artwork.Teal,true,108,true);Art.Text(g,"Pile: 5",585,383,17,artwork.Ink,true,108,true);Art.Text(g,"6 plays",748,383,17,artwork.Teal,true,108,true);
        Art.Text(g,"1. Clear covering cards to reveal the cards underneath.",292,433,18,artwork.Ink);
        Art.Text(g,"2. No match? Draw a new card from the stock. Aces wrap to Kings.",292,471,18,artwork.Ink);
        Art.Text(g,"3. Clear the whole board to win coins. Helpers are optional and one-use.",292,509,18,artwork.Ink);
        Art.Text(g,"No level countdown. Hints and boosts last only a few seconds.\nKeys: D draw - U undo - H hint - W Wild - Esc back - F11 maximize",280,551,14,artwork.Teal,false,720,true);
    }
}
