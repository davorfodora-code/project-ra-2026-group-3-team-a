using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class MyDistrictViewModel
{
    private readonly long _userId;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;
    private readonly ICitizenDistrictService _districtService;
    private readonly ICitizenTrustRecordService _trustRecordService;

    public MyDistrictViewModel(long userId)
    {
        _userId = userId;
        _membershipRepository = Injector.Get<ICitizenDistrictMembershipRepository>();
        _districtService = new CitizenDistrictService();
        _trustRecordService = Injector.Get<ICitizenTrustRecordService>();
    }

    public DistrictListItemDto? GetMyDistrict()
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(_userId);
        return districtId == null ? null : _districtService.GetById(districtId.Value);
    }

    public CitizenTrustRecordDto? GetMyTrustRecord()
    {
        return _trustRecordService.GetMyTrustRecord(_userId);
    }

    public List<MyDistrictTrustRowViewModel> GetDistrictTrustRows()
    {
        return _trustRecordService.GetDistrictTrustRecords(_userId)
            .OrderBy(r => r.CitizenFullName)
            .Select(r => new MyDistrictTrustRowViewModel
            {
                CitizenFullName = r.CitizenFullName,
                JoinDate = r.JoinDate.ToString("dd.MM.yyyy."),
                OrganizedCompletedEventsCount = r.OrganizedCompletedEventsCount,
                ConfirmedVolunteerEventsCount = r.ConfirmedVolunteerEventsCount,
                TrustLevel = FormatTrustLevel(r.TrustLevel)
            })
            .ToList();
    }

    public static string FormatTrustLevel(string level)
    {
        return level switch
        {
            "Nov" => "Nov u kvartu",
            "Neaktivan" => "Neaktivan građanin",
            "Aktivan" => "Aktivan građanin",
            "Istaknuti" => "Istaknut građanin",
            "OdPoverenja" => "Građanin od poverenja",
            _ => level
        };
    }
}

public class MyDistrictTrustRowViewModel
{
    public string CitizenFullName { get; set; } = string.Empty;
    public string JoinDate { get; set; } = string.Empty;
    public int OrganizedCompletedEventsCount { get; set; }
    public int ConfirmedVolunteerEventsCount { get; set; }
    public string TrustLevel { get; set; } = string.Empty;
}
