namespace MeadowPeaks;

public record Snapshot(bool[] Removed, List<Card> Stock, Card Waste, int Score, int Streak, MechanicState? Mechanics = null);
public sealed record SavedRun(int Seed, int Level, bool Endless, bool[] Removed, List<Card> Stock, Card Waste, int Score, int Streak, int BestStreak, int FreeUndos, bool Paid, Deal? DealtBoard = null, MechanicState? Mechanics = null);
public sealed class Hand : ISolitaireHand, IAdventureHand
{
    readonly ICardMechanics mechanics;
    MechanicState state = new();
    public bool Failed => state.Failed;
    public string CardStatus(int i) => mechanics.Status(Deal.Cards[i], state);
    public string BlockReason(int i) => Failed ? "A bomb burst! Undo your last move or replay this deal for free." : !mechanics.Allows(Deal.Cards[i], state) ? Deal.Cards[i].Kind == CardKind.Lock ? "Find and clear the KEY card to open every lock." : $"Clear {Math.Max(0, Deal.Cards[i].Strength - state.Clears)} other cards to melt this ice. Drawing does not thaw ice." : "Play a card one rank above or below the pile.";
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
    public Hand(Deal deal, ISolitaireRules rules, ICardMechanics? mechanics = null) { this.mechanics = mechanics ?? new CardMechanics(); this.rules = rules; Deal = deal; removed = new bool[deal.Cards.Count]; stock = deal.Stock.ToList(); Waste = deal.Waste; }
    public bool IsExposed(int i) => i >= 0 && i < removed.Length && rules.Exposed(Deal.Layout, removed, i);
    public SavedRun Capture(int seed, int level, bool endless) => new(seed, level, endless, (bool[])removed.Clone(), stock.ToList(), Waste, Score, Streak, BestStreak, FreeUndos, Paid, Deal, state);
    internal static Hand? Restore(SavedRun run, Deal deal, ISolitaireRules rules, ICardMechanics? mechanics = null)
    {
        if (run.Level < 1 || (!run.Endless && run.Level > MeadowCampaign.TotalLevels) || run.Seed < 0 || run.Removed == null || run.Stock == null || run.Waste == null || run.Score < 0 || run.Streak < 0 || run.BestStreak < run.Streak || run.FreeUndos < 0 || run.FreeUndos > 3 || run.Waste.Rank < 0 || run.Waste.Rank > 13 || run.Waste.Suit < 0 || run.Waste.Suit > 3 || run.Stock.Count > 100 || run.Stock.Any(c => c == null || c.Rank < 1 || c.Rank > 13 || c.Suit < 0 || c.Suit > 3)) return null;
        var h = new Hand(deal, rules, mechanics);
        if (run.Removed.Length != h.removed.Length || (run.Paid && run.Removed.Any(x => !x))) return null;
        if (run.Mechanics is { } m && (m.Turns < 0 || m.Clears < 0 || m.Clears > deal.Cards.Count)) return null;
        h.state = run.Mechanics ?? new();
        h.removed = (bool[])run.Removed.Clone(); h.stock = run.Stock.ToList(); h.Waste = run.Waste; h.Score = run.Score; h.Streak = run.Streak; h.BestStreak = run.BestStreak; h.FreeUndos = run.FreeUndos; h.Paid = run.Paid;
        return h;
    }
    public bool Won => removed.All(x => x);
    public bool CanPlay(int i) => i >= 0 && i < removed.Length && !Won && !Failed && mechanics.Allows(Deal.Cards[i], state) && IsExposed(i) && rules.Adjacent(Waste.Rank, Deal.Cards[i].Rank);
    public IEnumerable<int> Moves => Enumerable.Range(0, removed.Length).Where(CanPlay);
    public bool CanUndo => history.Count > 0 && !Paid;
    void Remember() => history.Push(new((bool[])removed.Clone(), stock.ToList(), Waste, Score, Streak, state));
    public bool Play(int i)
    {
        if (!CanPlay(i)) return false;
        Remember(); var previous = Waste; removed[i] = true; Waste = Deal.Cards[i];
        int bonus = mechanics.Bonus(Waste, previous);
        for (int n = 0; n < bonus; n++) stock.Add(new Card((Waste.Rank + n + 3) % 13 + 1, (Waste.Suit + n) % 4));
        state = mechanics.Advance(state, Deal, removed, Waste); Streak++; BestStreak = Math.Max(BestStreak, Streak);
        Score += 20 + Streak * 10; if (Deal.Layout[i].Row == 0) Score += 100;
        return true;
    }
    public bool Draw()
    {
        if (stock.Count == 0 || Won || Failed) return false;
        Remember(); Waste = stock[0]; stock.RemoveAt(0); Streak = 0; state = mechanics.Advance(state, Deal, removed, null); return true;
    }
    public bool Undo()
    {
        if (!CanUndo) return false;
        var s = history.Pop(); removed = s.Removed; stock = s.Stock; Waste = s.Waste; Score = s.Score; Streak = s.Streak; state = s.Mechanics ?? new();
        if (FreeUndos > 0) FreeUndos--; return true;
    }
    public bool Wild()
    {
        if (Failed || Won || Waste.Rank == 0) return false;
        history.Clear(); Waste = new(0, 0); return true;
    }
    public bool Extra(int seed)
    {
        if (Failed || Won || stock.Count > 0) return false;
        history.Clear(); var rng = new Random(seed);
        for (int i = 0; i < 5; i++) stock.Add(new(rng.Next(1, 14), rng.Next(4)));
        return true;
    }
    public bool Remove(int i)
    {
        if (Failed || Won || i < 0 || i >= removed.Length || !IsExposed(i)) return false;
        history.Clear(); removed[i] = true; Score += 20; state = mechanics.Advance(state, Deal, removed, Deal.Cards[i]); return true;
    }
}
