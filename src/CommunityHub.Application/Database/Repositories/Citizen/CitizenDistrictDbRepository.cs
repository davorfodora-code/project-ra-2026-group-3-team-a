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

    public List<District> Search(DistrictSearchFilter filter)
    {
        return _districtRepository.Search(filter.Name, filter.Address, filter.City, filter.Country);
    }
}
