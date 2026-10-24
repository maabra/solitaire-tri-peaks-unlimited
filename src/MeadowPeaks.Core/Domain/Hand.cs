namespace MeadowPeaks;

public record Snapshot(bool[] Removed, List<Card> Stock, Card Waste, int Score, int Streak);
public sealed record SavedRun(int Seed, int Level, bool Endless, bool[] Removed, List<Card> Stock, Card Waste, int Score, int Streak, int BestStreak, int FreeUndos, bool Paid, Deal? DealtBoard = null);
public sealed class Hand : ISolitaireHand
{
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
    public Hand(Deal deal, ISolitaireRules rules) { this.rules = rules; Deal = deal; removed = new bool[deal.Cards.Count]; stock = deal.Stock.ToList(); Waste = deal.Waste; }
    public bool IsExposed(int i) => i >= 0 && i < removed.Length && rules.Exposed(Deal.Layout, removed, i);
    public SavedRun Capture(int seed, int level, bool endless) => new(seed, level, endless, (bool[])removed.Clone(), stock.ToList(), Waste, Score, Streak, BestStreak, FreeUndos, Paid, Deal);
    internal static Hand? Restore(SavedRun run, Deal deal, ISolitaireRules rules)
    {
        if (run.Level < 1 || (!run.Endless && run.Level > 2000) || run.Seed < 0 || run.Removed == null || run.Stock == null || run.Waste == null || run.Score < 0 || run.Streak < 0 || run.BestStreak < run.Streak || run.FreeUndos < 0 || run.FreeUndos > 3 || run.Waste.Rank < 0 || run.Waste.Rank > 13 || run.Waste.Suit < 0 || run.Waste.Suit > 3 || run.Stock.Count > 100 || run.Stock.Any(c => c == null || c.Rank < 1 || c.Rank > 13 || c.Suit < 0 || c.Suit > 3)) return null;
        var h = new Hand(deal, rules);
        if (run.Removed.Length != h.removed.Length || (run.Paid && run.Removed.Any(x => !x))) return null;
        h.removed = (bool[])run.Removed.Clone(); h.stock = run.Stock.ToList(); h.Waste = run.Waste; h.Score = run.Score; h.Streak = run.Streak; h.BestStreak = run.BestStreak; h.FreeUndos = run.FreeUndos; h.Paid = run.Paid;
        return h;
    }
    public bool Won => removed.All(x => x);
    public bool CanPlay(int i) => i >= 0 && i < removed.Length && !Won && IsExposed(i) && rules.Adjacent(Waste.Rank, Deal.Cards[i].Rank);
    public IEnumerable<int> Moves => Enumerable.Range(0, removed.Length).Where(CanPlay);
    public bool CanUndo => history.Count > 0 && !Paid;
    void Remember() => history.Push(new((bool[])removed.Clone(), stock.ToList(), Waste, Score, Streak));
    public bool Play(int i)
    {
        if (!CanPlay(i)) return false;
        Remember(); removed[i] = true; Waste = Deal.Cards[i]; Streak++; BestStreak = Math.Max(BestStreak, Streak);
        Score += 20 + Streak * 10; if (Deal.Layout[i].Row == 0) Score += 100;
        return true;
    }
    public bool Draw()
    {
        if (stock.Count == 0 || Won) return false;
        Remember(); Waste = stock[0]; stock.RemoveAt(0); Streak = 0; return true;
    }
    public bool Undo()
    {
        if (!CanUndo) return false;
        var s = history.Pop(); removed = s.Removed; stock = s.Stock; Waste = s.Waste; Score = s.Score; Streak = s.Streak;
        if (FreeUndos > 0) FreeUndos--; return true;
    }
    public bool Wild()
    {
        if (Won || Waste.Rank == 0) return false;
        history.Clear(); Waste = new(0, 0); return true;
    }
    public bool Extra(int seed)
    {
        if (Won || stock.Count > 0) return false;
        history.Clear(); var rng = new Random(seed);
        for (int i = 0; i < 5; i++) stock.Add(new(rng.Next(1, 14), rng.Next(4)));
        return true;
    }
    public bool Remove(int i)
    {
        if (Won || i < 0 || i >= removed.Length || !IsExposed(i)) return false;
        history.Clear(); removed[i] = true; Score += 20; return true;
    }
}
