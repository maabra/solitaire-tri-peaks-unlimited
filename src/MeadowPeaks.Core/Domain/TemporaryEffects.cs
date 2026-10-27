namespace MeadowPeaks;

public interface ITemporaryHand
{
    MechanicState Effects { get; }
    // Advance timed hand effects independently of repaint frequency.
    void AdvanceTime(double seconds);
    // Request a bounded hint effect through the hand's cooldown rules.
    bool RequestHint();
    // Activate the named timed helper when its conditions are satisfied.
    bool UseEffect(string id);
    // Start the shared delay after a timed helper is accepted.
    void StartHelperCooldown();
}
public sealed partial class Hand : ITemporaryHand
{
    public MechanicState Effects => state;
    // Advance real-time effects independently of hazards that count successful actions.
    public void AdvanceTime(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0 || Won || Failed) return;
        // Update timed effects and clean up temporary faces or stock when their duration expires.
        double Tick(double value) => Math.Max(0, value - seconds);
        state = state with { HintSeconds = Tick(state.HintSeconds), HintCooldown = Tick(state.HintCooldown), HelperCooldown = Tick(state.HelperCooldown), LuckySeconds = Tick(state.LuckySeconds), ThawSeconds = Tick(state.ThawSeconds), ShieldSeconds = Tick(state.ShieldSeconds), WildSeconds = Tick(state.WildSeconds), KeySeconds = Tick(state.KeySeconds), PruneSeconds = Tick(state.PruneSeconds), PhaseSeconds = Tick(state.PhaseSeconds), PeekSeconds = Tick(state.PeekSeconds), FreshSeconds=Tick(state.FreshSeconds), DrawCooldown=Tick(state.DrawCooldown) };

    }
    // Start hint highlighting only when a legal move and the hint cooldown permit it.
    public bool RequestHint()
    {
        if (Won || Failed || state.HintCooldown > 0 || !Moves.Any()) return false;
        state = state with { HintSeconds = 3, HintCooldown = 15 }; return true;
    }
    // Record the post-use delay before another helper can activate.
    public void StartHelperCooldown() { if (Deal.TemporaryRules) state = state with { HelperCooldown = 5 }; }
    // Apply the requested timed helper using the hand's current obstacle and cooldown state.
    public bool UseEffect(string id)
    {
        if (Won || Failed || state.HelperCooldown > 0) return false;
        // Check whether the remaining board contains the requested mechanic.
        bool Has(CardKind kind) => Deal.Cards.Where((c, i) => !removed[i]).Any(c => c.Kind == kind);
        switch (id)
        {
            case "lucky" when state.LuckySeconds <= 0: state = state with { LuckySeconds = 10 }; break;
            case "thaw" when state.ThawSeconds <= 0 && Has(CardKind.Ice): state = state with { ThawSeconds = 8 }; break;
            case "shield" when state.ShieldSeconds <= 0 && Has(CardKind.Bomb): state = state with { ShieldSeconds = 10 }; break;
            case "keypass" when state.KeySeconds <= 0 && Has(CardKind.Lock): state = state with { KeySeconds = 6 }; break;
            case "prune" when state.PruneSeconds <= 0 && Has(CardKind.Vine): state = state with { PruneSeconds = 8 }; break;
            case "phase" when state.PhaseSeconds <= 0 && Has(CardKind.Gate): state = state with { PhaseSeconds = 6 }; break;
            case "peek" when state.PeekSeconds <= 0 && stock.Count > 0: state = state with { PeekSeconds = 6 }; break;
            case "scout" when Moves.Any(): state = state with { HintSeconds = 5 }; break;
            default: return false;
        }
        history.Clear(); return true;
    }
    // Apply clear-dependent bonuses and cleanup after a card actually leaves the board.
    void OnCardCleared(Card card)
    {
        state = card.Kind switch
        {
            CardKind.Lantern => state with { HintSeconds = 4 },
            CardKind.Lucky => state with { LuckySeconds = Math.Max(state.LuckySeconds, 6) },
            CardKind.Shield => state with { ShieldSeconds = Math.Max(state.ShieldSeconds, 6) },
            _ => state
        };
    }
}
