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

    void AnimationTick(object? sender, EventArgs args)
    {
        double now = presentationClock.Elapsed.TotalSeconds;
        float elapsed = (float)Math.Clamp(now - lastFrame, 0, .08); lastFrame = now;
        AdvancePresentation(elapsed);
        if (!smoke && effectsWindow != null)
        {
            effectsWindow.ViewScale = scale; effectsWindow.ViewOffset = new(offsetX, offsetY);
            effectsWindow.Follow(this, true);
        }
        if (overlay == "win" && effects.Time - victoryStarted < 1.8 && !profile.ReducedMotion) { backdropDirty = true; Invalidate(Rect(300, 250, 680, 220)); }
    }
    void AdvancePresentation(float elapsed)
    {
        effects.ReducedMotion = profile.ReducedMotion;
        effects.Ambient = overlay.Length == 0;
        effects.Theme = CurrentChapter.Theme;
        effects.Advance(elapsed);
        if (overlay == "win") backdropDirty = true;
        foreach (var button in Controls.OfType<GameButton>()) button.Advance(effects.Time, profile.ReducedMotion);
        UpdateGemAnimation();
    }
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
    void PresentSceneEntrance()
    {
        int i = 0;
        foreach (var button in Controls.OfType<GameButton>().Where(b => b.Name.StartsWith("card") || b.Name.StartsWith("level") || b.Name.StartsWith("gem"))) button.BeginEntrance(effects.Time, i++ * .018f);
    }
    PointF CardCenter(int index)
    {
        var slot = hand!.Deal.Layout[index]; return new(179 + slot.X * 87, 260 + slot.Row * 59);
    }
    HashSet<int> ExposedCards() => hand == null ? new() : Enumerable.Range(0, hand.Removed.Count).Where(hand.IsExposed).ToHashSet();
    void RevealCards(HashSet<int> previous)
    {
        if (hand == null || profile.ReducedMotion) return;
        foreach (int index in ExposedCards().Except(previous)) if (Controls.Find("card" + index, false).FirstOrDefault() is GameButton button)
        {
            button.BeginEntrance(effects.Time, .07f);
            effects.Burst(CardCenter(index), Color.FromArgb(254, 219, 130), 7);
        }
    }
    void RewardEffects(int coins, PointF origin)
    {
        effects.Coins(coins, origin); effects.Burst(origin, artwork.Gold, 24);
        effects.Cheer($"+{coins} little reasons to smile!", new(820, 530)); Chime(SoundCue.Coins);
    }
    void StartGemAnimation()
    {
        if (profile.ReducedMotion || gems is not IAnimatedCoinGame animated || animated.LastAnimation.Count == 0) return;
        gemAnimation = new(animated.LastAnimation, effects.Time);
        UpdateGemAnimation();
    }
    void UpdateGemAnimation()
    {
        if (gemAnimation == null) return;
        if (screen != "gems" || overlay.Length > 0 || profile.ReducedMotion) { gemAnimation = null; return; }
        bool changed = gemAnimation.Advance(effects.Time);
        if (gemAnimation.Finished) { gemAnimation = null; Build(); return; }
        var frame = gemAnimation.Current; float progress = gemAnimation.Progress;
        for (int i = 0; i < frame.Cells.Count; i++)
        {
            if (Controls.Find("gem" + i, false).FirstOrDefault() is not GameButton button) continue;
            button.Gem = frame.Cells[i]; button.Enabled = false; button.VisualOffset = PointF.Empty;
            button.HideGem = frame.Kind == GemFrameKind.Swap && (i == frame.First || i == frame.Second);
            button.MatchGlow = frame.Kind == GemFrameKind.Match && frame.Matched.Contains(i) ? MathF.Sin(progress * MathF.PI) : 0;
            if (frame.Kind == GemFrameKind.Swap && (i == frame.First || i == frame.Second))
            {
                // The moving gem is drawn above controls by the click-through effects layer.
            }
            if (frame.Kind == GemFrameKind.Drop && frame.Matched.Any(m => m % 8 == i % 8)) button.VisualOffset = new(0, -22 * (1 - progress) * scale);
            button.Invalidate();
        }
        if (changed && frame.Kind == GemFrameKind.Swap)
        {
            PointF Center(int i) => new(462 + i % 8 * 64, 218 + i / 8 * 60);
            effects.FlyGem(frame.Cells[frame.First], Center(frame.Second), Center(frame.First));
            effects.FlyGem(frame.Cells[frame.Second], Center(frame.First), Center(frame.Second));
        }
        if (changed && frame.Kind == GemFrameKind.Match)
        {
            foreach (int i in frame.Matched) effects.Burst(new(462 + i % 8 * 64, 218 + i / 8 * 60), WorldPalette.For(i % 20).Accent, 7);
            if (frame.Cascade > 1) effects.Cheer($"Cascade ×{frame.Cascade}!", new(1090, 531), Color.FromArgb(144, 100, 172));
        }
    }
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
    void PaintVictory(Graphics g)
    {
        float progress = profile.ReducedMotion ? 1 : Math.Clamp((float)(effects.Time - victoryStarted) / 1.1f, 0, 1);
        string heading = lastVictory?.ChapterName != null ? "A whole chapter of happiness!" : "You made the meadow smile!";
        Art.Text(g, heading, 260, 183, lastVictory?.ChapterName != null ? 32 : 37, artwork.Ink, true, 760, true);
        for (int i = 0; i < 3; i++)
        {
            float appear = profile.ReducedMotion ? 1 : Math.Clamp((float)(effects.Time - victoryStarted - i * .14) / .42f, 0, 1);
            float size = 32 * (float)Math.Sin(appear * Math.PI / 2);
            FestiveArt.Star(g, 552 + i * 88, 291, Math.Max(1, size), i < stars ? artwork.Gold : Color.FromArgb(221, 219, 198), i < stars ? (1 - appear) * -30 : 0);
        }
        Art.Text(g, $"+{(int)(reward * (1 - MathF.Pow(1 - progress, 3)))} coins", 280, 348, 37, artwork.Teal, true, 720, true);
        string detail = lastVictory?.ChapterName != null ? $"Chapter complete! Includes {lastVictory.ChapterBonus} bonus coins + {session.Economy.ChapterWilds} Wild.\nA new little adventure is waiting for you." : $"Score {hand?.Score:N0} · Best streak {hand?.BestStreak}\nOne more blossom in your growing garden. Beautifully done!";
        Art.Text(g, detail, 310, 410, 19, artwork.Teal, false, 660, true);
        FestiveArt.Pip(g, 320, 337, .7f, true); FestiveArt.Pip(g, 960, 337, .7f, true);
        Art.Text(g, "Your progress is saved. Your next smile is just a card away.", 280, 480, 14, artwork.Teal, false, 720, true);
    }
    void SaveFormationAtlas(string path)
    {
        using var image = new Bitmap(1280, 1210);
        using var g = Graphics.FromImage(image);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(artwork.Cream);
        Art.Text(g, "32 examples from 4,096 distinct formations", 24, 12, 24, artwork.Ink, true);
        var layouts = new AdventureLayouts();
        for (int n = 0; n < 32; n++)
        {
            float x = n % 4 * 320, y = 55 + n / 4 * 143;
            Art.Box(g, new(x + 8, y, 304, 135), Color.FromArgb(223, 235, 213), 12);
            Art.Text(g, AdventureLayouts.NameFor(n + 5), x + 18, y + 6, 14, artwork.Ink, true);
            foreach (var slot in layouts.Create(n + 5))
                artwork.DrawCard(g, new(x + 18 + slot.X * 25, y + 30 + slot.Row * 22, 22, 33), new Card(1, 0), false, false, false);
        }
        image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
}
