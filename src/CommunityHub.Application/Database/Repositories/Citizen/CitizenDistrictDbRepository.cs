using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictDbRepository
{
    private readonly DistrictDbRepository _districtRepository;

    public CitizenDistrictDbRepository()
    {
        _districtRepository = new DistrictDbRepository();
    }

    public List<District> GetAll()
    {
        return _districtRepository.GetAll();
    }

    public List<District> Search(string? name, string? address, string? city, string? country)
    {
        return _districtRepository.Search(name, address, city, country);
    }
}
