using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class DistrictRegistrationViewModel : ViewModelBase
{
    private readonly DistrictService _service;
    private readonly long _coordinatorId;

    private int _currentStep = 1;

    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _city = string.Empty;
    private string _country = string.Empty;

    private string _nameError = string.Empty;
    private string _descriptionError = string.Empty;
    private string _cityError = string.Empty;
    private string _countryError = string.Empty;

    private string _streetName = string.Empty;
    private string _streetStart = string.Empty;
    private string _streetEnd = string.Empty;
    private string _streetError = string.Empty;

    public DistrictRegistrationViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictService();

        Streets    = new ObservableCollection<StreetInputDto>();
        ImageSlots = new ObservableCollection<ImageSlotViewModel>
        {
            new ImageSlotViewModel(1),
            new ImageSlotViewModel(2),
            new ImageSlotViewModel(3),
            new ImageSlotViewModel(4),
        };

        NextStepCommand   = new RelayCommand(_ => NextStep());
        BackStepCommand   = new RelayCommand(_ => BackStep(), _ => _currentStep > 1);
        CancelCommand     = new RelayCommand(_ => CancelRequested?.Invoke());
        AddStreetCommand  = new RelayCommand(_ => AddStreet());
        RemoveStreetCommand = new RelayCommand(p =>
        {
            if (p is StreetInputDto s) Streets.Remove(s);
        });
        PickImageCommand = new RelayCommand(p =>
        {
            if (p is ImageSlotViewModel slot)
                PickImageRequested?.Invoke(slot);
        });
    }

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

    public string StepIndicator => _currentStep switch
    {
        1 => "Korak 1/3 – Podaci kvarta",
        2 => "Korak 2/3 – Ulice kvarta",
        3 => "Korak 3/3 – Slike kvarta",
        _ => string.Empty
    };

    public string NextButtonText => _currentStep == 3 ? "Registruj" : "Sledeći korak";

    public string Name
    {
        get => _name;
        set { if (SetProperty(ref _name, value)) ValidateName(); }
    }

    public string Description
    {
        get => _description;
        set { if (SetProperty(ref _description, value)) ValidateDescription(); }
    }

    public string City
    {
        get => _city;
        set { if (SetProperty(ref _city, value)) ValidateCity(); }
    }

    public string Country
    {
        get => _country;
        set { if (SetProperty(ref _country, value)) ValidateCountry(); }
    }

    public string NameError
    {
        get => _nameError;
        private set { if (SetProperty(ref _nameError, value)) OnPropertyChanged(nameof(HasNameError)); }
    }
    public bool HasNameError => !string.IsNullOrEmpty(_nameError);

    public string DescriptionError
    {
        get => _descriptionError;
        private set { if (SetProperty(ref _descriptionError, value)) OnPropertyChanged(nameof(HasDescriptionError)); }
    }
    public bool HasDescriptionError => !string.IsNullOrEmpty(_descriptionError);

    public string CityError
    {
        get => _cityError;
        private set { if (SetProperty(ref _cityError, value)) OnPropertyChanged(nameof(HasCityError)); }
    }
    public bool HasCityError => !string.IsNullOrEmpty(_cityError);

    public string CountryError
    {
        get => _countryError;
        private set { if (SetProperty(ref _countryError, value)) OnPropertyChanged(nameof(HasCountryError)); }
    }
    public bool HasCountryError => !string.IsNullOrEmpty(_countryError);

    public ObservableCollection<StreetInputDto> Streets { get; }

    public string StreetName
    {
        get => _streetName;
        set => SetProperty(ref _streetName, value);
    }

    public string StreetStart
    {
        get => _streetStart;
        set => SetProperty(ref _streetStart, value);
    }

    public string StreetEnd
    {
        get => _streetEnd;
        set => SetProperty(ref _streetEnd, value);
    }

    public string StreetError
    {
        get => _streetError;
        private set { if (SetProperty(ref _streetError, value)) OnPropertyChanged(nameof(HasStreetError)); }
    }
    public bool HasStreetError => !string.IsNullOrEmpty(_streetError);

    public ObservableCollection<ImageSlotViewModel> ImageSlots { get; }

    public ICommand NextStepCommand     { get; }
    public ICommand BackStepCommand     { get; }
    public ICommand CancelCommand       { get; }
    public ICommand AddStreetCommand    { get; }
    public ICommand RemoveStreetCommand { get; }
    public ICommand PickImageCommand    { get; }

    public event Action<ImageSlotViewModel>? PickImageRequested;
    public event Action? CancelRequested;
    public event Action? RegistrationCompleted;
    public event Action<string, bool>? NotificationRequested; // (poruka, jeLiUspjeh)


    private void NextStep()
    {
        if (_currentStep == 1)
        {
            if (!ValidateStep1()) return;
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

    private void ValidateName()
    {
        NameError = string.IsNullOrWhiteSpace(_name) ? "Naziv kvarta je obavezna stavka." : string.Empty;
    }

    private void ValidateDescription()
    {
        string text = _description.Trim();
        DescriptionError = string.IsNullOrWhiteSpace(text)
            ? "Opis kvarta je obavezna stavka."
            : text.Length < 30
                ? "Opis kvarta mora imati najmanje 30 karaktera."
                : string.Empty;
    }

    private void ValidateCity()
    {
        CityError = string.IsNullOrWhiteSpace(_city) ? "Grad je obavezna stavka." : string.Empty;
    }

    private void ValidateCountry()
    {
        CountryError = string.IsNullOrWhiteSpace(_country) ? "Država je obavezna stavka." : string.Empty;
    }

    private bool ValidateStep1()
    {
        ValidateName();
        ValidateDescription();
        ValidateCity();
        ValidateCountry();
        return !HasNameError && !HasDescriptionError && !HasCityError && !HasCountryError;
    }

    private void AddStreet()
    {
        StreetError = string.Empty;

        string name = _streetName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StreetError = "Naziv ulice ne sme biti prazan.";
            return;
        }

        if (!int.TryParse(_streetStart.Trim(), out int start) || start <= 0)
        {
            StreetError = "Neispravan početni broj (mora biti pozitivan ceo broj).";
            return;
        }

        if (!int.TryParse(_streetEnd.Trim(), out int end) || end <= 0)
        {
            StreetError = "Neispravan krajnji broj (mora biti pozitivan ceo broj).";
            return;
        }

        if (end < start)
        {
            StreetError = "Krajnji broj mora biti veći ili jednak početnom.";
            return;
        }

        Streets.Add(new StreetInputDto
        {
            StreetName  = name,
            StartNumber = start,
            EndNumber   = end
        });

        StreetName  = string.Empty;
        StreetStart = string.Empty;
        StreetEnd   = string.Empty;
    }

    private void TryRegister()
    {
        List<string> imagePaths = ImageSlots
            .Where(s => s.FilePath != null)
            .Select(s => s.FilePath!)
            .ToList();

        CreateDistrictDto dto = new CreateDistrictDto
        {
            Name          = _name.Trim(),
            Description   = _description.Trim(),
            City          = _city.Trim(),
            Country       = _country.Trim(),
            Streets       = Streets.ToList(),
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
