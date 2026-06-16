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

public class DistrictRegistrationViewModel : ViewModelBase
{
    private readonly DistrictService _service;
    private readonly long _coordinatorId;
    private int _currentStep = 1;

    public DistrictRegistrationViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictService(
            Injector.Get<IDistrictRepository>(),
            Injector.Get<IDistrictAccessRequestRepository>());

        BasicInfo  = new DistrictBasicInfoViewModel();
        StreetForm = new DistrictStreetInputViewModel();
        ImageSlots = new ObservableCollection<ImageSlotViewModel>();

        NextStepCommand     = new RelayCommand(_ => NextStep());
        BackStepCommand     = new RelayCommand(_ => BackStep(), _ => _currentStep > 1);
        CancelCommand       = new RelayCommand(_ => CancelRequested?.Invoke());
        AddImageSlotCommand = new RelayCommand(_ => AddMultipleImagesRequested?.Invoke());
        RemoveImageCommand  = new RelayCommand(p =>
        {
            if (p is ImageSlotViewModel slot) ImageSlots.Remove(slot);
        });
    }

    public DistrictBasicInfoViewModel             BasicInfo  { get; }
    public DistrictStreetInputViewModel           StreetForm { get; }
    public ObservableCollection<ImageSlotViewModel> ImageSlots { get; }

    public int CurrentStep
    {
        get => _currentStep;
        private set
        {
            if (SetProperty(ref _currentStep, value))
            {
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
                OnPropertyChanged(nameof(IsStep3));
                OnPropertyChanged(nameof(StepIndicator));
                OnPropertyChanged(nameof(NextButtonText));
            }
        }
    }

    public bool IsStep1 => _currentStep == 1;
    public bool IsStep2 => _currentStep == 2;
    public bool IsStep3 => _currentStep == 3;

    public bool HasUnsavedInput =>
        !string.IsNullOrWhiteSpace(BasicInfo.Name) ||
        !string.IsNullOrWhiteSpace(BasicInfo.Description) ||
        !string.IsNullOrWhiteSpace(BasicInfo.City) ||
        !string.IsNullOrWhiteSpace(BasicInfo.Country) ||
        StreetForm.Streets.Count > 0 ||
        ImageSlots.Count > 0 ||
        _currentStep > 1;

    public string StepIndicator => _currentStep switch
    {
        1 => "Korak 1/3 – Podaci kvarta",
        2 => "Korak 2/3 – Ulice kvarta",
        3 => "Korak 3/3 – Slike kvarta",
        _ => string.Empty
    };

    public string NextButtonText => _currentStep == 3 ? "Registruj" : "Sledeći korak";

    public ICommand NextStepCommand     { get; }
    public ICommand BackStepCommand     { get; }
    public ICommand CancelCommand       { get; }
    public ICommand AddImageSlotCommand { get; }
    public ICommand RemoveImageCommand  { get; }

    public event Action? AddMultipleImagesRequested;
    public event Action? CancelRequested;
    public event Action? RegistrationCompleted;
    public event Action<string, bool>? NotificationRequested;

    public void AddImagePaths(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            var slot = new ImageSlotViewModel(ImageSlots.Count + 1) { FilePath = path };
            ImageSlots.Add(slot);
        }
    }

    private void NextStep()
    {
        if (_currentStep == 1)
        {
            if (!BasicInfo.ValidateAll()) return;
            CurrentStep = 2;
        }
        else if (_currentStep == 2)
        {
            CurrentStep = 3;
        }
        else if (_currentStep == 3)
        {
            TryRegister();
        }
    }

    private void BackStep()
    {
        if (_currentStep > 1) CurrentStep = _currentStep - 1;
    }

    private void TryRegister()
    {
        List<string> imagePaths = ImageSlots
            .Where(s => s.FilePath != null)
            .Select(s => s.FilePath!)
            .ToList();

        CreateDistrictDto dto = new CreateDistrictDto
        {
            Name          = BasicInfo.Name.Trim(),
            Description   = BasicInfo.Description.Trim(),
            City          = BasicInfo.City.Trim(),
            Country       = BasicInfo.Country.Trim(),
            Streets       = StreetForm.Streets.ToList(),
            ImagePaths    = imagePaths,
            CoordinatorId = _coordinatorId
        };

        try
        {
            _service.Create(dto);
            NotificationRequested?.Invoke("Kvart je uspešno registrovan!", true);
            RegistrationCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke($"Greška pri registraciji: {ex.Message}", false);
        }
    }
}
