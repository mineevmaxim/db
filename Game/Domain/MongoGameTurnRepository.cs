using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain;

public class MongoGameTurnRepository : IGameTurnRepository
{
    public const string CollectionName = "game-turns";
    private readonly IMongoCollection<GameTurnEntity> gameTurnsCollection;

    public MongoGameTurnRepository(IMongoDatabase database)
    {
        var collection = database.GetCollection<GameTurnEntity>(CollectionName);
        gameTurnsCollection = collection;
    }

    public GameTurnEntity Add(GameTurnEntity gameTurn)
    {
        gameTurnsCollection.InsertOne(gameTurn);
        return gameTurn;
    }

    public GameTurnEntity GetById(Guid id)
    {
        return gameTurnsCollection
            .FindSync(g => g.TurnId == id)
            .FirstOrDefault();
    }

    public IList<GameTurnEntity> GetByGameId(Guid gameId)
    {
        return gameTurnsCollection
            .Find(g => g.GameId == gameId)
            .SortByDescending(gt => gt.TurnIndex)
            .Limit(5)
            .ToList()
            .OrderBy(gt => gt.TurnIndex)
            .ToList();
    }
}