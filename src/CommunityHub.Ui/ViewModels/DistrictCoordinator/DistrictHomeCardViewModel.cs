using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class DistrictHomeCardViewModel : ViewModelBase
{
    private bool _isSelected;

    public DistrictHomeCardViewModel(DistrictHomeCardDto dto)
    {
        Dto = dto;
    }

    public DistrictHomeCardDto Dto { get; }

    public long DistrictId            => Dto.DistrictId;
    public string Name                => Dto.Name;
    public string LocationDisplay     => Dto.LocationDisplay;
    public string BudgetDisplay       => Dto.BudgetDisplay;
    public decimal Budget             => Dto.Budget;
    public int MembersCount           => Dto.MembersCount;
    public int StreetsCount           => Dto.StreetsCount;
    public int PendingRequestsCount   => Dto.PendingRequestsCount;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
