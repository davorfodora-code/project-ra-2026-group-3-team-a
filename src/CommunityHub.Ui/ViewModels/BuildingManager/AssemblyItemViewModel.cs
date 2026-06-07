using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za jednu karticu skupstine u listi
public class AssemblyItemViewModel : BaseViewModel
{
    private readonly AssemblySummaryDto _dto;

    public AssemblyItemViewModel(AssemblySummaryDto dto) => _dto = dto;

    public long   Id             => _dto.Id;
    public string BuildingLine   => $"Zgrada: {_dto.BuildingAddress}";
    public string DateTimeLine   => _dto.DateTimeLine;
    public string StatusLine     => $"Status: {_dto.StatusDisplay}";
    public string TopicCountLine => $"Broj tema: {_dto.TopicCount}";
    public string Status         => _dto.Status;
}
