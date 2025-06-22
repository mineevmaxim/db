using System;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserEntity> userCollection;
    public const string CollectionName = "users";

    public MongoUserRepository(IMongoDatabase database)
    {
        userCollection = database.GetCollection<UserEntity>(CollectionName);
        userCollection.Indexes.CreateOne(Builders<UserEntity>.IndexKeys.Ascending("Login"), new CreateIndexOptions() { Unique = true });
    }

    public UserEntity Insert(UserEntity user)
    {
        userCollection.InsertOne(user);
        return user;
    }

    public UserEntity FindById(Guid id)
    {
        return userCollection.Find(user => user.Id == id).FirstOrDefault();
    }

    public UserEntity GetOrCreateByLogin(string login)
    {
        var cursor = userCollection.Find(user => user.Login == login);
        if (cursor.Any())
            return cursor.First();
        var user = new UserEntity(
            Guid.NewGuid(),
            login,
            null,
            null,
            0,
            null
        );
        userCollection.InsertOne(user);
        return user;
    }

    public void Update(UserEntity user)
    {
        userCollection.ReplaceOne(u => u.Id == user.Id, user);
    }

    public void Delete(Guid id)
    {
        userCollection.DeleteOne(u => u.Id == id);
    }

    public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
    {
        var allUsers = userCollection.AsQueryable();
        var count = allUsers.LongCount();
        var users = allUsers
            .OrderBy(u => u.Login)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PageList<UserEntity>(users, count, pageNumber, pageSize);
    }

    // Не нужно реализовывать этот метод
    public void UpdateOrInsert(UserEntity user, out bool isInserted)
    {
        throw new NotImplementedException();
    }
}