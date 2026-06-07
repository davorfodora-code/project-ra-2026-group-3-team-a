using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDonationRepository
{
    DonationDashboardDto GetDashboard(long districtId, long citizenId, IReadOnlyCollection<string> categories);
    void CreateDonation(long districtId, long citizenId, decimal amount, string category, DateTime donatedAt);
}
