using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class DistrictAccessRequestService
{
    private readonly IDistrictAccessRequestRepository _repository;
    private readonly IDistrictRepository _districtRepository;

    public DistrictAccessRequestService(IDistrictAccessRequestRepository repository, IDistrictRepository districtRepository)
    {
        _repository = repository;
        _districtRepository = districtRepository;
    }

    public List<DistrictAccessRequestDto> GetByCoordinatorId(long coordinatorId)
    {
        return _repository.GetByCoordinatorId(coordinatorId)
            .Select(MapRequestToDto)
            .ToList();
    }

    public void Approve(long requestId)
    {
        _repository.Approve(requestId);
    }

    public void Reject(long requestId, string? reason)
    {
        _repository.Reject(requestId, reason);
    }

    public List<DistrictDto> GetDistrictsByCoordinatorId(long coordinatorId)
    {
        return _districtRepository.GetByCoordinatorId(coordinatorId)
            .Select(MapDistrictToDto)
            .ToList();
    }

    public List<DistrictMembershipDto> GetMembershipsByDistrictId(long districtId)
    {
        return _repository.GetMembershipsByDistrictId(districtId)
            .Select(MapMembershipToDto)
            .ToList();
    }

    private static DistrictAccessRequestDto MapRequestToDto(DistrictAccessRequest r) =>
        new DistrictAccessRequestDto
        {
            RequestId       = r.Id,
            CitizenFullName = $"{r.User.Name} {r.User.Surname}",
            CitizenAddress  = r.User.Address ?? "Nije navedena",
            DistrictName    = r.District.Name,
            Location        = $"{r.District.Location.City}, {r.District.Location.Country}",
            CreatedAt       = r.CreationDate,
            Status          = r.Status.ToString(),
            RejectionReason = r.RejectionReason
        };

    private static DistrictDto MapDistrictToDto(District d) => new DistrictDto
    {
        Id          = d.Id,
        Name        = d.Name,
        Description = d.Description,
        City        = d.Location.City,
        Country     = d.Location.Country,
        Budget      = d.Budget
    };

    private static DistrictMembershipDto MapMembershipToDto(DistrictMembership m) =>
        new DistrictMembershipDto
        {
            DistrictId      = m.District?.Id ?? 0,
            CitizenFullName = $"{m.User.Name} {m.User.Surname}",
            CitizenAddress  = m.User.Address ?? "Nije navedena",
            JoinDate        = m.JoinDate,
            TrustLevel      = m.TrustLevel.ToString()
        };
}
