namespace MeadowPeaks;

// Undo captures board and mechanic state together. Restoring only the visible cards would
// leave keys, timers or other obstacle state inconsistent with the restored move.
public record Snapshot(bool[] Removed, List<Card> Stock, Card Waste, int Score, int Streak, MechanicState? Mechanics = null);
public sealed record SavedRun(int Seed, int Level, bool Endless, bool[] Removed, List<Card> Stock, Card Waste, int Score, int Streak, int BestStreak, int FreeUndos, bool Paid, Deal? DealtBoard = null, MechanicState? Mechanics = null);
public sealed partial class Hand : ISolitaireHand, IAdventureHand
{
    readonly ICardMechanics mechanics;
    MechanicState state = new();
    public bool Failed => state.Failed;
    int ArmorDamage(int i) => state.ArmorDamage != null && state.ArmorDamage.TryGetValue(i, out int damage) ? damage : ((state.Peeled & (1UL << i)) != 0 ? 1 : 0);
    public string CardStatus(int i) => Deal.Cards[i].Kind == CardKind.Stone ? $"\u25c6{Math.Max(1, Math.Max(2, Deal.Cards[i].Strength)-ArmorDamage(i))}" : mechanics.Status(Deal.Cards[i], state);
    public string BlockReason(int i)
    {
        if(Failed) return "A bomb burst. Undo the last move or replay this deal.";
        var card=Deal.Cards[i];
        if(mechanics.Allows(card,state,Waste)) return "Play a card one rank higher or lower than the pile.";
        return card.Kind switch {
            CardKind.Vine => $"Vines: clear {Math.Max(2,card.Strength)} cards in a row first.",
            CardKind.Lock => "Lock: collect the remaining keys first.",
            CardKind.Ice => $"Ice: clear {Math.Max(0,card.Strength-state.Clears)} other cards to thaw it.",
            CardKind.Gate => "Gate: clear a card or draw to switch sun and moon.",
            CardKind.ColorSeal => "Color seal: match the pile color as well as the rank.",
            _ => "Play one rank higher or lower than the pile." };
    }
    public Deal Deal { get; }
    bool[] removed;
    public IReadOnlyList<bool> Removed => Array.AsReadOnly(removed);
    List<Card> stock;
    public IReadOnlyList<Card> Stock => stock.AsReadOnly();
    public Card Waste { get; private set; }
    public int Score { get; private set; }
    public int Streak { get; private set; }
    public int BestStreak { get; private set; }
    public int FreeUndos { get; private set; } = 3;
    public bool Paid { get; set; }
    readonly Stack<Snapshot> history = new();
    readonly ISolitaireRules rules;
    public Hand(Deal deal, ISolitaireRules rules, ICardMechanics? mechanics = null) { this.mechanics = mechanics ?? new CardMechanics(); this.rules = rules; Deal = deal; state = new(HintSeconds: deal.OpeningEffect == "glow" ? 8 : deal.TemporaryRules ? 3 : 0, LuckySeconds: deal.OpeningEffect == "harvest" ? 6 : 0, ShieldSeconds: deal.OpeningEffect == "shelter" ? 6 : 0); removed = new bool[deal.Cards.Count]; stock = deal.Stock.ToList(); Waste = deal.Waste; }
    public bool IsExposed(int i) => i >= 0 && i < removed.Length && rules.Exposed(Deal.Layout, removed, i);
    public SavedRun Capture(int seed, int level, bool endless) => new(seed, level, endless, (bool[])removed.Clone(), stock.ToList(), Waste, Score, Streak, BestStreak, FreeUndos, Paid, Deal, state);
    internal static Hand? Restore(SavedRun run, Deal deal, ISolitaireRules rules, ICardMechanics? mechanics = null)
    {
        if (run.Level < 1 || (!run.Endless && run.Level > MeadowCampaign.TotalLevels) || run.Seed < 0 || run.Removed == null || run.Stock == null || run.Waste == null || run.Score < 0 || run.Streak < 0 || run.BestStreak < run.Streak || run.FreeUndos < 0 || run.FreeUndos > 3 || run.Waste.Rank < 0 || run.Waste.Rank > 13 || run.Waste.Suit < 0 || run.Waste.Suit > 3 || run.Stock.Count > 100 || run.Stock.Any(c => c == null || c.Rank < 1 || c.Rank > 13 || c.Suit < 0 || c.Suit > 3)) return null;
        var h = new Hand(deal, rules, mechanics);
        if (run.Removed.Length != h.removed.Length || (run.Paid && run.Removed.Any(x => !x))) return null;
        if (run.Mechanics is { } m && (m.Turns < 0 || m.Clears < 0 || m.Clears > deal.Cards.Count || m.Run < 0 || m.FuseDelay < 0 || m.Keys < 0 || m.Keys > deal.Cards.Count || (m.ArmorDamage != null && m.ArmorDamage.Any(x => x.Key < 0 || x.Key >= deal.Cards.Count || x.Value < 1 || x.Value > 3)) || new[] { m.HintSeconds, m.HintCooldown, m.HelperCooldown, m.LuckySeconds, m.ThawSeconds, m.ShieldSeconds, m.WildSeconds, m.KeySeconds, m.PruneSeconds, m.PhaseSeconds, m.PeekSeconds, m.FreshSeconds, m.DrawCooldown }.Any(t => !double.IsFinite(t) || t < 0 || t > 120) || (m.WildReturn != null && (m.WildReturn.Rank < 1 || m.WildReturn.Rank > 13 || m.WildReturn.Suit < 0 || m.WildReturn.Suit > 3)))) return null;
        h.state = run.Mechanics ?? h.state;
        h.removed = (bool[])run.Removed.Clone(); h.stock = run.Stock.ToList(); h.Waste = run.Waste; h.Score = run.Score; h.Streak = run.Streak; h.BestStreak = run.BestStreak; h.FreeUndos = run.FreeUndos; h.Paid = run.Paid;
        return h;
    }
    public bool Won => removed.All(x => x);
    public bool CanPlay(int i) => i >= 0 && i < removed.Length && !Won && !Failed && mechanics.Allows(Deal.Cards[i], state, Waste) && IsExposed(i) && rules.Adjacent(Waste.Rank, Deal.Cards[i].Rank);
    public IEnumerable<int> Moves => Enumerable.Range(0, removed.Length).Where(CanPlay);
    public bool CanUndo => history.Count > 0 && !Paid;
    void Remember() => history.Push(new((bool[])removed.Clone(), stock.ToList(), Waste, Score, Streak, state));
    // Check legality before taking a snapshot or mutating the hand. Rejected input must not
    // consume an undo step or advance obstacles that count successful actions.
    public bool Play(int i)
    {
        if (!CanPlay(i)) return false;
        Remember();
        if (Deal.Cards[i].Kind == CardKind.Stone && ArmorDamage(i) < Math.Max(2, Deal.Cards[i].Strength)-1)
        {
            var damage = state.ArmorDamage?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<int,int>();
            damage[i] = ArmorDamage(i)+1;
            state = mechanics.Advance(state with { Peeled = state.Peeled | (1UL << i), ArmorDamage = damage }, Deal, removed, null);
            return true;
        }
        var previous = Waste; removed[i] = true; Waste = Deal.Cards[i];
        int bonus = mechanics.Bonus(Waste, previous) + (state.LuckySeconds > 0 ? 1 : 0);
        for (int n = 0; n < bonus && stock.Count < 100; n++) stock.Add(new Card((Waste.Rank + n + 3) % 13 + 1, (Waste.Suit + n) % 4));
        state = mechanics.Advance(state, Deal, removed, Waste); OnCardCleared(Waste); Streak = (int)Math.Min(int.MaxValue, (long)Streak + 1); BestStreak = Math.Max(BestStreak, Streak);
        Score = (int)Math.Min(int.MaxValue, (long)Score + 20 + (long)Streak * 10 + (Deal.Layout[i].Row == 0 ? 100 : 0));
        return true;
    }
    public bool Draw()
    {
        if (stock.Count == 0 || Won || Failed) return false;
        Remember(); Waste = stock[0]; stock.RemoveAt(0); Streak = 0; state = mechanics.Advance(state, Deal, removed, null) with {DrawCooldown=0}; return true;
    }
    public bool Undo()
    {
        if (!CanUndo) return false;
        var current = state; var s = history.Pop(); removed = s.Removed; stock = s.Stock; Waste = s.Waste; Score = s.Score; Streak = s.Streak; state = (s.Mechanics ?? new()) with {
            HintSeconds = Math.Min(s.Mechanics?.HintSeconds ?? 0, current.HintSeconds),
            HintCooldown = Math.Max(s.Mechanics?.HintCooldown ?? 0, current.HintCooldown),
            DrawCooldown = Math.Max(s.Mechanics?.DrawCooldown ?? 0,current.DrawCooldown),
            HelperCooldown = Math.Max(s.Mechanics?.HelperCooldown ?? 0, current.HelperCooldown),
            LuckySeconds = Math.Min(s.Mechanics?.LuckySeconds ?? 0, current.LuckySeconds),
            ThawSeconds = Math.Min(s.Mechanics?.ThawSeconds ?? 0, current.ThawSeconds),
            ShieldSeconds = Math.Min(s.Mechanics?.ShieldSeconds ?? 0, current.ShieldSeconds),
            KeySeconds = Math.Min(s.Mechanics?.KeySeconds ?? 0, current.KeySeconds),
            PruneSeconds = Math.Min(s.Mechanics?.PruneSeconds ?? 0, current.PruneSeconds),
            PhaseSeconds = Math.Min(s.Mechanics?.PhaseSeconds ?? 0, current.PhaseSeconds),
            FreshSeconds = Math.Min(s.Mechanics?.FreshSeconds ?? 0,current.FreshSeconds),
            PeekSeconds = Math.Min(s.Mechanics?.PeekSeconds ?? 0, current.PeekSeconds),
            WildSeconds = Math.Min(s.Mechanics?.WildSeconds ?? 0, current.WildSeconds)
        };


        if (FreeUndos > 0) FreeUndos--; return true;
    }
    public bool Wild()
    {
        if (Failed || Won || Waste.Rank == 0) return false;
        history.Clear(); if (Deal.TemporaryRules) state = state with { WildReturn = null, WildSeconds = 0 }; Waste = new(0, 0); return true;
    }
    public bool Extra(int seed)
    {
        if (Failed || Won || stock.Count > 0) return false;
        history.Clear(); var rng = new Random(seed);
        state=state with {FreshSeconds=0};
        for (int i = 0; i < 5; i++) stock.Add(new(rng.Next(1, 14), rng.Next(4)){TemporaryStock=false});
        return true;
    }
    public bool Remove(int i)
    {
        if (Failed || Won || i < 0 || i >= removed.Length || !IsExposed(i)) return false;
        history.Clear(); removed[i] = true; Score = (int)Math.Min(int.MaxValue, (long)Score + 20); state = mechanics.Advance(state, Deal, removed, Deal.Cards[i]); OnCardCleared(Deal.Cards[i]); return true;
    }
}
