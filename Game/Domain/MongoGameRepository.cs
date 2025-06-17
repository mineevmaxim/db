using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace Game.Domain;

// TODO Сделать по аналогии с MongoUserRepository
public class MongoGameRepository : IGameRepository
{
    public const string CollectionName = "games";

    public MongoGameRepository(IMongoDatabase db)
    {
    }

    public GameEntity Insert(GameEntity game)
    {
        throw new NotImplementedException();
    }

    public GameEntity FindById(Guid gameId)
    {
        throw new NotImplementedException();
    }

    public void Update(GameEntity game)
    {
        throw new NotImplementedException();
    }

    // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
    public IList<GameEntity> FindWaitingToStart(int limit)
    {
        //TODO: Используй Find и Limit
        throw new NotImplementedException();
    }

    // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
    public bool TryUpdateWaitingToStart(GameEntity game)
    {
        //TODO: Для проверки успешности используй IsAcknowledged и ModifiedCount из результата
        throw new NotImplementedException();
    }
}