using CommunityHub.Application.Domain;
using CommunityHub.Application.Database.Repositories.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDistrictRepository
{
    List<District> GetAll();
    List<District> Search(DistrictSearchFilter filter);
}
