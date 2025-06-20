using System;
using JetBrains.Annotations;

namespace Game.Domain;

public interface IUserRepository
{
    UserEntity Insert(UserEntity user);
    [CanBeNull]
    UserEntity FindById(Guid id);
    [NotNull]
    UserEntity GetOrCreateByLogin(string login);
    void Update(UserEntity user);
    void UpdateOrInsert(UserEntity user, out bool isInserted);
    void Delete(Guid id);
    PageList<UserEntity> GetPage(int pageNumber, int pageSize);
}