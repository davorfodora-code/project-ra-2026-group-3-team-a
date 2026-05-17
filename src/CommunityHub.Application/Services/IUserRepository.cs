using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services;

public interface IUserRepository
{
    (long Id, string Role)? GetByCredentials(string username, string password);
    User? GetById(long userId);
    User? GetWithPosts(long userId);
}
