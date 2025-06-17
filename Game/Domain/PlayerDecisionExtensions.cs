namespace Game.Domain;

public static class PlayerDecisionExtensions
{
    public static bool Beats(this PlayerDecision currentDecision, PlayerDecision otherDecision) {
        var current = (int)currentDecision - 1;
        var other = (int)otherDecision - 1;
        return (current + 1)%3 == other;
    }
}