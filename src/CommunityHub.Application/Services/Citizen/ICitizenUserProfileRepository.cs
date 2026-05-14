using System.Collections.Generic;

namespace CommunityHub.Application.Services.Citizen
{
    public interface ICitizenUserProfileRepository
    {
        string? GetAddressByUserId(long userId);
    }
}
