using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AssemblyDetailViewModel : BaseViewModel
{
    private readonly AssemblyService _service;
    private readonly long _assemblyId;

    // Detail-derived snapshot (svi se osvezavaju u Load-u)
    private string _buildingLine     = string.Empty;
    private string _dateLine         = string.Empty;
    private string _timeLine         = string.Empty;
    private string _statusLine       = string.Empty;
    private string _editDeadlineLine = string.Empty;
    private bool   _hasSuggestions;
    private bool   _canEditTopics;
    private bool   _mustAddSuggestion;
    private string _errorMessage     = string.Empty;

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

    public string BuildingLine     => _buildingLine;
    public string DateLine         => _dateLine;
    public string TimeLine         => _timeLine;
    public string StatusLine       => _statusLine;
    public string EditDeadlineLine => _editDeadlineLine;

    public bool HasSuggestions    => _hasSuggestions;
    public bool CanEditTopics     => _canEditTopics;
    public bool CanNotEditTopics  => !_canEditTopics;
    public bool MustAddSuggestion => _mustAddSuggestion;
    public bool HasAttendances    => Attendances.Count > 0;
    public bool HasNoAttendances  => Attendances.Count == 0;

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
        AssemblyDetailDto? detail;
        try { detail = _service.GetDetail(_assemblyId); }
        catch { return; }
        if (detail == null) return;

        RefillCollection(Topics,      detail.Topics);
        RefillCollection(Suggestions, detail.Suggestions);
        RefillCollection(Attendances, detail.Attendances);
        UpdateSnapshot(detail);
        NotifyAllChanged();
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

    private void UpdateSnapshot(AssemblyDetailDto detail)
    {
        _buildingLine     = $"Zgrada: {detail.BuildingAddress}";
        _dateLine         = $"Datum: {detail.DateLine}";
        _timeLine         = $"Vreme: {detail.TimeLine}";
        _statusLine       = $"Status: {detail.StatusDisplay}";
        _editDeadlineLine = $"Rok za izmenu tema: {detail.EditDeadlineDisplay}";
        _hasSuggestions   = detail.HasSuggestions;
        _canEditTopics    = detail.CanEditTopics;
        _mustAddSuggestion = ComputeMustAddSuggestion();
    }

    private bool ComputeMustAddSuggestion()
    {
        if (!_hasSuggestions || !_canEditTopics) return false;
        foreach (AssemblyTopicDto t in Topics)
            if (t.IsAdditional) return false;
        return true;
    }

    private static void RefillCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (T item in source) target.Add(item);
    }

    private void NotifyAllChanged()
    {
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
}
