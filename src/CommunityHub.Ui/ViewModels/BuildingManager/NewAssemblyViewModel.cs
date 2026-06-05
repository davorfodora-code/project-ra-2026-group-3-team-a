using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class NewAssemblyViewModel : BaseViewModel
{
    private readonly AssemblyService              _assemblyService;
    private readonly BuildingAccessRequestService _buildingService;
    private readonly long _managerId;

    private BuildingDto? _selectedBuilding;
    private DateTime?    _selectedDate;
    private string       _timeText     = string.Empty;
    private string       _newTopicText = string.Empty;
    private string       _errorMessage = string.Empty;

    public NewAssemblyViewModel(long managerId)
    {
        _managerId       = managerId;
        _assemblyService = new AssemblyService(
            Injector.Get<IAssemblyRepository>(),
            Injector.Get<IBuildingRepository>());
        _buildingService = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>());

        Buildings = new ObservableCollection<BuildingDto>(
            _buildingService.GetBuildingsByManagerId(managerId));
        Topics = new ObservableCollection<TopicEntryViewModel>();
    }

    public ObservableCollection<BuildingDto>       Buildings { get; }
    public ObservableCollection<TopicEntryViewModel> Topics  { get; }

    public BuildingDto? SelectedBuilding
    {
        get => _selectedBuilding;
        set => SetProperty(ref _selectedBuilding, value);
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public string TimeText
    {
        get => _timeText;
        set => SetProperty(ref _timeText, value);
    }

    public string NewTopicText
    {
        get => _newTopicText;
        set => SetProperty(ref _newTopicText, value);
    }

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

    public void AddTopic()
    {
        if (string.IsNullOrWhiteSpace(NewTopicText)) return;
        Topics.Add(new TopicEntryViewModel(NewTopicText.Trim()));
        NewTopicText = string.Empty;
    }

    public void RemoveTopic(TopicEntryViewModel topic) => Topics.Remove(topic);

    public bool TrySubmit()
    {
        ErrorMessage = string.Empty;
        string? err = Validate();
        if (err != null) { ErrorMessage = err; return false; }
        try
        {
            _assemblyService.Create(BuildDto());
            return true;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; return false; }
    }

    private string? Validate()
    {
        if (_selectedBuilding == null)     return "Odaberite zgradu.";
        if (_selectedDate == null)         return "Odaberite datum skupštine.";
        if (_selectedDate.Value.Date < DateTime.Today)
                                           return "Datum skupštine mora biti u budućnosti.";
        if (!TimeSpan.TryParse(_timeText, out _))
                                           return "Unesite vreme u formatu HH:mm (npr. 18:30).";
        if (Topics.Count == 0)             return "Dodajte bar jednu temu.";
        return null;
    }

    private CreateAssemblyDto BuildDto()
    {
        TimeSpan.TryParse(_timeText, out TimeSpan time);
        return new CreateAssemblyDto
        {
            BuildingId  = _selectedBuilding!.Id,
            ScheduledAt = _selectedDate!.Value.Date + time,
            Topics      = Topics.Select(t => t.Text).ToList()
        };
    }
}
