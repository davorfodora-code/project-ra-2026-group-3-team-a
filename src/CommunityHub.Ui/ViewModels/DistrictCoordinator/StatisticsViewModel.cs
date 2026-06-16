using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class StatisticsViewModel : ViewModelBase
{
    private readonly DistrictStatisticsService _statsService;
    private readonly DistrictMeetingService    _meetingsService;
    private readonly long _coordinatorId;

    private CitizenTrustStatisticDto? _selectedDistrict;
    private int _novCount, _neaktivanCount, _aktivanCount, _istaknutiCount, _odPoverenjaCount;
    private string _recommendationText = "Odaberite kvart da biste videli preporuku.";

    public StatisticsViewModel(long coordinatorId)
    {
        _coordinatorId   = coordinatorId;
        _statsService    = new DistrictStatisticsService(
            Injector.Get<ICitizenTrustStatisticRepository>());
        _meetingsService = new DistrictMeetingService(
            Injector.Get<IDistrictMeetingRepository>());

        Districts   = new ObservableCollection<CitizenTrustStatisticDto>();
        MeetingList = new MeetingListViewModel();

        SetFilterCommand       = MeetingList.SetFilterCommand;
        ScheduleMeetingCommand = new RelayCommand(_ => RequestScheduleMeetingDialog());
        RefreshCommand         = new RelayCommand(_ => Initialize());
    }

    public ObservableCollection<CitizenTrustStatisticDto> Districts   { get; }
    public MeetingListViewModel                           MeetingList { get; }

    public CitizenTrustStatisticDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set { if (SetProperty(ref _selectedDistrict, value)) OnSelectedDistrictChanged(); }
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

    public ICommand SetFilterCommand       { get; }
    public ICommand ScheduleMeetingCommand { get; }
    public ICommand RefreshCommand         { get; }

    public event Action<long, MeetingType?, Action<bool>>? ScheduleMeetingDialogRequested;
    public event Action<MeetingDto, List<DateTime>, Action<DateTime?>>? ResolveTieDialogRequested;
    public event Action<string, string, bool>? NotificationRequested;

    public void Initialize()
    {
        try   { LoadAll(); }
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
        foreach (CitizenTrustStatisticDto s in stats) Districts.Add(s);

        if (Districts.Count > 0) SelectedDistrict = Districts[0];
        else                     ResetSelection();
    }

    private void OnSelectedDistrictChanged()
    {
        if (_selectedDistrict == null) { ResetSelection(); return; }

        NovCount         = _selectedDistrict.NovCount;
        NeaktivanCount   = _selectedDistrict.NeaktivanCount;
        AktivanCount     = _selectedDistrict.AktivanCount;
        IstaknutiCount   = _selectedDistrict.IstaknutiCount;
        OdPoverenjaCount = _selectedDistrict.OdPoverenjaCount;
        RecommendationText = _selectedDistrict.RecommendationDisplay;

        IEnumerable<MeetingDto> meetings = _meetingsService
            .GetAllByCoordinatorId(_coordinatorId)
            .Where(m => m.DistrictId == _selectedDistrict.DistrictId)
            .OrderByDescending(m => m.DateRangeStart);

        MeetingList.Load(meetings);
    }

    private void ResetSelection()
    {
        NovCount         = 0;
        NeaktivanCount   = 0;
        AktivanCount     = 0;
        IstaknutiCount   = 0;
        OdPoverenjaCount = 0;
        RecommendationText = "Odaberite kvart da biste videli preporuku.";
        MeetingList.Reset();
    }

    private void RequestScheduleMeetingDialog()
    {
        long districtId      = _selectedDistrict?.DistrictId ?? 0;
        MeetingType? recommended = _selectedDistrict?.Recommendation;

        ScheduleMeetingDialogRequested?.Invoke(districtId, recommended, success =>
        {
            if (!success) return;
            NotificationRequested?.Invoke("Sastanak je uspešno zakazan!", "Uspeh", true);
            long? rememberedId = _selectedDistrict?.DistrictId;
            LoadAll();
            if (rememberedId.HasValue)
            {
                CitizenTrustStatisticDto? sel = Districts.FirstOrDefault(d => d.DistrictId == rememberedId.Value);
                if (sel != null) SelectedDistrict = sel;
            }
        });
    }

    public void RunResolveCycle()
    {
        List<MeetingDto> ready = _meetingsService
            .GetAllByCoordinatorId(_coordinatorId)
            .Where(m => m.IsReadyForResolve)
            .ToList();
        if (ready.Count == 0) return;

        long? rememberedId = _selectedDistrict?.DistrictId;
        foreach (MeetingDto meeting in ready)
            MeetingResolveHelper.HandleResult(
                meeting, _meetingsService.ResolveMeeting(meeting.Id),
                _meetingsService, NotificationRequested!, ResolveTieDialogRequested!);

        LoadAll();
        CitizenTrustStatisticDto? restored = rememberedId.HasValue
            ? Districts.FirstOrDefault(d => d.DistrictId == rememberedId.Value) : null;
        if (restored != null) SelectedDistrict = restored;
    }
}
