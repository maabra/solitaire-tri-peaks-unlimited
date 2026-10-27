using System.Diagnostics;

namespace MeadowPeaks;

public sealed partial class GameWindow
{
    readonly EffectScene effects;
    readonly Stopwatch presentationClock = Stopwatch.StartNew();
    EffectsOverlay? effectsWindow;
    GemAnimationPlayer? gemAnimation;
    double lastFrame;
    double victoryStarted;
    WinReward? lastVictory;
    Bitmap? backdrop;
    bool backdropDirty = true;
    ICampaignCatalog campaign => session.Campaign;
    Chapter CurrentChapter => campaign.ForLevel(screen == "map" ? mapPage * campaign.LevelsPerChapter + 1 : screen == "home" ? profile.Unlocked : endless ? (level - 1) % campaign.LevelCount + 1 : level);

    // Advance presentation from elapsed time rather than tying game rules to repaint frequency.
    void AnimationTick(object? sender, EventArgs args)
    {
        double now = presentationClock.Elapsed.TotalSeconds;
        if(screen=="gems" && Controls.Find("gemnew",false).FirstOrDefault() is GameButton gardenButton){gardenButton.Text=GardenRefreshCaption();gardenButton.Enabled=gemAnimation==null && session.GardenRefreshRemaining<=0;}
        AdvanceTemporaryEffects(Math.Max(0, now - lastFrame));
        float elapsed = (float)Math.Clamp(now - lastFrame, 0, .08); lastFrame = now;
        AdvancePresentation(elapsed);
        if (!smoke && effectsWindow != null)
        {
            effectsWindow.ViewScale = scale; effectsWindow.ViewOffset = new(offsetX, offsetY);
            effectsWindow.Follow(this, true);
        }
        if (overlay == "win" && effects.Time - victoryStarted < 1.8 && !profile.ReducedMotion) { backdropDirty = true; Invalidate(Rect(300, 250, 680, 220)); }
    }
    // Advance current visual effects and invalidate the areas that need repainting.
    void AdvancePresentation(float elapsed)
    {
        if (smoke) AdvanceTemporaryEffects(elapsed);
        effects.ReducedMotion = profile.ReducedMotion;
        effects.Ambient = overlay.Length == 0;
        effects.Theme = CurrentChapter.Theme;
        effects.Advance(elapsed);
        if (overlay == "win") backdropDirty = true;
        foreach (var button in Controls.OfType<GameButton>()) button.Advance(effects.Time, profile.ReducedMotion);
        UpdateGemAnimation();
        AdvanceWheel();
        if(effects.Time<dealUntil+.1){backdropDirty=true;Invalidate();}
    }
    // Rebuild cached scenery only when the underlying scene changes.
    void RefreshBackdrop()
    {
        if (!backdropDirty && backdrop?.Size == ClientSize) return;
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
        var next = new Bitmap(ClientSize.Width, ClientSize.Height);
        using (var graphics = Graphics.FromImage(next)) { graphics.Clear(BackColor); PaintBackdrop(graphics); }
        var previous = backdrop; backdrop = next; backdropDirty = false;
        foreach (var button in Controls.OfType<GameButton>()) button.Backdrop = next;
        previous?.Dispose();
    }
    // Format the garden's remaining persisted refresh delay for the native controls.
    string GardenRefreshCaption() => session.GardenRefreshRemaining>0 ? "Fresh garden " + TimeSpan.FromSeconds(Math.Ceiling(session.GardenRefreshRemaining)).ToString(@"hh\:mm\:ss") : "Fresh garden";
    readonly Dictionary<int,double> dealReveals=new();
    double dealUntil=-100,dealStarted=-100;
    // Start decorative entrance effects for the newly displayed screen.
    void PresentSceneEntrance()
    {
        if(screen=="play"&&hand!=null&&!profile.ReducedMotion)
        {
            dealStarted=effects.Time;dealReveals.Clear();int order=0;
            for(int index=0;index<hand.Removed.Count;index++)if(!hand.Removed[index])
            {
                float delay=order++*.018f;effects.Add(new DealingCard(hand.Deal.Cards[index],hand.IsExposed(index),new(469,600),CardCenter(index),hand.Deal.Layout[index].Angle,delay));
                dealReveals[index]=effects.Time+delay+.43;
                if(Controls.Find("card"+index,false).FirstOrDefault() is GameButton card)card.RevealAt=effects.Time+delay+.43;
            }
            dealUntil=effects.Time+order*.018+.48;return;
        }
        int i = 0;
        foreach (var button in Controls.OfType<GameButton>().Where(b => b.Name.StartsWith("card") || b.Name.StartsWith("level") || b.Name.StartsWith("gem"))) button.BeginEntrance(effects.Time, i++ * .018f);
    }
    // Resolve a card's displayed rectangle from the current native layout.
    RectangleF CardBounds(int index)
    {
        var slot = hand!.Deal.Layout[index];
        if(slot.DrawX.HasValue)
        {
            double a=slot.Angle*Math.PI/180;float w=(float)(Math.Abs(Math.Cos(a))*76+Math.Abs(Math.Sin(a))*106)+8,h=(float)(Math.Abs(Math.Sin(a))*76+Math.Abs(Math.Cos(a))*106)+8;
            return new(140+slot.DrawX.Value-w/2,196+slot.DrawY!.Value-h/2,w,h);
        }
        int rows = hand.Deal.Layout.Max(s => s.Row) + 1;
        if (!hand.Deal.TemporaryRules && rows <= 4) return new(140 + slot.X * 87, 208 + slot.Row * 59, 79, 111);
        float height = Math.Min(111, 310f / (1 + (rows - 1) * .54f)), width = height * .72f;
        float min = hand.Deal.Layout.Min(s => s.X), max = hand.Deal.Layout.Max(s => s.X);
        float step = width / .6f;
        float shrink = Math.Min(1, 1000f / ((max-min)*step+width));
        height *= shrink; width *= shrink; step *= shrink;
        float stride = rows > 1 ? Math.Min(height * .7f, (310 - height) / (rows - 1)) : 0;
        return new(640 - ((min+max)*step+width)/2 + slot.X*step, 201 + slot.Row * stride, width, height);
    }
    // Find a card's displayed center for flight and sparkle origins.
    PointF CardCenter(int index)
    {
        var r = CardBounds(index); return new(r.X + r.Width / 2, r.Y + r.Height / 2);
    }
    double timerPaint, timerSave;
    // Advance the hand's timed effects and refresh their native indicators.
    void AdvanceTemporaryEffects(double seconds)
    {
        if (screen != "play" || overlay.Length > 0 || (!smoke && (!ContainsFocus || WindowState == FormWindowState.Minimized)) || hand is not ITemporaryHand timed || hand.Won) return;
        int rank = hand.Waste.Rank, stockCount=hand.Stock.Count; session.AdvanceTime(seconds);
        timerPaint += seconds; timerSave += seconds;
        if (rank != hand.Waste.Rank || stockCount!=hand.Stock.Count) { RefreshHandView(); return; }
        foreach (var button in Controls.OfType<GameButton>())
        {
            if (button.Name.StartsWith("card") && int.TryParse(button.Name.Substring(4), out int i))
            {
                bool highlight = selectedPowerup != null || timed.Effects.HintSeconds > 0 && (profile.Hints && hand.CanPlay(i) || cardHint == i);
                string badge = (hand as IAdventureHand)?.CardStatus(i) ?? "";
                if (button.Hint != highlight || button.CardBadge != badge) { button.Hint = highlight; button.CardBadge = badge; button.Invalidate(); }
            }
            if (button.Name.StartsWith("power-") && session.Powerups.Find(button.Name.Substring(6)) is { } helper)
            {
                button.Enabled=timed.Effects.HelperCooldown<=0 && helper.CanUse(hand);
                string subtitle=PowerupCaption(helper);
                if(button.Subtitle!=subtitle) {button.Subtitle=subtitle;button.Invalidate();}
            }
            if(button.Name=="draw") {button.Enabled=hand.Stock.Count>0 && !hand.Won && (hand as IAdventureHand)?.Failed!=true;button.CardBadge="";button.Invalidate();}
            if (button.Name == "hint") { button.Enabled = timed.Effects.HintCooldown <= 0 && (hand as IAdventureHand)?.Failed != true; button.Text = timed.Effects.HintCooldown > 0 ? $"Hint in {Math.Ceiling(timed.Effects.HintCooldown)}s" : "\U0001f50e Hint 3s"; }
        }
        if(Controls.Find("remix",false).FirstOrDefault() is GameButton remix) {remix.Enabled=profile.FreshDealCooldown<=0;remix.Text=profile.FreshDealCooldown>0 ? $"Fresh in {Math.Ceiling(profile.FreshDealCooldown)}s" : "\U0001f3b2 Remix";}
        UpdatePeekControls();
        if (timerPaint >= .2) { timerPaint = 0; backdropDirty = true; Invalidate(); }
        if (timerSave >= 5) { timerSave = 0; Save(); }
    }
    // Collect uncovered board indices for reveal and hint presentation.
    HashSet<int> ExposedCards() => hand == null ? new() : Enumerable.Range(0, hand.Removed.Count).Where(hand.IsExposed).ToHashSet();
    // Present newly exposed faces without altering the engine's covering graph.
    void RevealCards(HashSet<int> previous)
    {
        if (hand == null || profile.ReducedMotion) return;
        foreach (int index in ExposedCards().Except(previous)) if (Controls.Find("card" + index, false).FirstOrDefault() is GameButton button)
        {
            button.BeginEntrance(effects.Time, .07f);
            effects.Burst(CardCenter(index), Color.FromArgb(254, 219, 130), 7);
        }
    }
    // Animate the rewards already granted by the session.
    void RewardEffects(int coins, PointF origin)
    {
        effects.Coins(coins, origin); effects.Burst(origin, artwork.Gold, 24);
        effects.Cheer($"+{coins} little reasons to smile!", new(820, 530)); Chime(SoundCue.Coins);
    }
    GemAnimationFrame? reducedGemFrame;double reducedGemUntil;
    // Begin playback of the jewel engine's recorded cascade frames.
    void StartGemAnimation()
    {
        if(gems is not IAnimatedCoinGame animated || animated.LastAnimation.Count==0)return;
        if(profile.ReducedMotion){reducedGemFrame=animated.LastAnimation.FirstOrDefault(f=>f.Kind==GemFrameKind.Match);reducedGemUntil=effects.Time+.22;return;}
        gemAnimation = new(animated.LastAnimation, effects.Time);
        UpdateGemAnimation();
        AdvanceWheel();
        if(effects.Time<dealUntil+.1){backdropDirty=true;Invalidate();}
    }
    // Advance the native jewel presentation without replaying reward logic.
    void UpdateGemAnimation()
    {
        if(reducedGemFrame!=null && effects.Time>=reducedGemUntil){reducedGemFrame=null;if(screen=="gems")foreach(var b in Controls.OfType<GameButton>().Where(b=>b.Name.StartsWith("gem")&&int.TryParse(b.Name.Substring(3),out _))){int i=int.Parse(b.Name.Substring(3));b.Gem=gems!.Cells[i];b.MatchGlow=0;b.Invalidate();}}
        if (gemAnimation == null) return;
        if (screen != "gems" || overlay.Length > 0 || profile.ReducedMotion) { gemAnimation = null; return; }
        bool changed = gemAnimation.Advance(effects.Time);
        if (gemAnimation.Finished) { gemAnimation = null; Build(); return; }
        var frame = gemAnimation.Current; float progress = gemAnimation.Progress;
        // Map the animated jewel to its source row for drop interpolation.
        int SourceRow(int i){int col=i%8,row=i/8;var kept=Enumerable.Range(0,8).Where(r=>!frame.Matched.Contains(r*8+col)).ToArray();int missing=8-kept.Length;return row<missing?row-missing:kept[row-missing];}
        for (int i = 0; i < frame.Cells.Count; i++)
        {
            if (Controls.Find("gem" + i, false).FirstOrDefault() is not GameButton button) continue;
            button.Gem = frame.Cells[i]; button.Enabled = false; button.VisualOffset = PointF.Empty;
            button.HideGem = frame.Kind == GemFrameKind.Swap && (i == frame.First || i == frame.Second) || frame.Kind==GemFrameKind.Drop && SourceRow(i)!=i/8;
            button.MatchGlow = frame.Kind == GemFrameKind.Match && frame.Matched.Contains(i) ? MathF.Sin(progress * MathF.PI) : 0;
            if (frame.Kind == GemFrameKind.Swap && (i == frame.First || i == frame.Second))
            {
                // The moving gem is drawn above controls by the click-through effects layer.
            }

            button.Invalidate();
        }
        if (changed && frame.Kind == GemFrameKind.Swap)
        {
            // Resolve the drawing center used by the current native effect.
            PointF Center(int i) => new(462 + i % 8 * 64, 218 + i / 8 * 60);
            effects.FlyGem(frame.Cells[frame.First], Center(frame.Second), Center(frame.First));
            effects.FlyGem(frame.Cells[frame.Second], Center(frame.First), Center(frame.Second));
        }
        if(changed && frame.Kind==GemFrameKind.Drop)for(int i=0;i<64;i++)if(SourceRow(i)!=i/8)effects.FlyGem(frame.Cells[i],new(462+i%8*64,218+SourceRow(i)*60),new(462+i%8*64,218+i/8*60));
        if (changed && frame.Kind == GemFrameKind.Match)
        {
            effects.Add(new GemMatchLinks(frame.Matched));effects.Cheer($"+{frame.Matched.Count*2*frame.Cascade} coins",new(688,180),artwork.Teal);
            foreach (int i in frame.Matched) effects.Burst(new(462 + i % 8 * 64, 218 + i / 8 * 60), WorldPalette.For(i % 20).Accent, 7);
            if (frame.Cascade > 1) effects.Cheer($"Cascade ×{frame.Cascade}!", new(1090, 531), Color.FromArgb(144, 100, 172));
        }
    }
    // Draw the current chapter label and saved progress details.
    void PaintChapterDetails(Graphics g)
    {
        var chapter = CurrentChapter;
        int cleared = Enumerable.Range(chapter.FirstLevel, chapter.LastLevel - chapter.FirstLevel + 1).Count(profile.Stars.ContainsKey);
        Art.Text(g, chapter.Invitation, 270, 185, 14, artwork.Teal, false, 740, true);
        Art.Box(g, new(434, 660, 412, 12), Color.FromArgb(120, Color.White), 6);
        if (cleared > 0) Art.Box(g, new(434, 660, 412f * cleared / campaign.LevelsPerChapter, 12), artwork.Gold, 6);
        Art.Text(g, $"{cleared}/{campaign.LevelsPerChapter} blossoms · Chapter gift: {session.Economy.ChapterGift} coins + {session.Economy.ChapterWilds} Wild", 403, 636, 13, artwork.Ink, true, 480, true);
        FestiveArt.Pip(g, 130, 536, .8f);
        Art.Box(g, new(55, 394, 145, 69), Color.FromArgb(240, artwork.Cream), 17);
        Art.Text(g, "You’re doing\nsomething lovely.", 62, 407, 15, artwork.Teal, true, 131, true);
    }
    // Draw committed win results and their current presentation phase.
    void PaintVictory(Graphics g)
    {
        float progress = profile.ReducedMotion ? 1 : Math.Clamp((float)(effects.Time - victoryStarted) / 1.1f, 0, 1);
        string heading = lastVictory?.BonusCoins > 0 ? "Bonus level complete!" : lastVictory?.ChapterName != null ? "A whole chapter of happiness!" : "You made the meadow smile!";
        Art.Text(g, heading, 260, 183, lastVictory?.ChapterName != null ? 32 : 37, artwork.Ink, true, 760, true);
        for (int i = 0; i < 3; i++)
        {
            float appear = profile.ReducedMotion ? 1 : Math.Clamp((float)(effects.Time - victoryStarted - i * .14) / .42f, 0, 1);
            float size = 32 * (float)Math.Sin(appear * Math.PI / 2);
            FestiveArt.Star(g, 552 + i * 88, 291, Math.Max(1, size), i < stars ? artwork.Gold : Color.FromArgb(221, 219, 198), i < stars ? (1 - appear) * -30 : 0);
        }
        Art.Text(g, $"+{(int)(reward * (1 - MathF.Pow(1 - progress, 3)))} coins", 280, 348, 37, artwork.Teal, true, 720, true);
        string detail = lastVictory?.BonusCoins > 0 ? $"+{lastVictory.BonusCoins} \U0001fa99    +1 {session.Powerups.Find(lastVictory.BonusItem ?? "")?.Name}\nOne-use helper added to your bag" : lastVictory?.ChapterName != null ? $"Chapter complete! Includes {lastVictory.ChapterBonus} bonus coins + {session.Economy.ChapterWilds} Wild.\nA new little adventure is waiting for you." : $"Score {hand?.Score:N0} · Best streak {hand?.BestStreak}\nOne more blossom in your growing garden. Beautifully done!";
        Art.Text(g, detail, 310, 410, 19, artwork.Teal, false, 660, true);
        FestiveArt.Pip(g, 320, 337, .7f, true); FestiveArt.Pip(g, 960, 337, .7f, true);
        Art.Text(g, "Your progress is saved. Your next smile is just a card away.", 280, 480, 14, artwork.Teal, false, 720, true);
    }
    // Draw active effect timers and helper decoration around the hand.
    void PaintTemporaryEffects(Graphics g)
    {
        if (hand is not ITemporaryHand timed || hand.Won) return;
        var e = timed.Effects;
        var active = new[] {
            ("Hint",e.HintSeconds),("Lucky +1",e.LuckySeconds),("Thaw",e.ThawSeconds),("Shield",e.ShieldSeconds),
            ("Wild",hand.Waste.Rank==0 ? e.WildSeconds:0),("Recharge",e.HelperCooldown),("Keypass",e.KeySeconds),("Prune",e.PruneSeconds),("Phase",e.PhaseSeconds),("Peek",e.PeekSeconds),("Fresh +5",e.FreshSeconds),("Draw ready",e.DrawCooldown)
        }.Where(x=>x.Item2>0).Take(6).ToArray();
        for(int i=0;i<active.Length;i++)
        {
            float x=935+i%2*108,y=533+i/2*38;
            Art.Box(g,new(x,y,101,33),Color.FromArgb(240,artwork.Cream),10);
            Art.Text(g,$"{active[i].Item1} {Math.Ceiling(active[i].Item2)}s",x+3,y+6,12,artwork.Ink,true,95,true);
            Art.Box(g,new(x+7,y+29,(float)(87*Math.Min(1,active[i].Item2/12)),3),artwork.Gold,1);
        }
        if (e.PeekSeconds > 0)
        {
            Art.Box(g, new(140, 533, 230, 130), artwork.Cream, 14);
            Art.Text(g, $"Next 3 cards - {Math.Ceiling(e.PeekSeconds)}s", 150, 536, 12, artwork.Teal, true, 210, true);
            for (int n = 0; n < Math.Min(3, hand.Stock.Count); n++) artwork.DrawCard(g, new(154+n*66, 558, 56, 83), hand.Stock[n], true, false, false);
        }
        if (e.ThawSeconds > 0 || e.ShieldSeconds > 0 || e.LuckySeconds > 0)
        {
            using var pen = new Pen(e.ThawSeconds > 0 ? Color.LightSkyBlue : e.ShieldSeconds > 0 ? Color.Turquoise : artwork.Gold, 3);
            using var path = Art.Round(new(115, 195, 1051, 322), 24); g.DrawPath(pen, path);
        }
    }
    // Render formation previews for inspecting the authored native layouts.
    void SaveFormationAtlas(string path)
    {
        using var image = new Bitmap(1280, 1210);
        using var g = Graphics.FromImage(image);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(artwork.Cream);
        Art.Text(g, "32 examples from 480,000 distinct formations", 24, 12, 24, artwork.Ink, true);
        var layouts = new AdventureLayouts();
        for (int n = 0; n < 32; n++)
        {
            float x = n % 4 * 320, y = 55 + n / 4 * 143;
            Art.Box(g, new(x + 8, y, 304, 135), Color.FromArgb(223, 235, 213), 12);
            Art.Text(g, AdventureLayouts.NameFor(2100 + n), x + 18, y + 6, 14, artwork.Ink, true);
            foreach (var slot in layouts.Create(2100 + n))
                artwork.DrawCard(g, new(x + 18 + slot.X * 25, y + 30 + slot.Row * (75f / (AdventureLayouts.DepthFor(2100 + n) - 1)), 22, 24), new Card(1, 0), false, false, false);
        }
        image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
}
