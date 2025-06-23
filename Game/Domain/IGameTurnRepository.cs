using System;
using System.Collections.Generic;

namespace Game.Domain;

public interface IGameTurnRepository
{
    // TODO: Спроектировать интерфейс исходя из потребностей ConsoleApp
    public GameTurnEntity Add(GameTurnEntity gameTurn);
    public GameTurnEntity GetById(Guid id);
    public IList<GameTurnEntity> GetByGameId(Guid gameId);
}