using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenDonationService : ICitizenDonationService
{
    private static readonly string[] Categories =
    [
        "Uredjenje ulica",
        "Uredjenje parkova",
        "Rasveta",
        "Cistoca",
        "Zelenilo",
        "Hitne popravke"
    ];

    private readonly ICitizenDonationRepository _donationRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;

    public CitizenDonationService(
        ICitizenDonationRepository donationRepository,
        ICitizenDistrictMembershipRepository membershipRepository)
    {
        _donationRepository = donationRepository;
        _membershipRepository = membershipRepository;
    }

    public IReadOnlyList<DonationCategoryDto> GetCategories()
    {
        return Categories
            .Select(category => new DonationCategoryDto { Name = category })
            .ToList();
    }

    public DonationDashboardDto GetMyDistrictDonations(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return new DonationDashboardDto();
        }

        return _donationRepository.GetDashboard(districtId.Value, citizenId, Categories);
    }

    public DonationActionResultDto CreateDonation(long citizenId, CreateDonationDto dto)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return DonationActionResultDto.Fail("Niste clan nijednog kvarta.");
        }

        if (dto.Amount <= 0)
        {
            return DonationActionResultDto.Fail("Iznos donacije mora biti veci od 0.");
        }

        if (!Categories.Contains(dto.Category))
        {
            return DonationActionResultDto.Fail("Izaberite ispravnu kategoriju donacije.");
        }

        _donationRepository.CreateDonation(districtId.Value, citizenId, dto.Amount, dto.Category, DateTime.Now);
        return DonationActionResultDto.Ok("Donacija je uspesno evidentirana.");
    }
}
