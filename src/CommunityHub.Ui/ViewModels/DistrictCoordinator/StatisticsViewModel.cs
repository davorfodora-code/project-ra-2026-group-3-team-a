using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class StatisticsViewModel : ViewModelBase
{
    private readonly DistrictStatisticsService _statsService;
    private readonly DistrictMeetingService _meetingsService;
    private readonly long _coordinatorId;

    private List<MeetingDto> _districtMeetings = new();
    private CitizenTrustStatisticDto? _selectedDistrict;
    private string _activeFilter = "all";

    private int _novCount, _neaktivanCount, _aktivanCount, _istaknutiCount, _odPoverenjaCount;
    private string _recommendationText = "Odaberite kvart da biste videli preporuku.";

    public StatisticsViewModel(long coordinatorId)
    {
        _coordinatorId    = coordinatorId;
        _statsService     = new DistrictStatisticsService();
        _meetingsService  = new DistrictMeetingService();

        Districts        = new ObservableCollection<CitizenTrustStatisticDto>();
        FilteredMeetings = new ObservableCollection<MeetingDto>();

        SetFilterCommand       = new RelayCommand(p => ActiveFilter = p?.ToString() ?? "all");
        ScheduleMeetingCommand = new RelayCommand(_ => RequestScheduleMeetingDialog());
    }

    public ObservableCollection<CitizenTrustStatisticDto> Districts        { get; }
    public ObservableCollection<MeetingDto>               FilteredMeetings { get; }

    public CitizenTrustStatisticDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value))
                OnSelectedDistrictChanged();
        }
    }

    public int NovCount         { get => _novCount;         private set => SetProperty(ref _novCount, value); }
    public int NeaktivanCount   { get => _neaktivanCount;   private set => SetProperty(ref _neaktivanCount, value); }
    public int AktivanCount     { get => _aktivanCount;     private set => SetProperty(ref _aktivanCount, value); }
    public int IstaknutiCount   { get => _istaknutiCount;   private set => SetProperty(ref _istaknutiCount, value); }
    public int OdPoverenjaCount { get => _odPoverenjaCount; private set => SetProperty(ref _odPoverenjaCount, value); }

    public string RecommendationText
    {
        get => _recommendationText;
        private set => SetProperty(ref _recommendationText, value);
    }

    public string ActiveFilter
    {
        get => _activeFilter;
        set
        {
            if (SetProperty(ref _activeFilter, value))
                ApplyFilter();
        }
    }

    public ICommand SetFilterCommand       { get; }
    public ICommand ScheduleMeetingCommand { get; }

    public event Action<long, MeetingType?, Action<bool>>? ScheduleMeetingDialogRequested;
    public event Action<MeetingDto, List<DateTime>, Action<DateTime?>>? ResolveTieDialogRequested;
    public event Action<string, string, bool>? NotificationRequested; // (poruka, naslov, jeLiInfo)

    public void Initialize()
    {
        try
        {
            LoadAll();
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke(
                $"Greška pri učitavanju podataka:\n{ex.Message}\n\n" +
                "Proverite da li je migracija baze pokrenuta (migration_district_coordinator_33.sql).",
                "Greška", false);
        }
    }

    private void LoadAll()
    {
        List<CitizenTrustStatisticDto> stats = _statsService.GetStatisticsByCoordinatorId(_coordinatorId);
        Districts.Clear();
        foreach (var s in stats) Districts.Add(s);

        if (Districts.Count > 0)
            SelectedDistrict = Districts[0];
        else
            ResetSelection();
    }

    private void OnSelectedDistrictChanged()
    {
        if (_selectedDistrict == null)
        {
            ResetSelection();
            return;
        }

        NovCount         = _selectedDistrict.NovCount;
        NeaktivanCount   = _selectedDistrict.NeaktivanCount;
        AktivanCount     = _selectedDistrict.AktivanCount;
        IstaknutiCount   = _selectedDistrict.IstaknutiCount;
        OdPoverenjaCount = _selectedDistrict.OdPoverenjaCount;
        RecommendationText = _selectedDistrict.RecommendationDisplay;

        _districtMeetings = _meetingsService
            .GetAllByCoordinatorId(_coordinatorId)
            .Where(m => m.DistrictId == _selectedDistrict.DistrictId)
            .OrderByDescending(m => m.DateRangeStart)
            .ToList();

        ActiveFilter = "all"; // reset filtera
        ApplyFilter();        // (setter Promjena će triggerati, ali za svaki slučaj)
    }

    private void ResetSelection()
    {
        NovCount         = 0;
        NeaktivanCount   = 0;
        AktivanCount     = 0;
        IstaknutiCount   = 0;
        OdPoverenjaCount = 0;
        RecommendationText = "Odaberite kvart da biste videli preporuku.";
        _districtMeetings.Clear();
        FilteredMeetings.Clear();
    }

    private void ApplyFilter()
    {
        IEnumerable<MeetingDto> filtered = _activeFilter switch
        {
            "scheduled"  => _districtMeetings.Where(m => m.IsScheduled),
            "inprogress" => _districtMeetings.Where(m => m.IsInPreparation),
            _            => _districtMeetings
        };

        FilteredMeetings.Clear();
        foreach (var m in filtered) FilteredMeetings.Add(m);
    }

    private void RequestScheduleMeetingDialog()
    {
        long districtId = _selectedDistrict?.DistrictId ?? 0;
        MeetingType? recommended = _selectedDistrict?.Recommendation;

        ScheduleMeetingDialogRequested?.Invoke(districtId, recommended, success =>
        {
            if (!success) return;
            NotificationRequested?.Invoke("Sastanak je uspešno zakazan!", "Uspeh", true);
            long? rememberedId = _selectedDistrict?.DistrictId;
            LoadAll();
            if (rememberedId.HasValue)
            {
                var sel = Districts.FirstOrDefault(d => d.DistrictId == rememberedId.Value);
                if (sel != null) SelectedDistrict = sel;
            }
        });
    }

    public void RunResolveCycle()
    {
        List<MeetingDto> all = _meetingsService.GetAllByCoordinatorId(_coordinatorId);
        List<MeetingDto> ready = all.Where(m => m.IsReadyForResolve).ToList();
        if (ready.Count == 0) return;

        foreach (MeetingDto meeting in ready)
        {
            ResolveResult result = _meetingsService.ResolveMeeting(meeting.Id);

            if (!result.HasVotes)
            {
                NotificationRequested?.Invoke(
                    $"Sastanak '{meeting.MeetingTypeDisplay}' za kvart '{meeting.DistrictName}' " +
                    $"je automatski otkazan jer nema glasova.",
                    "Sastanak otkazan", true);
            }
            else if (result.IsTie)
            {
                NotificationRequested?.Invoke(
                    $"Glasanje za sastanak '{meeting.MeetingTypeDisplay}' (kvart: '{meeting.DistrictName}') " +
                    $"je izjednačeno. Molimo odaberite jedan od predloženih datuma.",
                    "Izjednačen broj glasova", true);

                MeetingDto m = meeting;
                ResolveTieDialogRequested?.Invoke(m, result.TiedDates, chosen =>
                {
                    if (chosen.HasValue) _meetingsService.ConfirmMeetingDate(m.Id, chosen.Value);
                });
            }
            else
            {
                NotificationRequested?.Invoke(
                    $"Sastanak '{meeting.MeetingTypeDisplay}' za kvart '{meeting.DistrictName}' " +
                    $"je zakazan za {result.WinnerDate:dd.MM.yyyy.} u {meeting.MeetingTimeDisplay}.",
                    "Sastanak zakazan", true);
            }
        }

        long? rememberedId = _selectedDistrict?.DistrictId;
        LoadAll();
        if (rememberedId.HasValue)
        {
            var sel = Districts.FirstOrDefault(d => d.DistrictId == rememberedId.Value);
            if (sel != null) SelectedDistrict = sel;
        }
    }
}
