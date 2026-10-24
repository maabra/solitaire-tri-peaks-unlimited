using MeadowPeaks;
static class TimedMoves
{
    public static bool Draw(ISolitaireHand hand)
    {
        if(hand is ITemporaryHand timed && timed.Effects.DrawCooldown>0) timed.AdvanceTime(timed.Effects.DrawCooldown);
        return hand.Draw();
    }
    public static bool Draw(GameSession session)
    {
        if(session.Hand is ITemporaryHand timed && timed.Effects.DrawCooldown>0) session.AdvanceTime(timed.Effects.DrawCooldown);
        return session.Draw();
    }
}
