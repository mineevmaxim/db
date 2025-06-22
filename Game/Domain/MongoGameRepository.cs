using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain;

// TODO Сделать по аналогии с MongoUserRepository
public class MongoGameRepository : IGameRepository
{
    public const string CollectionName = "games";
    private readonly IMongoCollection<GameEntity> gamesCollection;

    public MongoGameRepository(IMongoDatabase db)
    {
        gamesCollection = db.GetCollection<GameEntity>(CollectionName);
    }

    public GameEntity Insert(GameEntity game)
    {
        gamesCollection.InsertOne(game);
        return game;
    }

    public GameEntity FindById(Guid gameId)
    {
        return gamesCollection.Find(x => x.Id == gameId).FirstOrDefault();
    }

    public void Update(GameEntity game)
    {
        gamesCollection.ReplaceOne(x => x.Id == game.Id, game);
    }

    // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
    public IList<GameEntity> FindWaitingToStart(int limit)
    {
        return gamesCollection
            .Find(g => g.Status == GameStatus.WaitingToStart)
            .Limit(limit)
            .ToList();
    }

    // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
    public bool TryUpdateWaitingToStart(GameEntity game)
    {
        var currentGame = FindById(game.Id);
        if (currentGame == null)
            return false;
        
        if (currentGame.Status != GameStatus.WaitingToStart)
            return false;

        var updateResult = gamesCollection.ReplaceOne(x => x.Id == game.Id, game);
        return updateResult.IsAcknowledged || updateResult.ModifiedCount > 0;
    }
}