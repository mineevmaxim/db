using System;
using System.Linq;
using Game.Domain;
using Tests;

namespace ConsoleApp;

class Program
{
    private readonly IUserRepository userRepo;
    private readonly IGameRepository gameRepo;
    private readonly IGameTurnRepository gameTurnRepo;
    private readonly Random random = new Random();

    private Program(string[] args)
    {
        var db = TestMongoDatabase.Create();
        userRepo = new MongoUserRepository(db);
        gameRepo = new MongoGameRepository(db);
        gameTurnRepo = new MongoGameTurnRepository(db);
    }

    public static void Main(string[] args)
    {
        new Program(args).RunMenuLoop();
    }

    private void RunMenuLoop()
    {
        var humanUser = userRepo.GetOrCreateByLogin("Human");
        var aiUser = userRepo.GetOrCreateByLogin("AI");
        var game = FindCurrentGame(humanUser) ?? StartNewGame(humanUser);
        if (!TryJoinToGame(game, aiUser))
        {
            Console.WriteLine("Can't add AI user to the game");
            return;
        }

        while (HandleOneGameTurn(humanUser.Id))
        {
        }

        Console.WriteLine("Game is finished");
        Console.ReadLine();
    }

    private GameEntity StartNewGame(UserEntity user)
    {
        Console.WriteLine("Enter desired number of turns in game:");
        if (!int.TryParse(Console.ReadLine(), out var turnsCount))
        {
            turnsCount = 5;
            Console.WriteLine($"Bad input. Use default value for turns count: {turnsCount}");
        }

        var game = new GameEntity(turnsCount);
        game.AddPlayer(user);
        var savedGame = gameRepo.Insert(game);

        user.CurrentGameId = savedGame.Id;
        userRepo.Update(user);

        return savedGame;
    }

    private bool TryJoinToGame(GameEntity game, UserEntity user)
    {
        if (IsUserInGame(user, game))
            return true;

        if (user.CurrentGameId.HasValue)
            return false;

        if (game.Status != GameStatus.WaitingToStart)
            return false;

        game.AddPlayer(user);
        if (!gameRepo.TryUpdateWaitingToStart(game))
            return false;

        user.CurrentGameId = game.Id;
        userRepo.Update(user);

        return true;
    }

    private static bool IsUserInGame(UserEntity user, GameEntity game)
    {
        return user.CurrentGameId.HasValue
               && user.CurrentGameId.Value == game.Id
               && game.Players.Any(p => p.UserId == user.Id);
    }

    private GameEntity FindCurrentGame(UserEntity humanUser)
    {
        if (humanUser.CurrentGameId == null) return null;
        var game = gameRepo.FindById(humanUser.CurrentGameId.Value);
        if (game == null) return null;
        return game.Status switch
        {
            GameStatus.WaitingToStart or GameStatus.Playing => game,
            GameStatus.Finished or GameStatus.Canceled => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool HandleOneGameTurn(Guid humanUserId)
    {
        var game = GetGameByUser(humanUserId);

        if (game.IsFinished())
        {
            UpdatePlayersWhenGameFinished(game);
            return false;
        }

        var decision = AskHumanDecision();
        if (!decision.HasValue)
            return false;
        game.SetPlayerDecision(humanUserId, decision.Value);

        var aiPlayer = game.Players.First(p => p.UserId != humanUserId);
        var aiDecision = GetAiDecision();
        game.SetPlayerDecision(aiPlayer.UserId, aiDecision);

        if (game.HaveDecisionOfEveryPlayer)
        {
            // TODO: Сохранить информацию о прошедшем туре в IGameTurnRepository. Сформировать информацию о закончившемся туре внутри FinishTurn и вернуть её сюда.
            var turn = game.FinishTurn();
            gameTurnRepo.Add(turn);
        }

        ShowScore(game);
        gameRepo.Update(game);
        return true;
    }

    private GameEntity GetGameByUser(Guid userId)
    {
        var user = userRepo.FindById(userId) ?? throw new Exception($"Unknown user with id {userId}");
        var userCurrentGameId = user.CurrentGameId ?? throw new Exception($"No current game for user: {user}");
        return gameRepo.FindById(userCurrentGameId);
    }

    private PlayerDecision GetAiDecision()
    {
        return (PlayerDecision)Math.Min(3, 1 + random.Next(4));
    }

    private void UpdatePlayersWhenGameFinished(GameEntity game)
    {
        // Вместо этого кода можно написать специализированный метод в userRepo, который сделает все эти обновления за одну операцию UpdateMany.
        // Вместо 4 запросов к БД будет 1, но усложнится репозиторий. В данном случае, это редкая операция, поэтому нет смысла оптимизировать.
        foreach (var player in game.Players)
        {
            var playerUser = userRepo.FindById(player.UserId);
            if (playerUser == null) continue;
            playerUser.ExitGame();
            userRepo.Update(playerUser);
        }
    }

    private static PlayerDecision? AskHumanDecision()
    {
        Console.WriteLine();
        Console.WriteLine("Select your next decision:");
        Console.WriteLine("1 — Rock");
        Console.WriteLine("2 — Scissors");
        Console.WriteLine("3 — Paper");

        while (true)
        {
            var key = Console.ReadKey(true);
            switch (key.KeyChar)
            {
                case '1':
                    return PlayerDecision.Rock;
                case '2':
                    return PlayerDecision.Scissors;
                case '3':
                    return PlayerDecision.Paper;
            }

            if (key.Key == ConsoleKey.Escape) return null;
        }
    }

    private void ShowScore(GameEntity game)
    {
        var players = game.Players;
        // TODO: Показать информацию про 5 последних туров: кто как ходил и кто в итоге выиграл. Прочитать эту информацию из IGameTurnRepository
        var turns = gameTurnRepo.GetByGameId(game.Id);
        foreach (var turn in turns)
            Console.WriteLine(turn);
        
        Console.WriteLine($"Score: {players[0].Name} {players[0].Score} : {players[1].Score} {players[1].Name}");
    }
}