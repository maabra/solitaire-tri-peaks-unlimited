using System.Drawing.Drawing2D;

namespace MeadowPeaks;
public sealed class GameWindow : Form
{
    readonly GameSession session;
    readonly IGameArtwork artwork;
    readonly ISoundPlayer sound;
    Progress profile => session.Profile;
    readonly bool smoke;
    string screen = "home", overlay = "", message = "Welcome to the meadow. Take your time.";
    ISolitaireHand? hand => session.Hand;
    ICoinGame? gems => session.CoinGame;
    int level => session.Level;
    bool endless => session.Endless;
    int mapPage, selectedGem = -1, gemHint = -1, cardHint = -1, powerupPage;
    string? selectedPowerup;
    int reward, stars;
    float scale = 1, offsetX, offsetY;
    Action? confirmAction;
    readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 5000 };
    readonly System.Windows.Forms.Timer animation = new() { Interval = 40 };
    int celebrate;
    bool rebuilding;
    static readonly string[] Regions = { "Willow meadow", "Moonlit lake", "Amber orchard", "Snowdrop valley", "Fernwood trail", "Cloudberry hills", "Lavender coast", "Golden grove", "Starlight garden", "Homeward peaks" };
    public GameWindow(GameSession session, IGameArtwork artwork, ISoundPlayer sound, bool smoke = false)
    {
        this.session = session;
        this.artwork = artwork;
        this.sound = sound;
        this.smoke = smoke;
        Text = "Meadow Peaks • Solitaire Unlimited";
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        ClientSize = new(1280, 800); MinimumSize = new(1000, 680); StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true; KeyPreview = true; BackColor = artwork.Teal;
        if (session.LoadWarning.Length > 0) message = session.LoadWarning;
        mapPage = (profile.Unlocked - 1) / 20;
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); message = ""; Invalidate(); };
        animation.Tick += (_, _) => { if (celebrate > 0) { celebrate--; Invalidate(); } else animation.Stop(); };
        Resize += (_, _) => Build();
        FormClosing += (_, _) => Save();
        Shown += (_, _) => { Build(); if (smoke) BeginInvoke(new Action(Smoke)); };
        KeyDown += KeysPressed;
        AccessibleName = "Meadow Peaks Solitaire";
        Build();
    }
    void Save()
    {
        try { session.Save(); }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { message = "Progress could not be saved. Check access to your Local AppData folder."; Invalidate(); }
    }
    void Tell(string text)
    {
        message = text; toastTimer.Stop(); toastTimer.Start(); Invalidate();
    }
    void Chime()
    {
        if (profile.Sound && !smoke) sound.Play();
    }
    Rectangle Rect(float x, float y, float w, float h) => new((int)(offsetX + x * scale), (int)(offsetY + y * scale), Math.Max(1, (int)(w * scale)), Math.Max(1, (int)(h * scale)));
    GameButton Button(string name, string text, float x, float y, float w, float h, Action action, Color? fill = null, bool enabled = true)
    {
        var b = new GameButton(artwork) { Name = name, Text = text, AccessibleName = text.Replace("\n", " "), Bounds = Rect(x, y, w, h), Fill = fill ?? artwork.Cream, Enabled = enabled, TabIndex = Controls.Count };
        b.Click += (_, _) => action(); Controls.Add(b); return b;
    }
    void Build()
    {
        if (rebuilding || IsDisposed) return;
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
            Button("wallet", $"●  {profile.Coins:N0}   +", 1025, 26, 215, 46, () => ShowOverlay("shop"), artwork.Gold);
            Button("settings", "⚙", screen == "home" ? 38 : 168, 26, 48, 46, () => ShowOverlay("settings"));
            if (screen == "home") BuildHome();
            if (screen == "map") BuildMap();
            if (screen == "play") BuildHand();
            if (screen == "gems") BuildGems();
        }
        if (focused != null && Controls.Find(focused, false).FirstOrDefault() is { Enabled: true } found) found.Focus();
        ResumeLayout(); rebuilding = false; Invalidate();
    }
    void Navigate(string target) { overlay = ""; selectedPowerup = null; screen = target; Save(); Build(); }
    void ShowOverlay(string name) { overlay = name; selectedPowerup = null; Build(); }
    void BuildHome()
    {
        Button("adventure", "Continue adventure   →", 88, 402, 345, 65, () => { mapPage = (profile.Unlocked - 1) / 20; Navigate("map"); }, artwork.Gold);
        Button("endless", "∞   Endless meadow", 88, 482, 345, 58, StartEndless);
        Button("gems", "◆   Gem garden · earn coins", 88, 554, 345, 58, OpenGems);
        if (hand != null && !hand.Won) Button("resume", "Resume saved hand", 88, 626, 345, 48, () => Navigate("play"), Color.FromArgb(195, 225, 196));
        Button("how", "How to play", 88, 695, 160, 42, () => ShowOverlay("help"));
        Button("daily", session.DailyClaimed ? "Gift collected ✓" : $"Daily gift  +{session.Economy.DailyGift}", 265, 695, 168, 42, ClaimDaily, artwork.Gold, !session.DailyClaimed);
        if (session.GardenGifts > 0) Button("gardenGift", $"Collect garden gift · +{session.Economy.GardenGift}", 810, 651, 325, 50, ClaimGarden, artwork.Gold);
    }
    void ClaimDaily()
    {
        if (session.DailyClaimed) return;
        int coins = session.ClaimDaily();
        Save(); Tell($"A little gift for a lovely day. +{coins} coins!"); Build();
    }
    void ClaimGarden()
    {
        if (session.GardenGifts == 0) return;
        int coins = session.ClaimGarden();
        Save(); Tell($"Your garden thanks you. +{coins} coins!"); Build();
    }
    void BuildMap()
    {
        for (int i = 0; i < 20; i++)
        {
            int n = mapPage * 20 + i + 1;
            var p = MapPoint(i);
            bool unlocked = n <= profile.Unlocked;
            Button("level" + n, unlocked ? n.ToString() : "•", p.X - 32, p.Y - 29, 64, 64, () => StartAdventure(n), n == profile.Unlocked ? artwork.Gold : profile.Stars.ContainsKey(n) ? Color.FromArgb(207, 226, 177) : artwork.Cream, unlocked);
        }
        Button("prev", "‹  Previous", 70, 685, 160, 50, () => { mapPage--; Build(); }, enabled: mapPage > 0);
        Button("next", "Next  ›", 1050, 685, 160, 50, () => { mapPage++; Build(); }, enabled: mapPage < 99);
        Button("current", "My next level", 715, 685, 185, 50, () => { mapPage = (profile.Unlocked - 1) / 20; Build(); });
        Button("jump", "Go to level…", 910, 685, 130, 50, JumpLevel);
    }
    static PointF MapPoint(int i)
    {
        int row = i / 5, col = row % 2 == 0 ? i % 5 : 4 - i % 5;
        return new(270 + col * 187, 243 + row * 116 + (col % 2 == 0 ? 0 : 17));
    }
    void JumpLevel()
    {
        using var dialog = new Form { Text = "Find a level", ClientSize = new(330, 155), FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false };
        var label = new Label { Text = $"Choose a level (1–2000). Unlocked: {profile.Unlocked}", Left = 18, Top = 18, Width = 300 };
        var number = new NumericUpDown { Minimum = 1, Maximum = 2000, Value = profile.Unlocked, Left = 20, Top = 50, Width = 285 };
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
        if (n > profile.Unlocked || n < 1 || n > 2000) return;
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
        cardHint = -1; selectedPowerup = null; overlay = ""; screen = "play"; Save(); Build();
        Tell(n == 1 && !infinite ? "Play a card one higher or lower than the large card below. Suits do not matter." : "A fresh path through the peaks. Enjoy the journey.");
    }
    void BuildHand()
    {
        if (hand == null) { screen = "home"; BuildHome(); return; }
        for (int i = 0; i < hand.Removed.Count; i++)
        {
            if (hand.Removed[i]) continue;
            int index = i; var slot = hand.Deal.Layout[i]; bool face = hand.IsExposed(i);
            if (!face) continue;
            var b = Button("card" + i, "", 140 + slot.X * 87, 208 + slot.Row * 59, 79, 111, () => PlayCard(index));
            b.CardValue = hand.Deal.Cards[i]; b.Face = face;
            b.Hint = face && ((profile.Hints && hand.CanPlay(i)) || cardHint == i || selectedPowerup != null);
            b.AccessibleName = face ? $"{Art.Rank(b.CardValue.Rank)} of {new[] { "spades", "hearts", "diamonds", "clubs" }[b.CardValue.Suit]}{(hand.CanPlay(i) ? ", playable" : "")}" : "Covered card";
            b.TabStop = face; b.Enabled = face;
            b.BringToFront();
        }
        var draw = Button("draw", "", 426, 540, 86, 119, Draw);
        draw.CardValue = new(1, 0); draw.Face = false; draw.AccessibleName = $"Draw a card. {hand.Stock.Count} left"; draw.Enabled = hand.Stock.Count > 0 && !hand.Won;
        var waste = Button("waste", "", 561, 540, 86, 119, () => Tell("Play one rank higher or lower. Aces connect Kings and Twos."));
        waste.CardValue = hand.Waste; waste.TabStop = false; waste.AccessibleName = "Waste pile " + Art.Rank(hand.Waste.Rank);
        Button("hint", "Hint · free", 742, 551, 156, 47, Hint);
        Button("retry", "Replay deal", 742, 609, 156, 43, () => { confirmAction = () => { session.Replay(); selectedPowerup = null; overlay = ""; Save(); Build(); }; ShowOverlay("retry"); });
        Button("undo", hand.FreeUndos > 0 ? $"↶  Undo · {hand.FreeUndos} free" : $"↶  Undo · {session.Economy.UndoCost} coins", 158, 698, 210, 57, Undo, enabled: hand.CanUndo);
        var helpers = session.Powerups.All.Skip(powerupPage * 3).Take(3).ToArray();
        for (int i = 0; i < helpers.Length; i++)
        {
            var helper = helpers[i];
            Button("power-" + helper.Id, $"{helper.Name}  ×{session.Inventory.Count(helper.Id)}", 383 + i * 225, 698, 210, 57, () => ActivatePowerup(helper.Id), enabled: helper.CanUse(hand));
        }
        BuildPowerupPages(698);
        Button("help", "?", 1163, 108, 49, 46, () => ShowOverlay("help"));
        if (hand.Won) Button("finishedNext", "Next hand  →", 920, 566, 230, 53, () => NewHand(endless ? profile.EndlessDeal : Math.Min(level + 1, 2000), endless || level == 2000), artwork.Gold);
        if (!hand.Moves.Any() && hand.Stock.Count == 0 && !hand.Won) Button("earn", "Earn free coins  ◆", 920, 566, 230, 53, OpenGems, artwork.Gold);
    }
    void PlayCard(int i)
    {
        if (hand == null) return;
        if (selectedPowerup != null)
        {
            string helperName = session.Powerups.Find(selectedPowerup)?.Name ?? "Helper";
            if (session.UsePowerup(selectedPowerup, i) == CommandResult.Applied)
            {
                selectedPowerup = null; Tell(helperName + " used."); Changed();
            }
            return;
        }
        if (!session.PlayCard(i)) { Tell($"That card needs a {Art.Rank(hand.Deal.Cards[i].Rank == 1 ? 13 : hand.Deal.Cards[i].Rank - 1)} or {Art.Rank(hand.Deal.Cards[i].Rank == 13 ? 1 : hand.Deal.Cards[i].Rank + 1)} on the pile."); return; }
        cardHint = -1; Chime(); Changed();
    }
    void Changed()
    {
        var result = session.CompleteHand();
        if (result != null)
        {
            stars = result.Stars;
            reward = result.Coins;
            overlay = "win"; celebrate = profile.ReducedMotion ? 0 : 90;
            if (celebrate > 0) animation.Start();
        }
        Save(); Build();
    }
    void Draw() { if (session.Draw()) { selectedPowerup = null; cardHint = -1; Changed(); } else Tell("The stock is empty. Try a Wild, five more cards, or replay this deal for free."); }
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
        if (hand == null || helper == null || !helper.CanUse(hand)) return;
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
            selectedPowerup = null; Tell(helper.Name + " used."); Changed();
        }
    }
    void BuildPowerupPages(float y)
    {
        if (session.Powerups.All.Count <= 3) return;
        Button("prevPowerups", "‹", overlay == "shop" ? 233 : 105, y, 40, 48, () => { powerupPage--; Build(); }, enabled: powerupPage > 0);
        Button("nextPowerups", "›", overlay == "shop" ? 1008 : 1060, y, 40, 48, () => { powerupPage++; Build(); }, enabled: (powerupPage + 1) * 3 < session.Powerups.All.Count);
    }
    void Hint()
    {
        if (hand == null) return;
        cardHint = hand.Moves.FirstOrDefault(-1);
        if (cardHint >= 0) { Tell("Follow the golden outline. That card is one higher or lower."); Build(); Controls.Find("card" + cardHint, false).FirstOrDefault()?.Focus(); }
        else Tell(hand.Stock.Count > 0 ? "No cards fit right now. Draw from the stock (D)." : "No moves left. Use a helper, earn coins in Gem Garden, or replay for free.");
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
            b.Gem = gems.Cells[i]; b.Hint = i == selectedGem || i == gemHint;
            b.AccessibleName = $"{new[] { "Ruby diamond", "Blue pearl", "Violet triangle", "Gold star", "Jade square", "Amber hexagon" }[b.Gem]} row {i / 8 + 1} column {i % 8 + 1}";
            b.Enabled = gems.Moves > 0;
        }
        Button("gemhint", "Show a match · free", 84, 524, 270, 52, () => { var hint = gems.Hint(); if (hint.HasValue) { selectedGem = hint.Value.Item1; gemHint = hint.Value.Item2; Build(); Tell("Swap the two golden gems."); } }, enabled: gems.Moves > 0);
        Button("gemnew", gems.Moves == 0 ? "Play again · free" : "New garden · free", 84, 591, 270, 52, () => { session.NewCoinGame(); selectedGem = -1; gemHint = -1; Build(); }, artwork.Gold);
        Button("backhand", hand != null && !hand.Won ? "Back to my hand  →" : "Back to adventure  →", 432, 702, 508, 53, () => Navigate(hand != null && !hand.Won ? "play" : "map"));
    }
    void SelectGem(int i)
    {
        if (gems == null || gems.Moves == 0) return;
        if (selectedGem == i) { selectedGem = -1; Build(); return; }
        if (selectedGem < 0) { selectedGem = i; gemHint = -1; Build(); return; }
        int a = selectedGem;
        if (Math.Abs(a / 8 - i / 8) + Math.Abs(a % 8 - i % 8) != 1) { selectedGem = i; Build(); return; }
        if (session.PlayGems(a, i))
        {
            Save(); Tell($"Lovely match! +{gems.LastGain} coins{(gems.Moves == 0 ? ". Play again for free whenever you like!" : "")}"); Chime();
        }
        else Tell("Try a swap that makes a row or column of three. Invalid swaps cost no moves.");
        selectedGem = -1; gemHint = -1; Build();
    }
    void Buy(string id)
    {
        if (session.BuyPowerup(id) != CommandResult.Applied) { Tell("Need more coins? Gem Garden is always free."); return; }
        Save(); Tell("Helper added to your collection."); Build();
    }
    void BuildOverlay()
    {
        Button("closeOverlay", "×", 1004, 148, 46, 44, () => { overlay = ""; Build(); });
        if (overlay == "shop")
        {
            var helpers = session.Powerups.All.Skip(powerupPage * 3).Take(3).ToArray();
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
            Button("nextHand", !endless && level == 2000 ? "Enter the endless meadow  →" : "Next adventure  →", 439, 512, 402, 61, () => NewHand(endless ? profile.EndlessDeal : Math.Min(level + 1, 2000), endless || level == 2000), artwork.Gold);
            Button("winHome", "Back to the meadow", 439, 590, 402, 48, () => Navigate("home"));
        }
        else if (overlay == "help") Button("gotit", "Let’s play", 450, 602, 380, 56, () => { overlay = ""; Build(); }, artwork.Gold);
        else if (overlay == "settings")
        {
            Button("sound", "Sound  " + (profile.Sound ? "ON" : "OFF"), 375, 315, 530, 55, () => { profile.Sound = !profile.Sound; Save(); Build(); });
            Button("hints", "Playable card glow  " + (profile.Hints ? "ON" : "OFF"), 375, 387, 530, 55, () => { profile.Hints = !profile.Hints; Save(); Build(); });
            Button("motion", "Reduced motion  " + (profile.ReducedMotion ? "ON" : "OFF"), 375, 459, 530, 55, () => { profile.ReducedMotion = !profile.ReducedMotion; Save(); Build(); });
            Button("fullscreen", WindowState == FormWindowState.Maximized ? "Restore window" : "Maximize window", 375, 531, 530, 55, () => { WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; Build(); });
        }
        else
        {
            Button("confirm", overlay == "retry" ? "Replay for free" : "Start a new hand", 419, 460, 442, 60, () => { overlay = ""; var a = confirmAction; confirmAction = null; a?.Invoke(); }, artwork.Gold);
            Button("cancel", "Keep my current hand", 419, 540, 442, 49, () => { overlay = ""; confirmAction = null; Build(); });
        }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(offsetX, offsetY); g.ScaleTransform(scale, scale);
        artwork.DrawLandscape(g, screen == "map" ? mapPage / 5 : Math.Max(0, (level - 1) / 100));
        if (screen != "home") Art.Text(g, "MEADOW PEAKS", 445, 36, 23, artwork.Ink, true, 390, true);
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
        if (celebrate > 0 && overlay == "win")
        {
            for (int i = 0; i < 45; i++) { float x = (i * 173) % 1240 + 20, y = ((90 - celebrate) * 5 + i * 37) % 780; using var b = new SolidBrush(i % 2 == 0 ? artwork.Gold : Color.FromArgb(140, 202, 154)); g.FillEllipse(b, x, y, 6 + i % 4, 10); }
        }
    }
    void PaintHome(Graphics g)
    {
        Art.Box(g, new(58, 116, 406, 635), Color.FromArgb(225, 255, 249, 230), 29);
        Art.Text(g, "A LITTLE ESCAPE, EVERY DAY", 88, 146, 13, artwork.Teal, true);
        Art.Text(g, "Meadow\nPeaks", 82, 176, 72, artwork.Ink, true);
        Art.Text(g, "Solitaire, at your own pace.", 88, 354, 22, artwork.Teal);
        Art.Box(g, new(742, 155, 411, 106), Color.FromArgb(213, 255, 249, 230), 22);
        Art.Text(g, "THE NEXT CHAPTER", 772, 175, 13, artwork.Teal, true);
        Art.Text(g, $"{Regions[(profile.Unlocked - 1) / 200]}", 772, 199, 28, artwork.Ink, true);
        Art.Text(g, $"Level {profile.Unlocked:N0} of 2,000  ·  {profile.Stars.Values.Sum():N0} stars collected", 772, 237, 15, artwork.Teal);
        using var path = new Pen(Color.FromArgb(191, 180, 142), 22) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawCurve(path, new[] { new Point(666, 644), new Point(800, 543), new Point(743, 425), new Point(929, 353), new Point(1047, 307) }, .8f);
        int[] xx = { 695, 792, 752, 899, 1044 }, yy = { 615, 530, 424, 360, 310 };
        for (int i = 0; i < 5; i++)
        {
            Art.Box(g, new(xx[i] - 28, yy[i] - 28, 56, 56), i == 4 ? artwork.Gold : artwork.Cream, 28);
            Art.Text(g, i == 4 ? "★" : "✓", xx[i] - 28, yy[i] - 20, 28, artwork.Teal, true, 56, true);
        }
        artwork.DrawCard(g, new(521, 284, 97, 140), new(12, 1), true, false, false);
        artwork.DrawCard(g, new(555, 321, 97, 140), new(13, 0), true, true, false);
        Art.Box(g, new(769, 584, 405, 136), Color.FromArgb(222, 245, 242, 215), 22);
        Art.Text(g, "YOUR GROWING GARDEN", 795, 604, 14, artwork.Teal, true);
        Art.Text(g, $"{profile.Garden:N0} hands cleared  ·  {profile.EndlessWins:N0} endless wins", 795, 632, 19, artwork.Ink, true);
        if (session.GardenGifts <= 0) Art.Text(g, $"{session.Economy.GardenInterval - profile.Garden % session.Economy.GardenInterval} more clears to your next {session.Economy.GardenGift}-coin gift", 795, 668, 16, artwork.Teal);
    }
    void PaintMap(Graphics g)
    {
        Art.Text(g, Regions[mapPage / 10], 290, 105, 38, artwork.Ink, true, 700, true);
        Art.Text(g, $"Chapter {mapPage + 1} / 100   ·   Levels {mapPage * 20 + 1}–{mapPage * 20 + 20}   ·   Free to play, always", 290, 158, 17, artwork.Teal, false, 700, true);
        Art.Box(g, new(190, 207, 900, 443), Color.FromArgb(135, 247, 244, 214), 28);
        using var path = new Pen(Color.FromArgb(193, 177, 133), 12) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        g.DrawLines(path, Enumerable.Range(0, 20).Select(MapPoint).ToArray());
        for (int i = 0; i < 20; i++)
        {
            var p = MapPoint(i); int n = mapPage * 20 + i + 1; int earned = profile.Stars.GetValueOrDefault(n);
            Art.Text(g, earned > 0 ? new string('★', earned) : n == profile.Unlocked ? "YOU ARE HERE" : "", p.X - 69, p.Y + 38, earned > 0 ? 17 : 10, earned > 0 ? Color.FromArgb(145, 96, 25) : artwork.Teal, true, 138, true);
        }
        Art.Text(g, $"{profile.Stars.Count:N0} / 2,000 cleared", 260, 701, 18, artwork.Cream, true);
    }
    void PaintHand(Graphics g)
    {
        if (hand == null) return;
        Art.Box(g, new(117, 93, 1047, 82), Color.FromArgb(220, 255, 249, 230), 22);
        Art.Text(g, endless ? "ENDLESS MEADOW" : Regions[(level - 1) / 200], 147, 108, 13, artwork.Teal, true);
        Art.Text(g, endless ? $"Deal {level:N0}  ·  no finish line" : $"Level {level:N0} / 2,000", 147, 133, 23, artwork.Ink, true);
        Art.Text(g, "SCORE", 565, 108, 12, artwork.Teal, true); Art.Text(g, hand.Score.ToString("N0"), 565, 131, 25, artwork.Ink, true);
        Art.Text(g, "STREAK", 733, 108, 12, artwork.Teal, true); Art.Text(g, $"{hand.Streak}  ✦", 733, 131, 25, artwork.Ink, true);
        Art.Text(g, "CARDS LEFT", 914, 108, 12, artwork.Teal, true); Art.Text(g, hand.Removed.Count(x => !x).ToString(), 914, 131, 25, artwork.Ink, true);
        Art.Box(g, new(113, 193, 1055, 327), Color.FromArgb(50, 12, 65, 47), 26);
        for (int i = 0; i < hand.Removed.Count; i++)
        {
            if (hand.Removed[i] || hand.IsExposed(i)) continue;
            var slot = hand.Deal.Layout[i];
            artwork.DrawCard(g, new(141 + slot.X * 87, 209 + slot.Row * 59, 76, 104), hand.Deal.Cards[i], false, false, false);
        }
        Art.Box(g, new(390, 527, 540, 146), Color.FromArgb(175, 244, 240, 215), 22);
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
        Art.Text(g, "No entry fee. No timer. Play again\nas often as you like.", 84, 453, 15, artwork.Teal);
        Art.Box(g, new(414, 171, 540, 503), Color.FromArgb(220, 26, 77, 62), 23);
        Art.Box(g, new(995, 192, 225, 300), Color.FromArgb(220, 255, 248, 227), 22);
        Art.Text(g, "MOVES LEFT", 1019, 222, 14, artwork.Teal, true); Art.Text(g, gems.Moves.ToString(), 1014, 246, 55, artwork.Ink, true);
        Art.Text(g, "COINS THIS ROUND", 1019, 334, 14, artwork.Teal, true); Art.Text(g, "+" + gems.Earned, 1014, 363, 41, Color.FromArgb(158, 109, 33), true);
        Art.Text(g, gems.Moves == 0 ? "Beautifully done!\nYour coins are saved.\nPlay another free round." : "Tip: four and five gems\nmake bigger matches.\nLook for cascades!", 1008, 526, 19, artwork.Cream, true, 210, true);
    }
    void PaintOverlay(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(155, 12, 41, 33)); g.FillRectangle(shade, 0, 0, 1280, 800);
        Art.Box(g, new(215, 128, 850, 557), artwork.Cream, 28);
        if (overlay == "shop")
        {
            Art.Text(g, "A little help along the way", 260, 177, 35, artwork.Ink, true, 760, true);
            Art.Text(g, $"Your wallet: {profile.Coins:N0} coins  ·  All helpers are bought with earned coins", 260, 230, 17, artwork.Teal, false, 760, true);
            var helpers = session.Powerups.All.Skip(powerupPage * 3).Take(3).ToArray();
            for (int i = 0; i < helpers.Length; i++)
            {
                var helper = helpers[i];
                int x = 270 + i * 254; Art.Box(g, new(x - 7, 285, 244, 245), Color.FromArgb(231, 233, 209), 19);
                Art.Text(g, helper.Name, x, 313, 23, artwork.Ink, true, 230, true);
                Art.Text(g, helper.Description, x + 3, 361, 15, artwork.Teal, false, 224, true);
                Art.Text(g, $"In your bag: {session.Inventory.Count(helper.Id)}", x, 420, 15, artwork.Teal, true, 230, true);
            }
            Art.Text(g, "Running low? There’s always a free way to earn more.", 270, 547, 17, artwork.Teal, false, 740, true);
        }
        else if (overlay == "win")
        {
            Art.Text(g, "A lovely little victory", 280, 187, 42, artwork.Ink, true, 720, true);
            Art.Text(g, new string('★', stars), 280, 257, 62, Color.FromArgb(214, 155, 43), true, 720, true);
            Art.Text(g, $"+{reward} coins", 280, 355, 35, artwork.Teal, true, 720, true);
            Art.Text(g, $"Score {hand?.Score:N0}  ·  Best streak {hand?.BestStreak}\nYour garden has grown. Progress saved.", 280, 415, 20, artwork.Teal, false, 720, true);
        }
        else if (overlay == "help")
        {
            Art.Text(g, "One up. One down. All yours.", 280, 175, 34, artwork.Ink, true, 720, true);
            Art.Text(g, "1. Play any exposed card one higher or lower than the waste pile.\n     Suits do not matter. Aces connect to both Kings and Twos.\n\n2. Clear the cards covering a card to turn it over. Clear the whole\n     board to win. Draw from the stock when you need a new rank.\n\n3. Long streaks earn stars. Wins pay coins and grow your garden.\n     Spend coins on helpers, or earn more in the free Gem Garden.\n\nKeys: Tab + Enter to play · D draw · U undo · W wild\nH hint · Esc close / home · F11 maximize. No timers or entry fees.", 292, 247, 19, artwork.Teal, false, 711);
            Art.Text(g, "3 free undos per hand. Helpers clear undo history. Replay is free.", 292, 563, 16, artwork.Teal);
        }
        else if (overlay == "settings")
        {
            Art.Text(g, "Make yourself comfortable", 280, 190, 36, artwork.Ink, true, 720, true);
            Art.Text(g, "Everything saves automatically on this Windows account.", 280, 251, 18, artwork.Teal, false, 720, true);
            Art.Text(g, "Offline · No accounts · No purchases · Original artwork", 280, 624, 15, artwork.Teal, false, 720, true);
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
            void Shot(string name) { Refresh(); using var image = new Bitmap(Width, Height); DrawToBitmap(image, new Rectangle(0, 0, Width, Height)); image.Save(Path.Combine(outputDirectory, "screenshots", name + ".png")); }
            void Click(string name) { var b = Controls.Find(name, false).SingleOrDefault() as GameButton; if (b == null || !b.Enabled) throw new Exception("Missing or disabled UI button: " + name); b.PerformClick(); }
            Shot("01-home"); Click("adventure"); Shot("02-adventure"); Click("level1"); Shot("03-solitaire");
            var path = hand!.Deal.Solution.ToArray();
            foreach (var step in path) Click(step.Index < 0 ? "draw" : "card" + step.Index);
            if (hand?.Won != true || overlay != "win" || reward < 100) throw new Exception("UI win flow failed");
            Shot("04-victory"); Click("closeOverlay"); Click("wallet"); Shot("05-shop");
            int before = session.Inventory.Count("wild"); Click("buy-wild"); if (session.Inventory.Count("wild") != before + 1) throw new Exception("Shop failed");
            Click("earnGems"); int wallet = profile.Coins; var match = gems!.Hint()!.Value; Click("gem" + match.Item1); Click("gem" + match.Item2);
            if (profile.Coins <= wallet) throw new Exception("Gem wallet failed"); Shot("06-gems");
            ClientSize = new(1000, 680); Build(); Shot("07-small-window");
            ClientSize = new(1280, 800); Click("home"); Click("how"); Shot("08-help"); Click("closeOverlay"); Click("settings"); Shot("09-settings");
            File.WriteAllText(Path.Combine(outputDirectory, "smoke-result.txt"), "PASS: home, map, full solitaire win, reward, shop, gem earnings, resize.");
        }
        catch (Exception ex) { File.WriteAllText(Path.Combine(outputDirectory, "smoke-result.txt"), ex.ToString()); Environment.ExitCode = 1; }
        finally { Close(); }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { toastTimer.Dispose(); animation.Dispose(); } base.Dispose(disposing);
    }
}
