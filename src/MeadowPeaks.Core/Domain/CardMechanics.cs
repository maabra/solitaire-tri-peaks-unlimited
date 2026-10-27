namespace MeadowPeaks;

// Optional capability: replacement hands need not implement adventure mechanics.
public interface IAdventureHand
{
    bool Failed { get; }
    // Describe the selected card's current obstacle badge.
    string CardStatus(int index);
    // Explain why the selected interaction is currently blocked.
    string BlockReason(int index);
}
public sealed record MechanicState(int Turns = 0, int Clears = 0, bool HasKey = false, bool Failed = false,
    int Run = 0, int FuseDelay = 0, ulong Peeled = 0,
    double HintSeconds = 0, double HintCooldown = 0, double HelperCooldown = 0,
    double LuckySeconds = 0, double ThawSeconds = 0, double ShieldSeconds = 0,
    double WildSeconds = 0, Card? WildReturn = null, int Keys = 0, IReadOnlyDictionary<int, int>? ArmorDamage = null, double KeySeconds = 0, double PruneSeconds = 0, double PhaseSeconds = 0, double PeekSeconds = 0, double FreshSeconds = 0, double DrawCooldown = 0);

// Stateless policy. All mutable state lives in snapshots, so saves and undo agree.
public interface ICardMechanics
{
    // Check obstacle eligibility using the supplied snapshot without mutating it.
    bool Allows(Card card, MechanicState state);
    // Check obstacle eligibility using the supplied snapshot without mutating it.
    bool Allows(Card card, MechanicState state, Card waste) => Allows(card,state);
    // Describe the card's current obstacle requirement for presentation.
    string Status(Card card, MechanicState state);
    // Return mechanic state after an accepted action, suitable for saving and undo.
    MechanicState Advance(MechanicState state, Deal deal, IReadOnlyList<bool> removed, Card? played);
    // Calculate the extra-stock bonus caused by a clear.
    int Bonus(Card played, Card previous);
}
public sealed class CardMechanics : ICardMechanics
{
    // Evaluate obstacle permissions from the supplied snapshot without owning mutable hand state.
    public bool Allows(Card card, MechanicState state) => card.Kind switch
    {
        CardKind.Ice => state.ThawSeconds > 0 || state.Clears >= card.Strength,
        CardKind.Lock => state.KeySeconds > 0 || Math.Max(state.Keys, state.HasKey ? 1 : 0) >= Math.Max(1, card.Strength),
        CardKind.Vine => state.PruneSeconds > 0 || state.Run >= Math.Max(2, card.Strength),
        CardKind.Gate => state.PhaseSeconds > 0 || state.Turns%2 == card.Strength,
        _ => true
    };
    // Evaluate obstacle permissions from the supplied snapshot without owning mutable hand state.
    public bool Allows(Card card, MechanicState state, Card waste) => Allows(card,state) &&
        (card.Kind != CardKind.ColorSeal || waste.Rank == 0 || (waste.Suit is 1 or 2) == (card.Strength == 1));
    // Describe the obstacle's current requirement from the card and mechanic counters.
    public string Status(Card card, MechanicState state) => card.Kind switch
    {
        CardKind.Ice => state.ThawSeconds > 0 || state.Clears >= card.Strength ? "\u2744\u2713" : $"\u2744{card.Strength - state.Clears}",
        CardKind.Key => "\U0001f511",
        CardKind.Lock => state.KeySeconds > 0 || Math.Max(state.Keys, state.HasKey ? 1 : 0) >= Math.Max(1, card.Strength) ? "\U0001f513" : $"\U0001f512{Math.Max(1, card.Strength)-Math.Max(state.Keys, state.HasKey ? 1 : 0)}",
        CardKind.Bomb => $"\U0001f4a3{Math.Max(0, card.Strength + state.FuseDelay - state.Turns)}",
        CardKind.Gift => "\u2660+3",
        CardKind.Up => "\u2191+2",
        CardKind.Down => "\u2193+2",
        CardKind.Stone => "\u25c6\u00d72",
        CardKind.Vine => state.PruneSeconds > 0 || state.Run >= Math.Max(2, card.Strength) ? "\U0001f33f\u2713" : $"\U0001f33f{Math.Max(2, card.Strength)}",
        CardKind.Lantern => "\U0001f5264s",
        CardKind.Lucky => "\U0001f340+1",
        CardKind.Shield => "\U0001f6e16s",
        CardKind.Gate => (card.Strength == 0 ? "\u2600" : "\u263e") + (state.PhaseSeconds > 0 || state.Turns%2==card.Strength ? "\u2713" : "\u00d7"),
        CardKind.ColorSeal => card.Strength == 1 ? "\u2665" : "\u2660",
        _ => ""
    };
    // Return the next mechanic state after a successful action, retaining state needed for undo/save.
    public MechanicState Advance(MechanicState state, Deal deal, IReadOnlyList<bool> removed, Card? played)
    {
        var next = state with { Turns = state.Turns + 1, Clears = state.Clears + (played == null ? 0 : 1), HasKey = state.HasKey || played?.Kind == CardKind.Key, Keys = Math.Max(state.Keys, state.HasKey ? 1 : 0) + (played?.Kind == CardKind.Key ? 1 : 0), Run = played == null ? 0 : state.Run + 1, FuseDelay = state.FuseDelay + (state.ShieldSeconds > 0 ? 1 : 0) };
        return next with { Failed = deal.Cards.Where((c, i) => !removed[i]).Any(c => c.Kind == CardKind.Bomb && c.Strength + next.FuseDelay <= next.Turns) };
    }
    // Determine the extra-stock reward caused by this clear and its previous waste face.
    public int Bonus(Card played, Card previous) => played.Kind switch
    {
        CardKind.Gift => 3,
        CardKind.Up when previous.Rank != 0 && played.Rank == previous.Rank % 13 + 1 => 2,
        CardKind.Down when previous.Rank != 0 && played.Rank == (previous.Rank + 11) % 13 + 1 => 2,
        _ => 0
    };
}

