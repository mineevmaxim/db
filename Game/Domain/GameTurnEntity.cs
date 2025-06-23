using System;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Game.Domain;

public class GameTurnEntity
{
    [BsonId]
    public Guid TurnId { get; set; }
    public Guid GameId { get; set; }
    public Guid WinnerId { get; set; }
    public Player[] Players { get; set; }
    public int TurnIndex { get; set; }

    public GameTurnEntity(Guid gameId, Guid turnId, Guid winnerId, Player[] players, int turnIndex)
    {
        GameId = gameId;
        TurnId = turnId;
        WinnerId = winnerId;
        Players = players;
        TurnIndex = turnIndex;
    }

    public override string ToString()
    {
        var winnerName = "Tie";
        var potentialWinners = Players.Where(p => p.UserId == WinnerId).ToArray();
        if (potentialWinners.Length > 0)
            winnerName = potentialWinners.First().Name;
        var sb = new StringBuilder();
        sb.AppendLine("TurnId: " + TurnId);
        sb.AppendLine(" Turn number: " + (TurnIndex + 1));
        sb.AppendLine(" Winner: " + winnerName);
        return sb.ToString();
    }
}