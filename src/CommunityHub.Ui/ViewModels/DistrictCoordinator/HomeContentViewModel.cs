using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class HomeContentViewModel : ViewModelBase
{
    private readonly DistrictService _districtService;
    private DistrictHomeCardViewModel? _selectedCard;
    private bool _loaded;

    public HomeContentViewModel(long coordinatorId)
    {
        CoordinatorId    = coordinatorId;
        _districtService = new DistrictService();
        DistrictCards    = new ObservableCollection<DistrictHomeCardViewModel>();

        RegisterDistrictCommand = new RelayCommand(_ => RegisterDistrictRequested?.Invoke());
        SelectDistrictCommand   = new RelayCommand(p =>
        {
            if (p is DistrictHomeCardViewModel card) SelectedCard = card;
        });
        OpenFinancesCommand     = new RelayCommand(p =>
        {
            if (p is DistrictHomeCardViewModel card)
                FinanceRequested?.Invoke(card.Name);
        });
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

    public ICommand RegisterDistrictCommand { get; }
    public ICommand SelectDistrictCommand   { get; }
    public ICommand OpenFinancesCommand     { get; }

    public event Action? RegisterDistrictRequested;
    public event Action<string>? FinanceRequested;
    public event Action<DistrictHomeCardViewModel?>? SelectionChanged;


    public void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        Refresh();
    }

    public void Refresh()
    {
        DistrictCards.Clear();
        foreach (DistrictHomeCardDto dto in _districtService.GetHomeCardsByCoordinatorId(CoordinatorId))
            DistrictCards.Add(new DistrictHomeCardViewModel(dto));

        OnPropertyChanged(nameof(DistrictCount));
        OnPropertyChanged(nameof(DistrictCountSummary));

        SelectedCard = DistrictCards.FirstOrDefault();
    }
}
