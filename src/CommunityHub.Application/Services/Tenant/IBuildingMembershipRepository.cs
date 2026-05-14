namespace CommunityHub.Application.Services.Tenant;

public interface IBuildingMembershipRepository
{
    bool IsApartmentOccupied(long buildingId, int apartmentNumber);
}
