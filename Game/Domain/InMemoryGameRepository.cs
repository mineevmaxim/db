using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain;

public class InMemoryGameRepository : IGameRepository
{
    private readonly Dictionary<Guid, GameEntity> entities = new Dictionary<Guid, GameEntity>();

    public GameEntity Insert(GameEntity game)
    {
        if (game.Id != Guid.Empty)
            throw new Exception();

        var id = Guid.NewGuid();
        var entity = Clone(id, game);
        entities[id] = entity;
        return Clone(id, entity);
    }

    public GameEntity FindById(Guid id)
    {
        return entities.TryGetValue(id, out var entity) ? Clone(id, entity) : null;
    }

    public void Update(GameEntity game)
    {
        if (!entities.ContainsKey(game.Id))
            return;

        entities[game.Id] = Clone(game.Id, game);
    }

    public IList<GameEntity> FindWaitingToStart(int limit)
    {
        return entities
            .Select(pair => pair.Value)
            .Where(e => e.Status == GameStatus.WaitingToStart)
            .Take(limit)
            .Select(e => Clone(e.Id, e))
            .ToArray();
    }

    public bool TryUpdateWaitingToStart(GameEntity game)
    {
        if (!entities.TryGetValue(game.Id, out var savedGame)
            || savedGame.Status != GameStatus.WaitingToStart)
            return false;

        entities[game.Id] = Clone(game.Id, game);
        return true;
    }

    private GameEntity Clone(Guid id, GameEntity game)
    {
        return new GameEntity(id, game.Status, game.TurnsCount, game.CurrentTurnIndex, game.Players.ToList());
    }
}