public sealed class AdventureDealProvider : IDealProvider
{
    readonly IDealProvider source;
    readonly IDifficultyProgression progression;
    readonly string? generation;
    // Bind the base generator and mechanic policy used to decorate adventure deals.
    public AdventureDealProvider(IDealProvider source, IDifficultyProgression? progression = null,string? generation=null)
    { this.source = source; this.progression = progression ?? new CampaignProgression(); this.generation=generation; }
    // Add level-appropriate adventure obstacles to the underlying solvable deal.
    public Deal Generate(int seed, int level)
    {
        var deal = source.Generate(seed, level);
        var profile = progression.ForLevel(level);
        var cards = deal.Cards.ToArray();
        var solution = new List<Step>(); int run = 0, clears = 0, keys = 0, draws = 0, hazards = 0;
        var available = new[] { CardKind.Ice, CardKind.Lock, CardKind.Bomb, CardKind.Stone, CardKind.Vine }.Where(k => progression.Unlocked(k, level)).ToArray();
        Card previous = deal.Waste;
        foreach (var step in deal.Solution)
        {
            int i = step.Index;
            if (i < 0) { solution.Add(step); run = 0; previous = deal.Stock[draws++]; continue; }
            CardKind kind = CardKind.Normal; int strength = 0;
            if (progression.Unlocked(CardKind.Key, level) && keys < profile.KeysRequired && clears == 1 + keys*3)
            { kind = CardKind.Key; keys++; }
            else if (clears >= 3 && hazards < profile.HazardBudget && clears % 2 == 1 && available.Length > 0)
            {
                kind = hazards == 0 && available.Contains(profile.Focus) ? profile.Focus : available[(hazards + profile.Tier) % available.Length];
                if (kind == CardKind.Lock && keys < profile.KeysRequired || kind == CardKind.Vine && run < profile.VineRun) kind = CardKind.Ice;
                strength = kind switch {
                    CardKind.Ice => Math.Max(1, clears-profile.IceLead),
                    CardKind.Lock => profile.KeysRequired,
                    CardKind.Bomb => solution.Count + profile.BombSlack,
                    CardKind.Stone => profile.ArmorHits,
                    CardKind.Vine => profile.VineRun, _ => 0 };
                hazards++;
            }
            else if (progression.Unlocked(CardKind.Gate, level) && clears%13==10) { kind = CardKind.Gate; strength = solution.Count%2; }
            else if (progression.Unlocked(CardKind.ColorSeal, level) && clears%17==14) { kind = CardKind.ColorSeal; strength = previous.Suit is 1 or 2 ? 1 : 0; }
            else if (progression.Unlocked(CardKind.Lantern, level) && clears % 17 == 8) kind = CardKind.Lantern;
            else if (progression.Unlocked(CardKind.Lucky, level) && clears % 19 == 12) kind = CardKind.Lucky;
            else if (progression.Unlocked(CardKind.Shield, level) && clears % 17 == 10) kind = CardKind.Shield;
            else if (progression.Unlocked(CardKind.Gift, level) && clears % 11 == 2) kind = CardKind.Gift;
            else if (progression.Unlocked(CardKind.Up, level) && clears % 8 == 0) kind = cards[i].Rank == previous.Rank%13+1 ? CardKind.Up : CardKind.Down;
            cards[i] = cards[i] with { Kind = kind, Strength = strength };
            if (kind == CardKind.Stone)
                for (int hit = 1; hit < profile.ArmorHits; hit++) { solution.Add(new(i)); run = 0; }
            solution.Add(new(i)); run++;
            previous = cards[i]; clears++;
        }
        // Only remove reserve draws after the witness prefix; never its solution draws.
        int keep = Math.Max(draws, deal.Stock.Count-profile.ReserveTrim);
        bool story=deal.Layout.Any(s=>s.DrawX.HasValue);
        return new Deal { Generation = generation ?? (story ? "MP18" : "MP16"), Layout = deal.Layout, Cards = cards, Stock = deal.Stock.Take(keep).ToArray(), Waste = deal.Waste,
            Solution = solution, TemporaryRules = true, Bonus = level%25==0, OpeningEffect = level%25 != 0 ? "" : level >= 900 && level/25%3==0 ? "shelter" : level >= 700 && level/25%3==1 ? "harvest" : level >= 600 ? "glow" : "", Formation = generation=="MP19" ? StoryLayoutsV2.NameFor(level) : story ? StoryLayouts.NameFor(level) : AdventureLayouts.NameFor(level), Difficulty = profile };
    }
}
