using MeadowPeaks;
static class TimedMoves
{
    // Advance the timed-hand draw cooldown before issuing the draw used by this deterministic test.
    public static bool Draw(ISolitaireHand hand)
    {
        if(hand is ITemporaryHand timed && timed.Effects.DrawCooldown>0) timed.AdvanceTime(timed.Effects.DrawCooldown);
        return hand.Draw();
    }
    // Advance the timed-hand draw cooldown before issuing the draw used by this deterministic test.
    public static bool Draw(GameSession session)
    {
        if(session.Hand is ITemporaryHand timed && timed.Effects.DrawCooldown>0) session.AdvanceTime(timed.Effects.DrawCooldown);
        return session.Draw();
    }
}
