using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDonationService
{
    IReadOnlyList<DonationCategoryDto> GetCategories();
    DonationDashboardDto GetMyDistrictDonations(long citizenId);
    DonationActionResultDto CreateDonation(long citizenId, CreateDonationDto dto);
}
