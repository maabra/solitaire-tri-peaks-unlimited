namespace MeadowPeaks;

public interface ITemporaryHand
{
    MechanicState Effects { get; }
    void AdvanceTime(double seconds);
    bool RequestHint();
    bool UseEffect(string id);
    void StartHelperCooldown();
}
public sealed partial class Hand : ITemporaryHand
{
    public MechanicState Effects => state;
    public void AdvanceTime(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0 || Won || Failed) return;
        double Tick(double value) => Math.Max(0, value - seconds);
        state = state with { HintSeconds = Tick(state.HintSeconds), HintCooldown = Tick(state.HintCooldown), HelperCooldown = Tick(state.HelperCooldown), LuckySeconds = Tick(state.LuckySeconds), ThawSeconds = Tick(state.ThawSeconds), ShieldSeconds = Tick(state.ShieldSeconds), WildSeconds = Tick(state.WildSeconds), KeySeconds = Tick(state.KeySeconds), PruneSeconds = Tick(state.PruneSeconds), PhaseSeconds = Tick(state.PhaseSeconds), PeekSeconds = Tick(state.PeekSeconds), FreshSeconds=Tick(state.FreshSeconds) };
        if(state.FreshSeconds<=0) stock.RemoveAll(c=>c.TemporaryStock);
        if (Deal.TemporaryRules && state.WildSeconds <= 0 && state.WildReturn != null)
        {
            if (Waste.Rank == 0) Waste = state.WildReturn;
            state = state with { WildReturn = null };
        }
    }
    public bool RequestHint()
    {
        if (Won || Failed || state.HintCooldown > 0 || !Moves.Any()) return false;
        state = state with { HintSeconds = 3, HintCooldown = 15 }; return true;
    }
    public void StartHelperCooldown() { if (Deal.TemporaryRules) state = state with { HelperCooldown = 5 }; }
    public bool UseEffect(string id)
    {
        if (Won || Failed || state.HelperCooldown > 0) return false;
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
