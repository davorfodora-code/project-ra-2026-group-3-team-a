using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class DistrictDetailViewModel : ViewModelBase
{
    private int _currentImageIndex;

    public DistrictDetailViewModel(DistrictDetailDto dto)
    {
        Name        = dto.Name;
        Description = string.IsNullOrWhiteSpace(dto.Description) ? "—" : dto.Description;
        Location    = $"{dto.City}, {dto.Country}";
        Budget      = dto.Budget.ToString("N0", CultureInfo.GetCultureInfo("sr-Latn-RS")) + " RSD";
        HasImages   = dto.ImagePaths.Count > 0;
        HasStreets  = dto.Streets.Count > 0;

        Streets    = new ObservableCollection<string>(dto.Streets);
        ImagePaths = new ObservableCollection<string>(dto.ImagePaths);

        Dots = new ObservableCollection<ImageDotViewModel>(
            dto.ImagePaths.Select((_, i) => new ImageDotViewModel { IsActive = i == 0 }));

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        PrevCommand  = new RelayCommand(_ => CurrentImageIndex--, _ => CanGoPrev);
        NextCommand  = new RelayCommand(_ => CurrentImageIndex++, _ => CanGoNext);
    }

    public string Name        { get; }
    public string Description { get; }
    public string Location    { get; }
    public string Budget      { get; }
    public bool   HasImages   { get; }
    public bool   HasStreets  { get; }
    public bool   HasMultipleImages => ImagePaths.Count > 1;

    public ObservableCollection<string>           Streets    { get; }
    public ObservableCollection<string>           ImagePaths { get; }
    public ObservableCollection<ImageDotViewModel> Dots      { get; }

    public int CurrentImageIndex
    {
        get => _currentImageIndex;
        private set
        {
            if (SetProperty(ref _currentImageIndex, value))
            {
                OnPropertyChanged(nameof(CurrentImagePath));
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(ImageCounterDisplay));
                UpdateDots();
                (PrevCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (NextCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? CurrentImagePath    => HasImages ? ImagePaths[_currentImageIndex] : null;
    public bool    CanGoPrev           => _currentImageIndex > 0;
    public bool    CanGoNext           => _currentImageIndex < ImagePaths.Count - 1;
    public string  ImageCounterDisplay => $"{_currentImageIndex + 1} / {ImagePaths.Count}";

    public ICommand CloseCommand { get; }
    public ICommand PrevCommand  { get; }
    public ICommand NextCommand  { get; }

    public event Action? CloseRequested;

    private void UpdateDots()
    {
        for (int i = 0; i < Dots.Count; i++)
            Dots[i].IsActive = i == _currentImageIndex;
    }
}
