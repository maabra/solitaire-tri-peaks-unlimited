using MeadowPeaks;

static class AdventureTests
{
    public static void Run(Action<bool, string> check)
    {
        var rules = new TriPeaksRules();
        var provider = new AdventureDealProvider(new SeededDealProvider(new AdventureLayouts(), rules));
        var factory = new SolitaireFactory(provider, rules, new SeededDealProvider(new PeakLayouts(), rules));
        var patterns = new HashSet<string>();
        var layouts = new AdventureLayouts();
        for (int n = 5; n < 5 + AdventureLayouts.PatternCount; n++)
        {
            var board = layouts.Create(n);
            check(patterns.Add(AdventureLayouts.Fingerprint(board)), "Formation is distinct even after mirroring or shifting");
            check(board.SequenceEqual(layouts.Create(n)), "Formation is reproducible");
            check(board.Count >= 18 && board.Count <= 30 && board.All(s => s.X >= .5f && s.X <= 10 && s.Row >= 0 && s.Row < 4), "Formation fits board and deck");
            check(board.Distinct().Count() == board.Count, "No duplicate slots");
            foreach (var row in board.GroupBy(s => s.Row))
                check(row.OrderBy(s => s.X).Zip(row.OrderBy(s => s.X).Skip(1), (a,b) => b.X - a.X).All(d => d >= 1), "Face-up cards do not overlap within a row");
        }
        var kinds = new HashSet<CardKind>(); var formations = new HashSet<string>();
        for (int level = 1; level <= 4000; level++)
        {
            var hand = factory.Create(104729 + level * 997, level);
            formations.Add(hand.Deal.Formation);
            foreach (var card in hand.Deal.Cards) kinds.Add(card.Kind);
            foreach (var step in hand.Deal.Solution)
                check(step.Index < 0 ? hand.Draw() : hand.Play(step.Index), $"Adventure witness {level}/{step.Index}");
            check(hand.Won && !((IAdventureHand)hand).Failed, "Adventure solved without helpers");
            var restored = factory.Restore(hand.Capture(level, level, false));
            check(restored?.Won == true, "Adventure save restores");
        }
        for (int n = 4001; n <= 4100; n++)
        {
            var endless = factory.Create(170003 + n * 7919, n);
            foreach (var step in endless.Deal.Solution)
                check(step.Index < 0 ? endless.Draw() : endless.Play(step.Index), "Endless formation has a winning path");
            check(endless.Won, "Endless formation solved");
        }
        check(kinds.Count == 8 && formations.Count == 3997, "Every special card and formation generated");
        Deal Board(params Card[] cards) => new() { Cards = cards, Layout = cards.Select((_, i) => new Slot(i * 2, 0)).ToArray(), Waste = new(5, 0), Stock = new[] { new Card(7, 1), new Card(8, 1) } };
        var ice = new Hand(Board(new Card(6, 0) { Kind = CardKind.Ice, Strength = 1 }, new Card(6, 1)), rules);
        check(!ice.Play(0) && ice.Play(1) && ((IAdventureHand)ice).CardStatus(0) == "THAWED", "Ice blocks and thaws on clears");
        check(ice.Undo() && !ice.CanPlay(0), "Undo restores ice");
        var locks = new Hand(Board(new Card(6, 0) { Kind = CardKind.Lock }, new Card(6, 1) { Kind = CardKind.Key }), rules);
        check(!locks.CanPlay(0) && locks.Play(1) && ((IAdventureHand)locks).CardStatus(0) == "UNLOCKED", "Key opens lock");
        check(locks.Undo() && !locks.CanPlay(0), "Undo restores key state");
        var bomb = new Hand(Board(new Card(6, 0) { Kind = CardKind.Bomb, Strength = 1 }, new Card(9, 1)), rules);
        check(bomb.Draw() && bomb.Failed && !bomb.Draw() && !bomb.Play(0), "Bomb expires and stops hand");
        check(bomb.Undo() && !bomb.Failed && bomb.Play(0), "Undo rescues bomb and clearing defuses it");
        var gift = new Hand(Board(new Card(6, 0) { Kind = CardKind.Up }, new Card(7, 0) { Kind = CardKind.Gift }), rules);
        check(gift.Play(0) && gift.Stock.Count == 4 && gift.Stock[0].Rank == 7, "Up card appends stock without replacing next draw");
        var copy = factory.Restore(gift.Capture(1, 50, false));
        check(copy?.Stock.Count == 4 && ((IAdventureHand)copy).CardStatus(1) == "+3 CARDS", "Special state saves midhand");
        check(gift.Undo() && gift.Stock.Count == 2 && gift.Play(0) && gift.Stock.Count == 4, "Bonus stock undo prevents duplication");
        check(gift.Play(1) && gift.Stock.Count == 7, "Gift adds three cards");
        var policy = new CardMechanics();
        check(policy.Bonus(new Card(1, 0) { Kind = CardKind.Up }, new Card(13, 0)) == 2 && policy.Bonus(new Card(13, 0) { Kind = CardKind.Down }, new Card(1, 0)) == 2, "Directional rewards wrap ace and king");
    }
}
