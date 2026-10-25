using System.Drawing.Drawing2D;

namespace MeadowPeaks;
public sealed partial class GameWindow : Form
{
    readonly GameSession session;
    readonly IGameArtwork artwork;
    readonly ISoundPlayer sound;
    Progress profile => session.Profile;
    readonly bool smoke;
    readonly bool startupCheck;
    string screen = "map", overlay = "", message = "Welcome to the meadow. Take your time.";
    ISolitaireHand? hand => session.Hand;
    ICoinGame? gems => session.CoinGame;
    int level => session.Level;
    bool endless => session.Endless;
    int mapPage, selectedGem = -1, gemHint = -1, cardHint = -1, powerupPage;
    string? selectedPowerup;
    int reward, stars;
    float scale = 1, offsetX, offsetY;
    Action? confirmAction;
    readonly ToolTip iconTips = new() { AutoPopDelay = 12000 };
    readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 5000 };
    readonly System.Windows.Forms.Timer animation = new() { Interval = 33 };
    bool rebuilding;
    public GameWindow(GameSession session, IGameArtwork artwork, ISoundPlayer sound, bool smoke = false, EffectScene? visualEffects = null, bool startupCheck = false)
    {
        this.session = session;
        this.artwork = artwork;
        this.sound = sound;
        effects = visualEffects ?? new EffectScene();
        this.smoke = smoke; this.startupCheck = startupCheck;
        Text = "Meadow Peaks • Solitaire Unlimited";
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        ClientSize = new(1280, 800); MinimumSize = new(1000, 680); StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true; KeyPreview = true; BackColor = artwork.Teal;
        if (session.LoadWarning.Length > 0) message = session.LoadWarning;
        mapPage = (profile.Unlocked - 1) / 20;
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); message = ""; backdropDirty = true; Invalidate(); };
        animation.Tick += AnimationTick;
        effectsWindow = new EffectsOverlay(effects, artwork);
        LocationChanged += (_, _) => effectsWindow?.Follow(this, !smoke);
        Deactivate += (_, _) => effectsWindow?.Hide();
        Resize += (_, _) => Build();
        FormClosing += (_, _) => Save();
        Shown += (_, _) => { Build(); lastFrame = presentationClock.Elapsed.TotalSeconds; animation.Start(); StartupDiagnostics.Write("MeadowPeaks-startup.txt", "PASS: game window shown. Profile loaded; active level " + session.Level); if (startupCheck) BeginInvoke(new Action(Close)); else if (smoke) BeginInvoke(new Action(Smoke)); };
        KeyDown += KeysPressed;
        AccessibleName = "Meadow Peaks Solitaire";
        Build();
    }
    void Save()
    {
        if(startupCheck) return;
        try { session.Save(); }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { message = "Progress could not be saved. Check access to your Local AppData folder."; Invalidate(); }
    }
    void Tell(string text)
    {
        message = text; toastTimer.Stop(); toastTimer.Start(); backdropDirty = true; Invalidate();
    }
    void Chime(SoundCue cue = SoundCue.Card)
    {
        if (profile.Sound && !smoke) { if (sound is IGameSoundtrack soundtrack) soundtrack.Play(cue); else sound.Play(); }
    }
    Rectangle Rect(float x, float y, float w, float h) => new((int)(offsetX + x * scale), (int)(offsetY + y * scale), Math.Max(1, (int)(w * scale)), Math.Max(1, (int)(h * scale)));
    GameButton Button(string name, string text, float x, float y, float w, float h, Action action, Color? fill = null, bool enabled = true)
    {
        var b = new GameButton(artwork) { Name = name, Text = text, AccessibleName = text.Replace("\n", " "), Bounds = Rect(x, y, w, h), Fill = fill ?? artwork.Cream, Enabled = enabled, TabIndex = Controls.Count };
        string description = name switch { "mechanics" => "Special cards guide", "hint" => "Show a match for 3 seconds. Recharges in 15 seconds.", "picnic-wins" => "Win two hands", "picnic-clears" => "Clear 24 board cards", "picnic-gems" => "Earn 80 coins in Gem Garden", "journal" => "Journal: daily picnic, favorites and star hunt", "importDeal" or "shareDeal" or "journalImport" => "Share or open a reproducible deal code", "starHunt" => "Find a completed level with fewer than three stars", "favorite" => "Add or remove this level from favorites", "remix" => "Deal fresh cards at this level. Current hand is replaced.", _ => name.StartsWith("power-") ? session.Powerups.Find(name.Substring(6))?.Description ?? "" : "" };
        if (description.Length > 0) { b.AccessibleName = description; iconTips.SetToolTip(b, description); }
        b.Advance(effects.Time, profile.ReducedMotion);
        b.Click += (_, _) => action(); Controls.Add(b); return b;
    }
    void Build()
    {
        if (rebuilding || IsDisposed) return;
        effects.ReducedMotion = profile.ReducedMotion;
        rebuilding = true;
        var focused = ActiveControl?.Name;
        SuspendLayout();
        var old = Controls.Cast<Control>().ToArray(); Controls.Clear(); foreach (var c in old) c.Dispose();
        scale = Math.Min(ClientSize.Width / 1280f, ClientSize.Height / 800f);
        offsetX = (ClientSize.Width - 1280 * scale) / 2; offsetY = (ClientSize.Height - 800 * scale) / 2;
        if (overlay.Length > 0) BuildOverlay();
        else
        {
            if (screen != "home") Button("home", "‹  Home", 38, 26, 114, 46, () => Navigate("home"));
            var walletButton=Button("wallet", $"●  {profile.Coins:N0}   +", 1025, 26, 215, 46, () => ShowOverlay("shop"), artwork.Gold);
            if(screen=="map"){walletButton.Fill=Color.FromArgb(80,37,83);walletButton.Fore=Color.White;}
            Button("settings", "⚙", screen == "home" ? 38 : 168, 26, 48, 46, () => ShowOverlay("settings"));
            if (screen == "home") BuildHome();
            if (screen == "map") BuildMap();
            if (screen == "play") BuildHand();
            if (screen == "gems") BuildGems();
        }
        if(screen=="play" && overlay.Length==0) UpdatePeekControls();
        if (focused != null && Controls.Find(focused, false).FirstOrDefault() is { Enabled: true } found) found.Focus();
        backdropDirty = true; RefreshBackdrop(); ResumeLayout(); rebuilding = false; Invalidate();
    }
    void Navigate(string target) { overlay = ""; selectedPowerup = null; gemAnimation = null; effects.Clear(); screen = target; Save(); Build(); PresentSceneEntrance(); }
    void ShowOverlay(string name) { overlay = name; selectedPowerup = null; gemAnimation = null; effects.Clear(); Build(); }
    void BuildHome()
    {
        Button("adventure", hand is {Won:false} ? $"Resume level {level:N0}  \u2192" : "Play adventure  \u2192", 88, 402, 345, 65, () => { if(hand is {Won:false}) Navigate("play"); else { mapPage = (profile.Unlocked - 1) / 20; Navigate("map"); } }, artwork.Gold);
        Button("endless", "∞   Endless meadow", 88, 482, 345, 58, StartEndless);
        Button("gems", "◆   Gem garden · earn coins", 88, 554, 345, 58, OpenGems);
        if (hand != null && !hand.Won) Button("resume", "Browse adventure map", 88, 626, 345, 48, () => { mapPage=(profile.Unlocked-1)/20; Navigate("map"); }, Color.FromArgb(195, 225, 196));
        Button("how", "How to play", 88, 695, 160, 42, () => ShowOverlay("help"));
        Button("daily", session.DailyClaimed ? "Gift collected ✓" : $"Daily gift  +{session.Economy.DailyGift}", 265, 695, 168, 42, ClaimDaily, artwork.Gold, !session.DailyClaimed);
        Button("journal", "\U0001f4d6 Journal", 755, 520, 190, 47, () => ShowOverlay("journal"));
        Button("importDeal", "\U0001f517 Open deal", 960, 520, 190, 47, ImportDeal);
        if (session.GardenGifts > 0) Button("gardenGift", $"Collect garden gift · +{session.Economy.GardenGift}", 810, 651, 325, 50, ClaimGarden, artwork.Gold);
    }
    void ClaimDaily()
    {
        if (session.DailyClaimed) return;
        int coins = session.ClaimDaily();
        Save(); Tell($"A little gift for a lovely day. +{coins} coins!"); Build(); RewardEffects(coins, new(348, 695));
    }
    void ClaimGarden()
    {
        if (session.GardenGifts == 0) return;
        int coins = session.ClaimGarden();
        Save(); Tell($"Your garden thanks you. +{coins} coins!"); Build(); RewardEffects(coins, new(960, 651));
    }
    int previewLevel;
    void PreviewLevel(int n){previewLevel=n;ShowOverlay("levelPreview");}
    void BuildMap()
    {
        for(int i=0;i<20;i++)
        {
            int n=mapPage*20+i+1;var p=MapPoint(i);bool current=n==profile.Unlocked;
            var node=Button("level"+n,n.ToString(),p.X-33,p.Y-28,66,59,()=>PreviewLevel(n),current?Color.FromArgb(240,183,51):profile.Stars.ContainsKey(n)?Color.FromArgb(115,178,52):Color.FromArgb(138,160,122),n<=profile.Unlocked);
            node.MapNode=true;node.CurrentNode=current;node.AccessibleName=$"Level {n}";
        }
        var activities=new[]{("gems","Gem Garden"),("wheel","Lucky wheel"),("journal","Journal"),("endless","Endless")};
        for(int i=0;i<activities.Length;i++){var item=activities[i];var b=Button("map-"+item.Item1,item.Item2,24,220+i*99,90,86,()=>{if(item.Item1=="gems")OpenGems();else if(item.Item1=="endless")StartEndless();else ShowOverlay(item.Item1);},Color.FromArgb(184,62,82));b.MenuIcon=item.Item1;}
        Button("prev","\u2039",170,720,58,49,()=>{mapPage--;Build();PresentSceneEntrance();},Color.FromArgb(154,87,173),mapPage>0);
        Button("current","My next level",405,720,260,52,()=>{mapPage=(profile.Unlocked-1)/20;Build();},Color.FromArgb(106,206,47));
        Button("jump","Go to level...",690,724,170,44,JumpLevel,Color.FromArgb(246,217,167));
        Button("next","\u203a",1035,720,58,49,()=>{mapPage++;Build();PresentSceneEntrance();},Color.FromArgb(154,87,173),mapPage<campaign.Chapters.Count-1);
    }
    static PointF MapPoint(int i)=>AdventureMapArt.Route[i];
    void JumpLevel()
    {
        using var dialog = new Form { Text = "Find a level", ClientSize = new(330, 155), FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false };
        var label = new Label { Text = $"Choose a level (1–{campaign.LevelCount:N0}). Unlocked: {profile.Unlocked}", Left = 18, Top = 18, Width = 300 };
        var number = new NumericUpDown { Minimum = 1, Maximum = campaign.LevelCount, Value = profile.Unlocked, Left = 20, Top = 50, Width = 285 };
        var ok = new Button { Text = "Show on map", DialogResult = DialogResult.OK, Left = 155, Top = 95, Width = 150 };
        dialog.Controls.AddRange(new Control[] { label, number, ok }); dialog.AcceptButton = ok;
        if (dialog.ShowDialog(this) == DialogResult.OK) { mapPage = ((int)number.Value - 1) / 20; Build(); }
    }
    void ReplaceHand(Action action)
    {
        if (hand != null && !hand.Won) { confirmAction = action; ShowOverlay("replace"); }
        else action();
    }
    void StartAdventure(int n)
    {
        if (n > profile.Unlocked || n < 1 || n > campaign.LevelCount) return;
        if (hand != null && !hand.Won && !endless && level == n) { Navigate("play"); return; }
        ReplaceHand(() => NewHand(n, false));
    }
    void StartEndless()
    {
        if (hand != null && !hand.Won && endless) { Navigate("play"); return; }
        ReplaceHand(() => NewHand(profile.EndlessDeal, true));
    }
    void NewHand(int n, bool infinite)
    {
        session.StartHand(n, infinite);
        cardHint = -1; selectedPowerup = null; overlay = ""; screen = "play"; Save(); Build(); PresentSceneEntrance();
        if (!infinite && (n - 1) % campaign.LevelsPerChapter == 0) effects.Burst(new(1100, 179), WorldPalette.For(CurrentChapter.Theme).Accent, 20);
        Tell(n == 1 && !infinite ? "\u2660 \u2191 / \u2193     \U0001f50e 3s" : n % 100 == 0 ? $"\u2b06 {hand?.Deal.Difficulty?.Badge}" : hand?.Deal.Bonus == true && !profile.Stars.ContainsKey(n) && !infinite ? "\U0001f381 +125 \U0001fa99 +1 \u2728" : $"\u2660 {hand?.Deal.Layout.Max(s => s.Row) + 1} \u2195    \u2728");
    }
    void BuildHand()
    {
        if (hand == null) { screen = "home"; BuildHome(); return; }
        for(int i=0;i<hand.Removed.Count;i++) EnsureCardControl(i);
        if (!endless) Button("favorite", profile.Favorites.Contains(level) ? "\u2605 Saved" : "\u2606 Save", 449, 117, 88, 42, () => { session.ToggleFavorite(level); Save(); Build(); }, artwork.Gold);
        var draw = Button("draw", "", 426, 540, 86, 119, Draw);
        draw.CardValue = new(1, 0); draw.Face = false; draw.AccessibleName = $"Draw a card. {hand.Stock.Count} left"; draw.Enabled = hand.Stock.Count > 0 && !hand.Won && (hand as IAdventureHand)?.Failed != true;
        var waste = Button("waste", "", 561, 540, 86, 119, () => Tell("Play one rank higher or lower. Aces connect Kings and Twos."));
        waste.CardValue = hand.Waste; waste.TabStop = false; waste.AccessibleName = "Waste pile " + Art.Rank(hand.Waste.Rank);
        Button("hint", "\U0001f50e Hint 3s", 742, 551, 156, 47, Hint, enabled:(hand as ITemporaryHand)?.Effects.HintCooldown<=0);
        Button("retry", "Replay deal", 742, 609, 156, 43, () => { confirmAction = () => { session.Replay(); selectedPowerup = null; overlay = ""; Save(); Build(); }; ShowOverlay("retry"); });
        Button("remix", profile.FreshDealCooldown>0 ? $"Fresh in {Math.Ceiling(profile.FreshDealCooldown)}s" : "\U0001f3b2 Remix", 140, 609, 110, 43, () => { confirmAction = () => { session.Remix(); selectedPowerup = null; overlay = ""; Save(); Build(); }; ShowOverlay("remix"); },enabled:profile.FreshDealCooldown<=0);
        Button("shareDeal", "\U0001f517 Share", 260, 609, 110, 43, () => ShowOverlay("share"));
        Button("undo", hand.FreeUndos > 0 ? $"↶  Undo · {hand.FreeUndos} left" : $"↶  Undo · {session.Economy.UndoCost} coins", 158, 698, 210, 57, Undo, enabled: hand.CanUndo);
        var helpers = VisibleHelpers().Skip(powerupPage * 3).Take(3).ToArray();
        for (int i = 0; i < helpers.Length; i++)
        {
            var helper = helpers[i];
            var powerButton=Button("power-" + helper.Id, $"{helper.Name}  ×{session.Inventory.Count(helper.Id)}", 383 + i * 225, 698, 210, 57, () => ActivatePowerup(helper.Id), enabled: helper.CanUse(hand) && (hand as ITemporaryHand)?.Effects.HelperCooldown <= 0);
            powerButton.Subtitle=PowerupCaption(helper);
        }
        BuildPowerupPages(698);
        Button("mechanics", "\u2753 Card guide", 140, 551, 230, 47, () => ShowOverlay("mechanics"));
        Button("help", "?", 1163, 108, 49, 46, () => ShowOverlay("help"));
        if (hand.Won) Button("finishedNext", "Next hand  →", 920, 566, 230, 53, () => NewHand(endless ? profile.EndlessDeal : Math.Min(level + 1, campaign.LevelCount), endless || level == campaign.LevelCount), artwork.Gold);
        if (!hand.Moves.Any() && hand.Stock.Count == 0 && !hand.Won) Button("earn", "Earn coins  ◆", 920, 566, 230, 53, OpenGems, artwork.Gold);
    }
    void EnsureCardControl(int i)
    {
            if (hand==null) return;
            int index = i; var slot = hand.Deal.Layout[i]; bool face = hand.IsExposed(i);
            if (!face || hand.Removed[i]) { if(Controls.Find("card"+i,false).FirstOrDefault() is Control old){Controls.Remove(old);old.Dispose();}return;}
            var bounds = CardBounds(i);
            var b = Controls.Find("card"+i,false).FirstOrDefault() as GameButton; if(b==null) b = Button("card" + i, "", bounds.X, bounds.Y, bounds.Width, bounds.Height, () => PlayCard(index));
            b.CardValue = hand.Deal.Cards[i]; b.Face = face;
            if(slot.DrawX.HasValue && b.CardSize.Width==0)
            {
                b.CardAngle=slot.Angle;b.CardSize=new(76,106);
                using var hit=new GraphicsPath();hit.AddRectangle(new RectangleF(-37,-52,74,104));
                using var matrix=new Matrix();matrix.Rotate(slot.Angle);matrix.Translate(b.Width/2f,b.Height/2f,MatrixOrder.Append);hit.Transform(matrix);b.Region=new Region(hit);
            }
            b.CardBadge = (hand as IAdventureHand)?.CardStatus(i) ?? "";
            b.Hint = face && (selectedPowerup != null || (hand as ITemporaryHand)?.Effects.HintSeconds > 0 && ((profile.Hints && hand.CanPlay(i)) || cardHint == i));
            b.AccessibleName = face ? $"{Art.Rank(b.CardValue.Rank)} of {new[] { "spades", "hearts", "diamonds", "clubs" }[b.CardValue.Suit]}, {b.CardBadge}{(hand.CanPlay(i) ? ", playable" : "")}" : "Covered card";
            iconTips.SetToolTip(b, CardGuide.Describe(hand,i));
            b.AccessibleDescription=CardGuide.Describe(hand,i);
            b.TabStop = face; b.Enabled = face;
            b.BringToFront();
    }
    void RefreshHandView()
    {
        if(hand==null||screen!="play"||overlay.Length>0){Build();return;}
        for(int i=0;i<hand.Removed.Count;i++)EnsureCardControl(i);
        if(Controls.Find("waste",false).FirstOrDefault() is GameButton waste){waste.CardValue=hand.Waste;waste.Invalidate();}
        if(Controls.Find("draw",false).FirstOrDefault() is GameButton draw){draw.Enabled=hand.Stock.Count>0&&!hand.Won&&!((hand as IAdventureHand)?.Failed??false);draw.Invalidate();}
        if(Controls.Find("undo",false).FirstOrDefault() is GameButton undo){undo.Enabled=hand.CanUndo;undo.Text=hand.FreeUndos>0?$"Undo - {hand.FreeUndos} left":$"Undo - {session.Economy.UndoCost} coins";}
        foreach(var old in Controls.OfType<GameButton>().Where(b=>b.Name.StartsWith("power-")||b.Name is "prevPowerups" or "nextPowerups").ToArray()){Controls.Remove(old);old.Dispose();}
        var helpers=VisibleHelpers().Skip(powerupPage*3).Take(3).ToArray();
        for(int i=0;i<helpers.Length;i++){var h=helpers[i];var b=Button("power-"+h.Id,$"{h.Name} x{session.Inventory.Count(h.Id)}",383+i*225,698,210,57,()=>ActivatePowerup(h.Id),enabled:h.CanUse(hand)&&((hand as ITemporaryHand)?.Effects.HelperCooldown??0)<=0);b.Subtitle=PowerupCaption(h);}
        BuildPowerupPages(698);UpdatePeekControls();backdropDirty=true;RefreshBackdrop();Invalidate();
    }
    void PlayCard(int i)
    {
        if (hand == null) return;
        if (selectedPowerup != null)
        {
            string helperName = session.Powerups.Find(selectedPowerup)?.Name ?? "Helper";
            if (session.UsePowerup(selectedPowerup, i) == CommandResult.Applied)
            {
                effects.Burst(CardCenter(i), Color.FromArgb(203, 155, 244), 28);
                selectedPowerup = null; Tell(helperName + " used."); Changed();
            }
            return;
        }
        var previous = ExposedCards(); int oldScore = hand.Score; int oldStock = hand.Stock.Count;
        var card = hand.Deal.Cards[i]; var origin = CardCenter(i);
        if (!session.PlayCard(i)) { Tell((hand as IAdventureHand)?.BlockReason(i) ?? $"That card needs a {Art.Rank(card.Rank == 1 ? 13 : card.Rank - 1)} or {Art.Rank(card.Rank == 13 ? 1 : card.Rank + 1)} on the pile."); return; }
        if (!hand.Removed[i])
        {
            effects.Burst(origin, Color.SlateGray, 28); effects.Cheer("\u25c6 \U0001f4a5", origin); cardHint = -1; Changed(); return;
        }
        if (card.Kind != CardKind.Normal)
        {
            effects.Burst(origin, card.Kind switch { CardKind.Ice => Color.LightSkyBlue, CardKind.Vine => Color.LightGreen, CardKind.Gate => Color.MediumPurple, CardKind.ColorSeal => card.Strength==1 ? Color.HotPink : Color.SlateGray, _ => artwork.Gold }, 18);
            effects.Cheer(card.Kind switch { CardKind.Key => "\U0001f511 \U0001f513", CardKind.Bomb => "\U0001f4a3 \u2713", CardKind.Ice => "\u2744 \u2728", CardKind.Gift => "\u2660 +3", CardKind.Up or CardKind.Down => hand.Stock.Count > oldStock ? "\u2660 +2" : "\u2728", CardKind.Vine => "\U0001f33f \u2728", CardKind.Lucky => "\U0001f340 +1 6s", CardKind.Lantern => "\U0001f526 4s", CardKind.Shield => "\U0001f6e1 6s", _ => "\u2728" }, new(1000, 550));
        }
        if (hand.Stock.Count > oldStock)
        {
            effects.Fly(hand.Stock[^1], origin, new(465, 593));
            effects.Burst(new(465, 593), Color.LightGreen, 12);
        }
        if (card.Kind == CardKind.Key)
            foreach (int locked in Enumerable.Range(0, hand.Deal.Cards.Count).Where(n => !hand.Removed[n] && hand.Deal.Cards[n].Kind == CardKind.Lock)) effects.Burst(CardCenter(locked), artwork.Gold, 8);
        effects.Fly(card, origin, new(604, 593)); effects.Burst(origin, artwork.Gold, 10);
        effects.Cheer($"+{hand.Score - oldScore}", new(origin.X, origin.Y - 35), Color.FromArgb(69, 139, 118));
        if (hand.Streak >= 3 && hand.Streak % 3 == 0)
        {
            string[] cheers = { "Lovely little run!", "Blooming brilliant!", "You're on a happy roll!", "A pocket full of sunshine!" };
            effects.Cheer(cheers[(hand.Streak / 3 - 1) % cheers.Length], new(989, 537), Color.FromArgb(149, 108, 175));
            Chime(SoundCue.Combo);
        }
        else Chime();
        cardHint = -1; Changed(); RevealCards(previous);
    }
    void Changed()
    {
        var result = session.CompleteHand();
        if (result != null)
        {
            stars = result.Stars;
            reward = result.Coins;
            overlay = "win"; lastVictory = result; victoryStarted = effects.Time;
            effects.Celebrate(); effects.Coins(result.Coins, new(640, 373)); Chime(SoundCue.Victory);
        }
        Save(); if(overlay.Length>0)Build();else RefreshHandView();
        if ((hand as IAdventureHand)?.Failed == true) Tell("A bomb burst! Undo your last move or replay this deal.");
    }
    void Draw() { if (session.Draw()) { effects.Fly(hand!.Waste, new(465, 593), new(604, 593)); selectedPowerup = null; cardHint = -1; Chime(); Changed(); } else Tell((hand as ITemporaryHand)?.Effects.DrawCooldown>0 ? $"Stock ready in {((ITemporaryHand)hand!).Effects.DrawCooldown:0.0}s" : "The stock is empty. Try a Wild, five more cards, or replay this deal."); }
    void Undo()
    {
        var result = session.Undo();
        if (result == CommandResult.InsufficientCoins) { ShowOverlay("shop"); return; }
        if (result == CommandResult.Applied) { selectedPowerup = null; Changed(); }
    }
    void Wild() => ActivatePowerup("wild");
    void ActivatePowerup(string id)
    {
        var helper = session.Powerups.Find(id);
        if (hand == null || helper == null || (hand as ITemporaryHand)?.Effects.HelperCooldown > 0 || !helper.CanUse(hand)) return;
        if (session.Inventory.Count(id) == 0) { ShowOverlay("shop"); return; }
        if (helper.RequiresTarget)
        {
            selectedPowerup = selectedPowerup == id ? null : id;
            Tell(selectedPowerup == null ? "Helper cancelled." : "Choose an exposed card. Click the helper again to cancel.");
            Build();
            return;
        }
        if (session.UsePowerup(id) == CommandResult.Applied)
        {
            effects.Burst(new(604, 593), Color.FromArgb(203, 155, 244), 35);
            effects.Cheer("\u2728 \u23f3", new(980, 537), Color.FromArgb(149, 108, 175));
            if (id is "thaw" or "shield" or "keypass" or "prune" or "phase")
                foreach (int special in Enumerable.Range(0, hand.Deal.Cards.Count).Where(n => !hand.Removed[n] && hand.Deal.Cards[n].Kind == (id switch { "thaw"=>CardKind.Ice, "shield"=>CardKind.Bomb, "keypass"=>CardKind.Lock, "prune"=>CardKind.Vine, _=>CardKind.Gate }))) effects.Burst(CardCenter(special), id == "thaw" ? Color.LightSkyBlue : Color.Turquoise, 14);
            selectedPowerup = null; Tell(helper.Name + " \u23f3"); Chime(SoundCue.Combo); Changed();
        }
    }
    IPowerup[] VisibleHelpers() {var visible=session.Powerups.All.Where(h=>session.HelperDiscovered(h.Id)&&(overlay=="shop"||screen!="play"||session.HelperRelevant(h.Id))).ToArray();powerupPage=Math.Clamp(powerupPage,0,Math.Max(0,(visible.Length-1)/3));return visible;}
    void BuildPowerupPages(float y)
    {
        int count=VisibleHelpers().Length; if (count <= 3) return;
        Button("prevPowerups", "‹", overlay == "shop" ? 233 : 105, y, 40, 48, () => { powerupPage--; Build(); }, enabled: powerupPage > 0);
        Button("nextPowerups", "›", overlay == "shop" ? 1008 : 1060, y, 40, 48, () => { powerupPage++; Build(); }, enabled: (powerupPage + 1) * 3 < count);
    }
    void Hint()
    {
        if (hand == null) return;
        if (hand is ITemporaryHand timed && !timed.RequestHint()) { Tell("Hint is recharging, or there is no matching card."); return; }
        cardHint = hand.Moves.FirstOrDefault(-1);
        if (cardHint >= 0) { Tell("Match highlighted for 3 seconds. Hints recharge in 15 seconds."); RefreshHandView(); Controls.Find("card" + cardHint, false).FirstOrDefault()?.Focus(); }
        else Tell(hand.Stock.Count > 0 ? "No cards fit right now. Draw from the stock (D)." : "No moves left. Use a helper, earn coins in Gem Garden, or replay the deal.");
    }
    void OpenGems() { session.OpenCoinGame(); selectedGem = -1; gemHint = -1; Navigate("gems"); }
    void BuildGems()
    {
        session.OpenCoinGame();
        var gems = session.CoinGame!;
        for (int i = 0; i < 64; i++)
        {
            int index = i;
            var b = Button("gem" + i, "", 432 + i % 8 * 64, 189 + i / 8 * 60, 60, 59, () => SelectGem(index));
            b.Gem = reducedGemFrame?.Cells[i] ?? gems.Cells[i];b.MatchGlow=reducedGemFrame?.Matched.Contains(i)==true?1:0; b.Hint = i == selectedGem || i == gemHint;
            b.AccessibleName = $"{new[] { "Ruby diamond", "Blue pearl", "Violet triangle", "Gold star", "Jade square", "Amber hexagon" }[b.Gem]} row {i / 8 + 1} column {i % 8 + 1}";
            b.Enabled = gems.Moves > 0 && gemAnimation == null;
        }
        Button("gemhint", "Show a match", 84, 524, 270, 52, () => { var hint = gems.Hint(); if (hint.HasValue) { selectedGem = hint.Value.Item1; gemHint = hint.Value.Item2; Build(); Tell("Swap the two golden gems."); } }, enabled: gems.Moves > 0 && gemAnimation == null);
        Button("gemnew", GardenRefreshCaption(), 84, 591, 270, 52, () => { if(!session.NewCoinGame()) return; selectedGem = -1; gemHint = -1; Build(); PresentSceneEntrance(); }, artwork.Gold, gemAnimation == null && session.GardenRefreshRemaining<=0);
        Button("backhand", hand != null && !hand.Won ? "Back to my hand  →" : "Back to adventure  →", 432, 702, 508, 53, () => Navigate(hand != null && !hand.Won ? "play" : "map"));
    }
    void SelectGem(int i)
    {
        if (gems == null || gems.Moves == 0 || gemAnimation != null) return;
        if (selectedGem == i) { selectedGem = -1; Build(); return; }
        if (selectedGem < 0) { selectedGem = i; gemHint = -1; Build(); return; }
        int a = selectedGem;
        if (Math.Abs(a / 8 - i / 8) + Math.Abs(a % 8 - i % 8) != 1) { selectedGem = i; Build(); return; }
        if (session.PlayGems(a, i))
        {
            Save(); Tell($"Lovely match! +{gems.LastGain} coins{(gems.Moves == 0 ? ". Start another round when Fresh Garden is ready." : "")}"); Chime(SoundCue.Gems);
            effects.Coins(gems.LastGain, new(686, 395));
            effects.Cheer($"Sparkling! +{gems.LastGain} coins", new(688, 142), Color.FromArgb(149, 108, 175));
            StartGemAnimation();
        }
        else Tell("Try a swap that makes a row or column of three. Invalid swaps cost no moves.");
        selectedGem = -1; gemHint = -1; Build();
    }
    void Buy(string id)
    {
        if (session.BuyPowerup(id) != CommandResult.Applied) { Tell("Need more coins? Earn coins in Gem Garden."); return; }
        Save(); Tell("Helper added to your collection."); Build(); effects.Burst(new(640, 440), artwork.Gold, 20); Chime(SoundCue.Coins);
    }
    void BuildOverlay()
    {
        Button("closeOverlay", "×", 1004, 148, 46, 44, () => { overlay = ""; Build(); });
        if(overlay=="wheel"){Button("spinWheel",$"Spin  |  {session.Wheel.Spins}",460,585,360,55,SpinWheel,Color.FromArgb(107,208,44),!wheelSpinning&&session.Wheel.Spins>0);}
        else if (overlay == "levelPreview")
        {
            Button("playPreview","Play",450,555,380,70,()=>{overlay="";StartAdventure(previewLevel);},Color.FromArgb(105,211,44));
        }
        else if (overlay == "shop")
        {
            var helpers = VisibleHelpers().Skip(powerupPage * 3).Take(3).ToArray();
            for (int i = 0; i < helpers.Length; i++)
            {
                var helper = helpers[i];
                Button("buy-" + helper.Id, $"Buy · {helper.Price} coins", 270 + i * 254, 464, 230, 52, () => Buy(helper.Id), artwork.Gold, profile.Coins >= helper.Price);
            }
            BuildPowerupPages(537);
            Button("earnGems", "◆   Earn coins in Gem Garden", 393, 581, 494, 57, OpenGems, Color.FromArgb(201, 224, 187));
        }
        else if (overlay == "win")
        {
            Button("nextHand", !endless && level == campaign.LevelCount ? "Enter the endless meadow  →" : "Next adventure  →", 439, 512, 402, 61, () => NewHand(endless ? profile.EndlessDeal : Math.Min(level + 1, campaign.LevelCount), endless || level == campaign.LevelCount), artwork.Gold);
            Button("winHome", "Back to the meadow", 439, 590, 402, 48, () => Navigate("home"));
        }
        else if (overlay == "help" || overlay == "mechanics")
        {
            Button("gotit", "Got it - let\u2019s play", 450, 602, 380, 56, () => { overlay = ""; Build(); }, artwork.Gold);
            if(overlay=="help") Button("helpCardGuide","Card guide \u2192",855,604,170,48,()=>{mechanicsPage=0;ShowOverlay("mechanics");});
            if(overlay=="mechanics")
            {
                Button("mechanicsPrev", "\u2190 Previous",255,604,170,48,()=>{mechanicsPage=0;Build();},enabled:mechanicsPage>0);
                Button("mechanicsNext", "Next \u2192",855,604,170,48,()=>{mechanicsPage=1;Build();},enabled:mechanicsPage==0);
            }
        }
        else if (overlay == "journal") BuildJournal();
        else if (overlay == "share") Button("copyDeal", "\U0001f4cb Copy deal code", 440, 525, 400, 55, CopyDeal, artwork.Gold, session.ShareCode != null);
        else if (overlay == "settings")
        {
            Button("sound", "Sound  " + (profile.Sound ? "ON" : "OFF"), 375, 315, 530, 55, () => { profile.Sound = !profile.Sound; Save(); Build(); });
            Button("hints", "Playable card glow  " + (profile.Hints ? "ON" : "OFF"), 375, 387, 530, 55, () => { profile.Hints = !profile.Hints; Save(); Build(); });
            Button("motion", "Reduced motion  " + (profile.ReducedMotion ? "ON" : "OFF"), 375, 459, 530, 55, () => { profile.ReducedMotion = !profile.ReducedMotion; Save(); Build(); });
            Button("fullscreen", WindowState == FormWindowState.Maximized ? "Restore window" : "Maximize window", 375, 531, 530, 55, () => { WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; Build(); });
        }
        else
        {
            Button("confirm", overlay == "retry" ? "Replay deal" : "Start a new hand", 419, 460, 442, 60, () => { overlay = ""; var a = confirmAction; confirmAction = null; a?.Invoke(); }, artwork.Gold);
            Button("cancel", "Keep my current hand", 419, 540, 442, 49, () => { overlay = ""; confirmAction = null; Build(); });
        }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); RefreshBackdrop();
        if (backdrop != null) e.Graphics.DrawImageUnscaled(backdrop, 0, 0);
    }
    void PaintBackdrop(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(offsetX, offsetY); g.ScaleTransform(scale, scale);
        if(screen=="map")AdventureMapArt.Landscape(g);else artwork.DrawLandscape(g, CurrentChapter.Theme);
        ChapterScenery.Draw(g,CurrentChapter.Number);
        if (screen != "home" && screen!="map") Art.Box(g, new(425, 24, 430, 48), Color.FromArgb(185, artwork.Cream), 20);
        if (screen != "home" && screen!="map") Art.Text(g, "MEADOW PEAKS", 445, 36, 23, artwork.Ink, true, 390, true);
        if (screen == "home") PaintHome(g);
        if (screen == "map") PaintMap(g);
        if (screen == "play") PaintHand(g);
        if (screen == "gems") PaintGems(g);
        if (message.Length > 0 && overlay.Length == 0)
        {
            Art.Box(g, new(125, 761, 1030, 32), Color.FromArgb(210, 26, 66, 53), 12);
            Art.Text(g, message, 140, 767, 14, artwork.Cream, false, 1000, true);
        }
        if (overlay.Length > 0) PaintOverlay(g);
    }
    void PaintHome(Graphics g)
    {
        Art.Box(g, new(58, 116, 406, 635), Color.FromArgb(225, 255, 249, 230), 29);
        Art.Text(g, "A LITTLE ESCAPE, EVERY DAY", 88, 146, 13, artwork.Teal, true);
        Art.Text(g, "Meadow\nPeaks", 82, 176, 72, artwork.Ink, true);
        Art.Text(g, "Solitaire, at your own pace.", 88, 354, 22, artwork.Teal);
        Art.Box(g, new(742, 155, 411, 106), Color.FromArgb(213, 255, 249, 230), 22);
        Art.Text(g, $"CHAPTER {campaign.ForLevel(profile.Unlocked).Number} OF {campaign.Chapters.Count}", 772, 175, 13, artwork.Teal, true);
        Art.Text(g, $"{campaign.ForLevel(profile.Unlocked).World}", 772, 199, 28, artwork.Ink, true);
        Art.Text(g, $"Level {profile.Unlocked:N0} of {campaign.LevelCount:N0}  ·  {profile.Stars.Values.Sum():N0} stars collected", 772, 237, 15, artwork.Teal);
        using var path = new Pen(Color.FromArgb(191, 180, 142), 22) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawCurve(path, new[] { new Point(666, 644), new Point(800, 543), new Point(743, 425), new Point(929, 353), new Point(1047, 307) }, .8f);
        int[] xx = { 695, 792, 752, 899, 1044 }, yy = { 615, 530, 424, 360, 310 };
        for (int i = 0; i < 5; i++)
        {
            Art.Box(g, new(xx[i] - 28, yy[i] - 28, 56, 56), i == 4 ? artwork.Gold : artwork.Cream, 28);
            Art.Text(g, i == 4 ? "★" : "✓", xx[i] - 28, yy[i] - 20, 28, artwork.Teal, true, 56, true);
        }
        FestiveArt.Pip(g, 652, 553, 1.05f);
        Art.Box(g, new(545, 455, 190, 43), Color.FromArgb(235, artwork.Cream), 19);
        Art.Text(g, "Pip is happy you're here!", 551, 466, 14, artwork.Teal, true, 178, true);
        artwork.DrawCard(g, new(521, 284, 97, 140), new(12, 1), true, false, false);
        artwork.DrawCard(g, new(555, 321, 97, 140), new(13, 0), true, true, false);
        Art.Box(g, new(769, 584, 405, 136), Color.FromArgb(222, 245, 242, 215), 22);
        Art.Text(g, "YOUR GROWING GARDEN", 795, 604, 14, artwork.Teal, true);
        Art.Text(g, $"{profile.Garden:N0} hands cleared  ·  {profile.EndlessWins:N0} endless wins", 795, 632, 19, artwork.Ink, true);
        if (session.GardenGifts <= 0) Art.Text(g, $"{session.Economy.GardenInterval - profile.Garden % session.Economy.GardenInterval} more clears to your next {session.Economy.GardenGift}-coin gift", 795, 668, 16, artwork.Teal);
    }
    void PaintMap(Graphics g)
    {
        AdventureMapArt.Trail(g);
        AdventureMapArt.Ribbon(g,new(433,85,420,72),CurrentChapter.World,$"CHAPTER {mapPage+1:N0}");
        int cleared=0;
        for(int i=0;i<20;i++)
        {
            var p=MapPoint(i);int n=mapPage*20+i+1;int earned=profile.Stars.GetValueOrDefault(n);if(earned>0)cleared++;
            if(earned>0)for(int j=0;j<earned;j++)FestiveArt.Star(g,p.X+(j-(earned-1)/2f)*18,p.Y+37,10,Color.FromArgb(255,217,67));
            if(n==profile.Unlocked){Art.Box(g,new(p.X-31,p.Y-60,62,24),Color.FromArgb(214,61,77),9);Art.Text(g,"PLAY",p.X-31,p.Y-56,12,Color.White,true,62,true);}
        }
        Art.Box(g,new(445,660,390,50),Color.FromArgb(238,255,247,221),17);
        Art.Text(g,$"{cleared}/20 blossoms  |  Gift: 150 coins + 1 Wild",450,671,12,artwork.Ink,true,380,true);
        Art.Box(g,new(460,695,360,7),Color.FromArgb(190,190,157),4);if(cleared>0)Art.Box(g,new(460,695,18*cleared,7),Color.FromArgb(122,195,67),4);
        FestiveArt.Pip(g,1127,585,.8f);Art.Box(g,new(1042,623,190,60),Color.FromArgb(245,255,247,220),16);Art.Text(g,"Every little step is\nsomething lovely.",1050,635,15,artwork.Teal,true,174,true);
    }
    void PaintHand(Graphics g)
    {
        if (hand == null) return;
        Art.Box(g, new(117, 93, 1047, 82), Color.FromArgb(220, 255, 249, 230), 22);
        Art.Text(g, endless ? "ENDLESS MEADOW" : CurrentChapter.World, 147, 108, 13, artwork.Teal, true);
        Art.Text(g, endless ? $"Deal {level:N0}  ·  no finish line" : $"Level {level:N0} / {campaign.LevelCount:N0}", 147, 133, 23, artwork.Ink, true);
        Art.Text(g, "SCORE", 565, 108, 12, artwork.Teal, true); Art.Text(g, hand.Score.ToString("N0"), 565, 131, 25, artwork.Ink, true);
        Art.Text(g, "STREAK", 733, 108, 12, artwork.Teal, true); Art.Text(g, $"{hand.Streak}  ✦", 733, 131, 25, artwork.Ink, true);
        StoryProgressArt.Draw(g,new(906,103,220,62),hand.BestStreak,hand.Removed.Count(x=>!x));
        Art.Text(g, (hand.Deal.Bonus ? "\U0001f381 " : "") + hand.Deal.Formation + (hand.Deal.Difficulty is { Tier: > 0 } difficulty ? "  |  " + difficulty.Badge : ""), 150, 179, 11, artwork.Cream, true);
        var liveBombs = hand.Deal.Cards.Select((c, i) => new { c, i }).Where(x => x.c.Kind == CardKind.Bomb && !hand.Removed[x.i]).Select(x => (hand as IAdventureHand)?.CardStatus(x.i)).ToArray();
        if ((hand as IAdventureHand)?.Failed == true)
            Art.Text(g, "\U0001f4a3 \U0001f4a5     \u21b6 / \u21bb", 160, 508, 13, Color.LightGoldenrodYellow, true, 960, true);
        else if (liveBombs.Length > 0)
            Art.Text(g, string.Join("   ", liveBombs) + (hand.Deal.Cards.Any(c=>c.Kind==CardKind.Gate) && hand is ITemporaryHand phaseHand ? phaseHand.Effects.Turns%2==0 ? "   \u2600" : "   \u263e" : ""), 160, 508, 12, artwork.Cream, true, 960, true);
        Art.Box(g, new(113, 193, 1055, 327), Color.FromArgb(50, 12, 65, 47), 26);
        for (int i = 0; i < hand.Removed.Count; i++)
        {
            if (hand.Removed[i] || hand.IsExposed(i) || !profile.ReducedMotion && dealReveals.TryGetValue(i,out double reveal) && effects.Time<reveal) continue;
            var r = CardBounds(i);
            var slot=hand.Deal.Layout[i];
            if(slot.DrawX.HasValue){var state=g.Save();g.TranslateTransform(r.X+r.Width/2,r.Y+r.Height/2);g.RotateTransform(slot.Angle);artwork.DrawCard(g,new(-38,-53,73,99),hand.Deal.Cards[i],false,false,false);g.Restore(state);}
            else artwork.DrawCard(g, new(r.X + 1, r.Y + 1, r.Width - 3, r.Height - 7), hand.Deal.Cards[i], false, false, false);
        }
        Art.Text(g,"Clear all cards to win",143,530,13,artwork.Cream,true);
        PaintTemporaryEffects(g);
        Art.Box(g, new(390, 527, 540, 146), Color.FromArgb(175, 244, 240, 215), 22);
        Art.Box(g, new(403, 660, 130, 25), Color.FromArgb(200, artwork.Ink), 12);
        Art.Box(g, new(541, 660, 130, 25), Color.FromArgb(200, artwork.Ink), 12);
        Art.Text(g, $"{hand.Stock.Count} in stock", 403, 665, 14, artwork.Cream, true, 130, true);
        Art.Text(g, hand.Waste.Rank == 0 ? "Any card plays" : "One up or down", 541, 665, 14, artwork.Cream, true, 130, true);
        if (!hand.Moves.Any() && hand.Stock.Count == 0 && !hand.Won)
        {
            Art.Box(g, new(940, 622, 216, 50), Color.FromArgb(200, artwork.Cream), 12);
            Art.Text(g, "No rush. A helper or a fresh\ndeal can open a new path.", 950, 629, 13, artwork.Ink, false, 195, true);
        }
        if (selectedPowerup != null) Art.Text(g, "Choose an exposed card for your helper", 170, 557, 19, artwork.Cream, true, 220, true);
    }
    void PaintGems(Graphics g)
    {
        if (gems == null) return;
        Art.Box(g, new(57, 145, 328, 528), Color.FromArgb(231, 255, 248, 227), 25);
        Art.Text(g, "THE GEM GARDEN", 84, 172, 13, artwork.Teal, true);
        Art.Text(g, "A little sparkle.\nA few more coins.", 81, 203, 29, artwork.Ink, true);
        Art.Text(g, "Swap neighboring gems to match\n3 or more in a row or column.\nCascades earn extra coins.\n\nEvery coin goes straight to\nyour solitaire wallet.", 84, 298, 18, artwork.Teal);
        Art.Text(g, "Fresh Garden: 1-hour recharge.\nYour current round is saved.", 84, 453, 15, artwork.Teal);
        Art.Box(g, new(414, 171, 540, 503), Color.FromArgb(220, 26, 77, 62), 23);
        Art.Box(g, new(995, 192, 225, 300), Color.FromArgb(220, 255, 248, 227), 22);
        Art.Text(g, "MOVES LEFT", 1019, 222, 14, artwork.Teal, true); Art.Text(g, gems.Moves.ToString(), 1014, 246, 55, artwork.Ink, true);
        Art.Text(g, "COINS THIS ROUND", 1019, 334, 14, artwork.Teal, true); Art.Text(g, "+" + gems.Earned, 1014, 363, 41, Color.FromArgb(158, 109, 33), true);
        Art.Box(g, new(995, 510, 225, 140), Color.FromArgb(220, artwork.Cream), 20);
        Art.Text(g, gems.Moves == 0 ? "Beautifully done!\nYour coins are saved.\nFresh Garden: 1 hour." : "Tip: four and five gems\nmake bigger matches.\nLook for cascades!", 1008, 526, 19, artwork.Teal, true, 210, true);
    }
    void PaintOverlay(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(155, 12, 41, 33)); g.FillRectangle(shade, 0, 0, 1280, 800);
        Art.Box(g, new(215, 128, 850, 557), artwork.Cream, 28);
        if(overlay=="wheel")PaintWheel(g);
        else if(overlay=="levelPreview")
        {
            AdventureMapArt.Ribbon(g,new(315,142,650,75),$"Level {previewLevel:N0}","A LOVELY ADVENTURE");
            int earned=profile.Stars.GetValueOrDefault(previewLevel);
            for(int i=0;i<3;i++)FestiveArt.Star(g,490+i*150,310-(i==1?15:0),i==1?65:52,i<earned?artwork.Gold:Color.FromArgb(188,133,118));
            Art.Box(g,new(350,396,580,55),Color.FromArgb(240,212,176),21);Art.Text(g,"Clear all cards!",350,405,28,Color.FromArgb(144,57,67),true,580,true);
            Art.Text(g,StoryLayoutsV2.NameFor(previewLevel)+"  |  A lovely adventure.",330,644,18,artwork.Teal,true,620,true);
            var items=session.Powerups.All.Where(p=>session.HelperDiscovered(p.Id)).Take(3).ToArray();
            for(int i=0;i<items.Length;i++){Art.Box(g,new(440+i*140,475,120,50),Color.FromArgb(163,115,179),18);Art.Text(g,$"{items[i].Name} x{session.Inventory.Count(items[i].Id)}",440+i*140,490,12,Color.White,true,120,true);}
        }
        else if (overlay == "shop")
        {
            Art.Text(g, "A little help along the way", 260, 177, 35, artwork.Ink, true, 760, true);
            Art.Text(g, $"Your wallet: {profile.Coins:N0} coins  ·  All helpers are bought with earned coins", 260, 230, 17, artwork.Teal, false, 760, true);
            var helpers = VisibleHelpers().Skip(powerupPage * 3).Take(3).ToArray();
            for (int i = 0; i < helpers.Length; i++)
            {
                var helper = helpers[i];
                int x = 270 + i * 254; Art.Box(g, new(x - 7, 285, 244, 245), Color.FromArgb(231, 233, 209), 19);
                Art.Text(g, helper.Name, x, 313, 23, artwork.Ink, true, 230, true);
                Art.Text(g, helper.Description, x + 3, 361, 15, artwork.Teal, false, 224, true);
                Art.Text(g, $"In your bag: {session.Inventory.Count(helper.Id)}", x, 420, 15, artwork.Teal, true, 230, true);
            }
            Art.Text(g, "Earn more coins in Gem Garden.", 270, 547, 17, artwork.Teal, false, 740, true);
        }
        else if (overlay == "win")
        {
            PaintVictory(g);
        }
        else if (overlay == "mechanics") PaintCardGuide(g);
        else if (overlay == "help") PaintHowToPlay(g);
        else if (overlay == "journal") PaintJournal(g);
        else if (overlay == "share")
        {
            Art.Text(g, "Share this deal", 280, 180, 36, artwork.Ink, true, 720, true);
            Art.Text(g, session.ShareCode ?? "Start a new hand to share its deal.", 260, 310, 22, artwork.Teal, true, 760, true);
            Art.Text(g, "Same cards, same level. Open this code from the home screen.\nCampaign levels must be unlocked. Share between current Windows and browser editions.", 280, 410, 18, artwork.Teal, false, 720, true);
        }
        else if (overlay == "settings")
        {
            Art.Text(g, "Make yourself comfortable", 280, 190, 36, artwork.Ink, true, 720, true);
            Art.Text(g, "Everything saves automatically on this Windows account.", 280, 251, 18, artwork.Teal, false, 720, true);
            Art.Text(g, "Progress is saved on this device.", 280, 624, 15, artwork.Teal, false, 720, true);
        }
        else
        {
            Art.Text(g, overlay == "retry" ? "A fresh start?" : "Follow a new path?", 280, 226, 40, artwork.Ink, true, 720, true);
            Art.Text(g, overlay == "retry" ? "Replay this same deal from the beginning.\nYour current hand will be replaced.\nCoins already spent and helpers used stay spent." : "You have a hand in progress.\nStarting a new hand will replace it.\nYour coins and adventure progress are safe.", 280, 316, 22, artwork.Teal, false, 720, true);
        }
    }
    void KeysPressed(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F11) { WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; e.Handled = true; return; }
        if (e.KeyCode == Keys.Escape) { if (overlay.Length > 0) { overlay = ""; Build(); } else if (selectedPowerup != null) { selectedPowerup = null; Build(); } else Navigate("home"); e.Handled = true; return; }
        if (overlay.Length > 0 || screen != "play") return;
        if (e.KeyCode == Keys.D || (e.KeyCode == Keys.Space && ActiveControl is not GameButton)) Draw();
        else if (e.KeyCode == Keys.U) Undo(); else if (e.KeyCode == Keys.W) Wild(); else if (e.KeyCode == Keys.H) Hint(); else return;
        e.Handled = true; e.SuppressKeyPress = true;
    }
    void Smoke()
    {
        // IncludeAllContentForSelfExtract changes AppContext.BaseDirectory to
        // the runtime cache. Keep diagnostics next to the launched executable.
        string outputDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        try
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "screenshots"));
            void Shot(string name, float advance = .85f)
            {
                AdvancePresentation(advance); Refresh();
                effectsWindow!.Size = ClientSize; effectsWindow.ViewScale = scale; effectsWindow.ViewOffset = new(offsetX, offsetY);
                if (!effectsWindow.RenderNativeFrame()) throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error(), "Native alpha animation layer failed");
                using var image = new Bitmap(Width, Height); DrawToBitmap(image, new Rectangle(0, 0, Width, Height));
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var clientOrigin = PointToScreen(Point.Empty);
                    graphics.TranslateTransform(clientOrigin.X - Left + offsetX, clientOrigin.Y - Top + offsetY);
                    graphics.ScaleTransform(scale, scale); graphics.SetClip(new RectangleF(0, 0, 1280, 800)); effects.Draw(graphics, artwork);
                }
                image.Save(Path.Combine(outputDirectory, "screenshots", name + ".png"));
            }
            void Click(string name) { if(name.StartsWith("card")&&effects.Time<dealUntil)AdvancePresentation((float)(dealUntil-effects.Time+.01)); if(name=="draw" && (hand as ITemporaryHand)?.Effects.DrawCooldown>0) AdvancePresentation(2.01f); var b = Controls.Find(name, false).SingleOrDefault() as GameButton; if (b == null || !b.Enabled) throw new Exception("Missing or disabled UI button: " + name); var stableCards=Controls.OfType<GameButton>().Where(c=>c.Name.StartsWith("card")&&c.Name!=name).ToDictionary(c=>c.Name);b.PerformClick();if(overlay.Length==0 && screen=="play" && (name=="draw"||name.StartsWith("card")))foreach(var pair in stableCards)if(Controls.Find(pair.Key,false).FirstOrDefault() is Control current&&!ReferenceEquals(current,pair.Value))throw new Exception("Untouched card control was replaced"); }
            Navigate("home"); Shot("01-home"); Click("adventure"); Shot("02-adventure"); Click("map-wheel"); Shot("25-lucky-wheel"); Click("spinWheel"); Shot("26-wheel-result",4.3f); if(session.Wheel.Spins!=0||profile.WheelSpinsUsed!=1)throw new Exception("Wheel UI did not consume a welcome spin"); Click("closeOverlay"); Click("level1"); Shot("02-level-preview"); Click("playPreview"); Shot("03-solitaire",1.2f);
            var path = hand!.Deal.Solution.ToArray();
            Click("card" + path[0].Index);
            if (effects.Count == 0) throw new Exception("Card effects did not start");
            Shot("10-card-flight", .15f); Shot("11-card-sparkles", .12f);
            foreach (var step in path.Skip(1)) Click(step.Index < 0 ? "draw" : "card" + step.Index);
            if (hand?.Won != true || overlay != "win" || reward < 100) throw new Exception("UI win flow failed");
            Shot("04-victory"); Click("closeOverlay"); Click("wallet"); Shot("05-shop");
            int before = session.Inventory.Count("wild"); Click("buy-wild"); if (session.Inventory.Count("wild") != before + 1) throw new Exception("Shop failed");
            Click("earnGems"); int wallet = profile.Coins; var match = gems!.Hint()!.Value; Click("gem" + match.Item1); Click("gem" + match.Item2);
            if (profile.Coins <= wallet || gemAnimation == null) throw new Exception("Gem wallet or animation failed"); Shot("06-gems", .3f);
            AdvancePresentation(10);
            if (gemAnimation != null) throw new Exception("Gem animation did not complete");
            ClientSize = new(1000, 680); Build(); Shot("07-small-window");
            ClientSize = new(1280, 800); Click("home"); Click("how"); Shot("08-help"); Click("closeOverlay"); Click("settings"); Shot("09-settings");
            Click("motion"); AdvancePresentation(.04f);
            if (!effects.ReducedMotion || effects.Count != 0) throw new Exception("Reduced motion did not suppress effects");
            Click("motion"); Click("closeOverlay"); Click("adventure"); mapPage = campaign.Chapters.Count - 1; Build(); Shot("12-celebration-island");
            if (Controls.Find("level" + campaign.LevelCount, false).Length != 1) throw new Exception("Final chapter is missing");
            for (int n = 1; n < 20; n++) profile.Stars[n] = 1;
            profile.Unlocked = 20; NewHand(20, false);
            foreach (var step in hand!.Deal.Solution) Click(step.Index < 0 ? "draw" : "card" + step.Index);
            if (lastVictory?.ChapterBonus != 150 || !profile.ChapterGifts.Contains(1)) throw new Exception("Chapter celebration or gift failed");
            Shot("13-chapter-celebration", 1.2f);
            overlay = ""; profile.Unlocked = Math.Max(profile.Unlocked, 900); NewHand(900, false);
            if (!hand!.Deal.Cards.Any(c => c.Kind == CardKind.Bomb) || !hand.Deal.Cards.Any(c => c.Kind == CardKind.Ice)) throw new Exception("Special cards missing");
            Shot("14-special-cards"); Click("mechanics"); Shot("15-special-guide"); Click("mechanicsNext"); Shot("24-expanded-guide"); Click("gotit");
            foreach (var step in hand.Deal.Solution) Click(step.Index < 0 ? "draw" : "card" + step.Index);
            if (!hand.Won || ((IAdventureHand)hand).Failed) throw new Exception("Special-card UI witness failed");
            Shot("16-special-victory");
            for (int n = 2101; n <= 2104; n++)
            {
                overlay = ""; profile.Unlocked = n; NewHand(n, false);
                int rows = hand!.Deal.Layout.Max(s => s.Row)+1;
                if (!hand.Deal.Layout.All(s=>s.DrawX.HasValue && s.BlockedBy!=null)) throw new Exception("Story formation geometry missing");
                foreach (var button in Controls.OfType<GameButton>().Where(b => b.Name.StartsWith("card")))
                    if (button.Bottom > Rect(0, 0, 0, 521).Bottom) throw new Exception("Deep cards overlap stock");
                Shot("18-story-"+n);
                foreach (var step in hand.Deal.Solution) Click(step.Index < 0 ? "draw" : "card"+step.Index);
                if (!hand.Won) throw new Exception("Deep board UI path failed");
            }
            overlay = ""; profile.Unlocked = 3000; NewHand(3000, false);
            Shot("20-advanced-tier");
            foreach (var step in hand!.Deal.Solution) Click(step.Index < 0 ? "draw" : "card"+step.Index);
            if (!hand.Won) throw new Exception("Advanced progression UI path failed");
            overlay = ""; profile.Unlocked = 730; NewHand(730, false);
            session.Inventory.Add("lucky", 1); powerupPage=1; Build(); Click("power-lucky");
            Shot("19-temporary-effects", .2f);
            if (((ITemporaryHand)hand!).Effects.LuckySeconds <= 0) throw new Exception("Temporary boost did not start");
            AdvancePresentation(11);
            if (((ITemporaryHand)hand!).Effects.LuckySeconds != 0) throw new Exception("Temporary boost did not expire");
            powerupPage=0; Build();
            overlay=""; profile.Unlocked=2400; NewHand(2400,false);
            if(!hand!.Deal.Cards.Any(c=>c.Kind==CardKind.Gate) || !hand.Deal.Cards.Any(c=>c.Kind==CardKind.ColorSeal)) throw new Exception("Expansion cards missing");
            session.Inventory.Add("peek",1); powerupPage=Array.FindIndex(VisibleHelpers(),h=>h.Id=="peek")/3; Build(); Click("power-peek"); Shot("21-stock-preview",.2f);
            if(Controls.Find("mechanics",false).Single().Visible) throw new Exception("Preview controls not hidden");
            AdvancePresentation(7);
            if(((ITemporaryHand)hand).Effects.PeekSeconds>0 || !Controls.Find("mechanics",false).Single().Visible) throw new Exception("Stock preview expiry failed");
            Click("shareDeal"); Shot("22-share-code"); string code=session.ShareCode ?? throw new Exception("Shared code missing"); Click("closeOverlay");
            var sharedCards=hand.Deal.Cards.ToArray();
            if(!session.StartSharedDeal(code) || !hand!.Deal.Cards.SequenceEqual(sharedCards)) throw new Exception("Shared deal reproduction failed");
            Build(); Click("favorite"); if(!profile.Favorites.Contains(2400)) throw new Exception("Favorite failed");
            Click("home"); session.Picnic.Win();session.Picnic.Win();session.Picnic.Clear(24);session.Picnic.Gems(80);
            Click("journal"); Shot("23-journal"); int picnicWallet=profile.Coins; Click("picnic-wins");
            if(profile.Coins!=picnicWallet+40) throw new Exception("Picnic claim UI failed");
            Click("favorite-level2400"); if(screen!="map") throw new Exception("Favorite navigation failed");
            SaveFormationAtlas(Path.Combine(outputDirectory, "screenshots", "17-formations.png"));
            File.WriteAllText(Path.Combine(outputDirectory, "smoke-result.txt"), "PASS: journal, picnic, favorites, share codes, stock preview, gates, color seals, special cards, ice, keys, locks, bombs, bonus stock, new formations, guide,  home, 24,000 chapters, 480,000 levels, 5-8 rows, temporary effects, solitaire win, rewards, shop, animated gem cascades, card flight, effects, reduced motion, chapter gift, resize.");
        }
        catch (Exception ex) { File.WriteAllText(Path.Combine(outputDirectory, "smoke-result.txt"), ex.ToString()); Environment.ExitCode = 1; }
        finally { Close(); }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { iconTips.Dispose(); toastTimer.Dispose(); animation.Dispose(); effectsWindow?.Dispose(); backdrop?.Dispose(); } base.Dispose(disposing);
    }
}
