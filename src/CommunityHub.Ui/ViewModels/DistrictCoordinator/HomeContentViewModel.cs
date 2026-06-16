using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class HomeContentViewModel : ViewModelBase
{
    private readonly DistrictService _districtService;
    private DistrictHomeCardViewModel? _selectedCard;
    private bool _loaded;
    private bool _detailsLoaded;
    private Dictionary<long, DistrictDetailDto> _detailCache = new();

    public HomeContentViewModel(long coordinatorId)
    {
        CoordinatorId    = coordinatorId;
        _districtService = new DistrictService(
            Injector.Get<IDistrictRepository>(),
            Injector.Get<IDistrictAccessRequestRepository>());
        DistrictCards    = new ObservableCollection<DistrictHomeCardViewModel>();

        RegisterDistrictCommand     = new RelayCommand(_ => RegisterDistrictRequested?.Invoke());
        SelectDistrictCommand       = new RelayCommand(p =>
        {
            if (p is DistrictHomeCardViewModel card)
            {
                SelectedCard = card;
                UserDistrictClicked?.Invoke(card.DistrictId);
            }
        });
        OpenFinancesCommand         = new RelayCommand(p =>
        {
            if (p is DistrictHomeCardViewModel card)
                FinanceRequested?.Invoke(card.DistrictId);
        });
        OpenDistrictDetailCommand   = new RelayCommand(p =>
        {
            if (p is DistrictHomeCardViewModel card)
            {
                SelectedCard = card;
                OpenDistrictDetail(card.DistrictId);
            }
        });
        RefreshCommand = new RelayCommand(_ => Refresh());
    }

    public long CoordinatorId { get; }
    public ObservableCollection<DistrictHomeCardViewModel> DistrictCards { get; }

    public DistrictHomeCardViewModel? SelectedCard
    {
        get => _selectedCard;
        set
        {
            DistrictHomeCardViewModel? previous = _selectedCard;
            if (SetProperty(ref _selectedCard, value))
            {
                if (previous != null) previous.IsSelected = false;
                if (value != null)    value.IsSelected    = true;
                SelectionChanged?.Invoke(value);
            }
        }
    }

    public int DistrictCount => DistrictCards.Count;

    public string DistrictCountSummary =>
        DistrictCount == 1
            ? "Vi ste koordinator 1 kvarta"
            : $"Vi ste koordinator {DistrictCount} kvarta";

    public ICommand RegisterDistrictCommand   { get; }
    public ICommand SelectDistrictCommand     { get; }
    public ICommand OpenFinancesCommand       { get; }
    public ICommand OpenDistrictDetailCommand { get; }
    public ICommand RefreshCommand            { get; }

    public event Action? RegisterDistrictRequested;
    public event Action<long>? FinanceRequested;
    public event Action<long>? UserDistrictClicked;
    public event Action<DistrictHomeCardViewModel?>? SelectionChanged;
    public event Action<DistrictDetailDto>? DistrictDetailRequested;


    public void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        Refresh();
    }

    public void Refresh()
    {
        _detailsLoaded = false;
        _detailCache.Clear();

        DistrictCards.Clear();
        foreach (DistrictHomeCardDto dto in _districtService.GetHomeCardsByCoordinatorId(CoordinatorId))
            DistrictCards.Add(new DistrictHomeCardViewModel(dto));

        OnPropertyChanged(nameof(DistrictCount));
        OnPropertyChanged(nameof(DistrictCountSummary));

        SelectedCard = DistrictCards.FirstOrDefault();
    }

    private void OpenDistrictDetail(long districtId)
    {
        if (!_detailsLoaded)
        {
            _detailCache = _districtService.GetDetailsByCoordinatorId(CoordinatorId)
                .ToDictionary(d => d.Id);
            _detailsLoaded = true;
        }
        if (_detailCache.TryGetValue(districtId, out DistrictDetailDto? dto))
            DistrictDetailRequested?.Invoke(dto);
    }
}
