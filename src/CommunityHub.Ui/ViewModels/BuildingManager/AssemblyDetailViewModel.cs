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
        Suggestions = new ObservableCollection<TopicSuggestionDto>();
        Attendances = new ObservableCollection<AttendanceItemDto>();
        Load();
    }

    public ObservableCollection<AssemblyTopicDto>   Topics      { get; }
    public ObservableCollection<TopicSuggestionDto> Suggestions { get; }
    public ObservableCollection<AttendanceItemDto>  Attendances { get; }

    public string BuildingLine     => _detail != null ? $"Zgrada: {_detail.BuildingAddress}"                  : string.Empty;
    public string DateLine         => _detail != null ? $"Datum: {_detail.DateLine}"                          : string.Empty;
    public string TimeLine         => _detail != null ? $"Vreme: {_detail.TimeLine}"                          : string.Empty;
    public string StatusLine       => _detail != null ? $"Status: {_detail.StatusDisplay}"                    : string.Empty;
    public string EditDeadlineLine => _detail != null ? $"Rok za izmenu tema: {_detail.EditDeadlineDisplay}"  : string.Empty;

    public bool HasSuggestions   => (_detail?.HasSuggestions) == true;
    public bool CanEditTopics    => (_detail?.CanEditTopics)  == true;
    // Upozorenje se prikazuje samo kad ima predloga I nijedan nije jos dodat
    public bool CanNotEditTopics  => !CanEditTopics;
    public bool MustAddSuggestion => HasSuggestions && CanEditTopics && !Topics.Any(t => t.IsAdditional);
    public bool HasAttendances   => Attendances.Count > 0;
    public bool HasNoAttendances => Attendances.Count == 0;

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public void Load()
    {
        try
        {
            _detail = _service.GetDetail(_assemblyId);
            if (_detail == null) return;

            Topics.Clear();
            foreach (var t in _detail.Topics)      Topics.Add(t);
            Suggestions.Clear();
            foreach (var s in _detail.Suggestions) Suggestions.Add(s);
            Attendances.Clear();
            foreach (var a in _detail.Attendances) Attendances.Add(a);

            OnPropertyChanged(nameof(BuildingLine));
            OnPropertyChanged(nameof(DateLine));
            OnPropertyChanged(nameof(TimeLine));
            OnPropertyChanged(nameof(StatusLine));
            OnPropertyChanged(nameof(EditDeadlineLine));
            OnPropertyChanged(nameof(HasSuggestions));
            OnPropertyChanged(nameof(CanEditTopics));
            OnPropertyChanged(nameof(CanNotEditTopics));
            OnPropertyChanged(nameof(MustAddSuggestion));
            OnPropertyChanged(nameof(HasAttendances));
            OnPropertyChanged(nameof(HasNoAttendances));
        }
        catch { /* prikazuje prazne kolekcije */ }
    }

    // Dodaje predlozenu temu kao dodatnu i uklanja je iz predloga (1.7)
    public void AddSuggestedTopic(TopicSuggestionDto suggestion)
    {
        ErrorMessage = string.Empty;
        try
        {
            _service.AddSuggestedTopic(_assemblyId, suggestion.Id, suggestion.Topic);
            Load();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }
}
