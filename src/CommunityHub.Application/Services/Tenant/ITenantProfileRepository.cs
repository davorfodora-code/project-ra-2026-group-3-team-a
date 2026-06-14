using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface ITenantProfileRepository
{
    List<TenantMembershipDto> GetMemberships(long tenantId);
}
