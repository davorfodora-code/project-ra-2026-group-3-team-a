using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AssemblyDetailViewModel : BaseViewModel
{
    private readonly AssemblyService _service;
    private readonly long _assemblyId;
    private AssemblyDetailDto? _detail;

    public AssemblyDetailViewModel(long assemblyId)
    {
        _assemblyId = assemblyId;
        _service    = new AssemblyService(
            Injector.Get<IAssemblyRepository>(),
            Injector.Get<IBuildingRepository>());
        Topics      = new ObservableCollection<AssemblyTopicDto>();
        Attendances = new ObservableCollection<AttendanceItemDto>();
        Load();
    }

    public ObservableCollection<AssemblyTopicDto>  Topics      { get; }
    public ObservableCollection<AttendanceItemDto> Attendances { get; }

    public string BuildingLine => _detail != null ? $"Zgrada: {_detail.BuildingAddress}" : string.Empty;
    public string DateLine     => _detail != null ? $"Datum: {_detail.DateLine}"          : string.Empty;
    public string TimeLine     => _detail != null ? $"Vreme: {_detail.TimeLine}"          : string.Empty;
    public string StatusLine   => _detail != null ? $"Status: {_detail.StatusDisplay}"    : string.Empty;

    public bool HasAttendances   => Attendances.Count > 0;
    public bool HasNoAttendances => Attendances.Count == 0;

    public void Load()
    {
        try
        {
            _detail = _service.GetDetail(_assemblyId);
            if (_detail == null) return;

            Topics.Clear();
            foreach (var t in _detail.Topics)      Topics.Add(t);
            Attendances.Clear();
            foreach (var a in _detail.Attendances) Attendances.Add(a);

            OnPropertyChanged(nameof(BuildingLine));
            OnPropertyChanged(nameof(DateLine));
            OnPropertyChanged(nameof(TimeLine));
            OnPropertyChanged(nameof(StatusLine));
            OnPropertyChanged(nameof(HasAttendances));
            OnPropertyChanged(nameof(HasNoAttendances));
        }
        catch { /* prikazuje prazne kolekcije */ }
    }
}
