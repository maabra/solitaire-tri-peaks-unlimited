namespace MeadowPeaks;

public sealed partial class GameWindow
{
    int journalPage, mechanicsPage;
    void UpdatePeekControls()
    {
        bool show = (hand as ITemporaryHand)?.Effects.PeekSeconds is not > 0 || hand?.Won == true;
        foreach (string name in new[] { "mechanics", "remix", "shareDeal" })
            if (Controls.Find(name,false).FirstOrDefault() is { } control) control.Visible=show;
    }
    void CopyDeal()
    {
        if(session.ShareCode is not { } code || smoke) return;
        try { Clipboard.SetText(code); Tell("\U0001f4cb \u2713"); }
        catch(System.Runtime.InteropServices.ExternalException) { Tell("Clipboard is busy. Try again."); }
    }
    void ImportDeal()
    {
        using var dialog=new Form { Text="Open a deal code", ClientSize=new(590,165), FormBorderStyle=FormBorderStyle.FixedDialog, StartPosition=FormStartPosition.CenterParent, MaximizeBox=false, MinimizeBox=false };
        var label=new Label { Text="Paste a Meadow Peaks 1.6 deal code. Your current hand will be replaced.", Left=18, Top=16, Width=550, Height=35 };
        var input=new TextBox { Left=18, Top=57, Width=550, MaxLength=80 };
        var open=new Button { Text="Open", Left=295, Top=105, Width=130, DialogResult=DialogResult.OK };
        var cancel=new Button { Text="Cancel", Left=438, Top=105, Width=130, DialogResult=DialogResult.Cancel };
        dialog.Controls.AddRange(new Control[] {label,input,open,cancel}); dialog.AcceptButton=open; dialog.CancelButton=cancel;
        if(dialog.ShowDialog(this)!=DialogResult.OK) return;
        string code=input.Text;
        if(!DealCodes.TryParse(code,out var address) || address==null) { Tell("Invalid or incompatible deal code."); return; }
        if(!address.Endless && address.Level>profile.Unlocked) { Tell("\U0001f512 Unlock that campaign level first."); return; }
        ReplaceHand(()=> {
            if(!session.StartSharedDeal(code)) { Tell("Unable to open deal."); return; }
            screen="play"; overlay=""; selectedPowerup=null; cardHint=-1; Save(); Build(); PresentSceneEntrance();
        });
    }
    void BuildJournal()
    {
        var goals=session.Picnic.Goals;
        for(int i=0;i<goals.Count;i++)
        {
            var goal=goals[i];
            Button("picnic-"+goal.Id,goal.Claimed ? "\u2713" : $"+{goal.Coins} \U0001fa99",270+i*254,350,230,48,()=>{
                int coins=session.Picnic.Claim(goal.Id); if(coins<=0) return;
                Save(); Build(); RewardEffects(coins,new(640,370));
            },artwork.Gold,goal.Ready);
        }
        var favorites=profile.Favorites.OrderBy(n=>n).ToArray();
        journalPage=Math.Clamp(journalPage,0,Math.Max(0,(favorites.Length-1)/6));
        for(int i=0;i<Math.Min(6,favorites.Length-journalPage*6);i++)
        {
            int n=favorites[journalPage*6+i];
            Button("favorite-level"+n,$"\u2605 {n:N0}",280+i%3*244,486+i/3*53,228,43,()=>{mapPage=(n-1)/20;Navigate("map");});
        }
        Button("favoritePrev","\u2039",280,604,80,45,()=>{journalPage--;Build();},enabled:journalPage>0);
        Button("favoriteNext","\u203a",920,604,80,45,()=>{journalPage++;Build();},enabled:(journalPage+1)*6<favorites.Length);
        Button("journalImport","\U0001f517",405,604,200,45,ImportDeal);
        Button("starHunt","\u2605 \u2192",650,604,200,45,()=>{
            int next=profile.Stars.Where(kv=>kv.Value<3).Select(kv=>kv.Key).DefaultIfEmpty(profile.Unlocked).Min();
            mapPage=(next-1)/20;Navigate("map");
        });
    }
    void PaintJournal(Graphics g)
    {
        Art.Text(g,"\U0001f4d6",260,174,36,artwork.Ink,true,760,true);
        Art.Text(g,$"\u2605 {profile.Stars.Values.Sum():N0}    \u2691 {profile.Stars.Count:N0}    \u221e {profile.EndlessWins:N0}",280,228,18,artwork.Teal,true,720,true);
        var goals=session.Picnic.Goals;
        for(int i=0;i<goals.Count;i++)
        {
            var goal=goals[i]; int x=270+i*254;
            Art.Box(g,new(x-6,272,242,137),Color.FromArgb(228,234,208),15);
            Art.Text(g,$"{goal.Icon} {Math.Min(goal.Current,goal.Target)}/{goal.Target}",x,290,23,artwork.Ink,true,230,true);
        }
        Art.Text(g,"\U0001f381 Daily picnic: finish all three for one consumable. No level timers.",280,423,15,artwork.Teal,false,720,true);
        Art.Text(g,$"\u2605 {profile.Favorites.Count:N0}    \u2022    {journalPage+1}",280,455,17,artwork.Teal,true,720,true);
        if(profile.Favorites.Count==0) Art.Text(g,"Tap the star on a campaign level to save it here.",280,514,18,artwork.Teal,false,720,true);
    }
}
