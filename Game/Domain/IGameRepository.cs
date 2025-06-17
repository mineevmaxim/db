using System;
using System.Collections.Generic;

namespace Game.Domain;

public interface IGameRepository
{
    GameEntity Insert(GameEntity game);
    GameEntity FindById(Guid gameId);
    void Update(GameEntity game);
    IList<GameEntity> FindWaitingToStart(int limit = 10);
    bool TryUpdateWaitingToStart(GameEntity game);
}