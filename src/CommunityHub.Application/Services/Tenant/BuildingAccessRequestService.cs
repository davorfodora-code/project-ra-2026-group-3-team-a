using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class BuildingAccessRequestService
{
    private readonly IBuildingAccessRequestRepository _requestRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public BuildingAccessRequestService(
        IBuildingAccessRequestRepository requestRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _requestRepository = requestRepository;
        _membershipRepository = membershipRepository;
    }

    public List<TenantAccessRequestDto> GetAllByTenant(long tenantId)
    {
        return _requestRepository.GetAllByTenant(tenantId)
            .Select(MapToDto)
            .ToList();
    }

    public void Create(CreateBuildingAccessRequestDto dto)
    {
        _requestRepository.Create(dto.TenantId, dto.BuildingId, dto.ApartmentNumber);
    }

    public void Cancel(long requestId)
    {
        _requestRepository.Delete(requestId);
    }

    public bool IsApartmentOccupied(long buildingId, int apartmentNumber)
    {
        return _membershipRepository.IsApartmentOccupied(buildingId, apartmentNumber);
    }

    public bool HasPendingRequest(long tenantId, long buildingId)
    {
        return _requestRepository.ExistsPending(tenantId, buildingId);
    }

    private TenantAccessRequestDto MapToDto(BuildingAccessRequest r) => new TenantAccessRequestDto
    {
        RequestId = r.Id,
        AddressDisplay = $"{r.Building.Address.Street} {r.Building.Address.StreetNumber}",
        Neighborhood = r.Building.Neighborhood,
        LocationDisplay = $"{r.Building.Location.City}, {r.Building.Location.Country}",
        ApartmentNumber = r.ApartmentNumber,
        CreatedAt = r.CreatedAt,
        Status = r.Status,
        CanCancel = r.CanBeProcessed()
    };
}
