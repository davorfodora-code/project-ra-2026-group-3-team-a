using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CitizenDonationsViewModel
{
    private readonly long _citizenId;
    private readonly ICitizenDonationService _service;

    public CitizenDonationsViewModel(long citizenId)
    {
        _citizenId = citizenId;
        _service = Injector.Get<ICitizenDonationService>();
    }

    public IReadOnlyList<DonationCategoryDto> Categories => _service.GetCategories();

    public DonationDashboardDto LoadDashboard()
    {
        return _service.GetMyDistrictDonations(_citizenId);
    }

    public DonationActionResultDto CreateDonation(decimal amount, string category)
    {
        return _service.CreateDonation(_citizenId, new CreateDonationDto
        {
            Amount = amount,
            Category = category
        });
    }
}
