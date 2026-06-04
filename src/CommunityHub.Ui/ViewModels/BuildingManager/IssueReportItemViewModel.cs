using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za jednu karticu prijave problema
public class IssueReportItemViewModel : BaseViewModel
{
    private readonly IssueReportDto _dto;

    public IssueReportItemViewModel(IssueReportDto dto) => _dto = dto;

    public long   Id               => _dto.Id;
    public string TenantFullName   => _dto.TenantFullName;
    public string BuildingAddress  => _dto.BuildingAddress;
    public string Description      => _dto.Description;
    public string DateLine         => _dto.DateLine;
    public string StatusDisplay    => _dto.StatusDisplay;
    public string Status           => _dto.Status;
    public bool   CanMarkAsResolved => _dto.CanMarkAsResolved;
}
