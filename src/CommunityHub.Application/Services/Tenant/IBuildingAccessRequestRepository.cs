using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Tenant;

public interface IBuildingAccessRequestRepository
{
    bool ExistsPending(long tenantId, long buildingId);
    long Create(long tenantId, long buildingId, int apartmentNumber);
    List<BuildingAccessRequest> GetAllByTenant(long tenantId);
    void Delete(long requestId);
    List<int> GetApartmentNumbers(long buildingId);
}
