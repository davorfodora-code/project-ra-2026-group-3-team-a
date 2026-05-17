using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class DistrictStreetInputViewModel : ViewModelBase
{
    private string _streetName = string.Empty;
    private string _streetStart = string.Empty;
    private string _streetEnd = string.Empty;
    private string _streetError = string.Empty;

    public DistrictStreetInputViewModel()
    {
        Streets = new ObservableCollection<StreetInputDto>();
        AddStreetCommand    = new RelayCommand(_ => AddStreet());
        RemoveStreetCommand = new RelayCommand(p =>
        {
            if (p is StreetInputDto s) Streets.Remove(s);
        });
    }

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

    public ICommand AddStreetCommand    { get; }
    public ICommand RemoveStreetCommand { get; }

    private bool SetStreetError(string message) { StreetError = message; return false; }

    private bool TryParseStreetInput(out string name, out int start, out int end)
    {
        start = 0; end = 0;
        name = _streetName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return SetStreetError("Naziv ulice ne sme biti prazan.");
        if (!int.TryParse(_streetStart.Trim(), out start) || start <= 0)
            return SetStreetError("Neispravan početni broj (mora biti pozitivan ceo broj).");
        if (!int.TryParse(_streetEnd.Trim(), out end) || end <= 0)
            return SetStreetError("Neispravan krajnji broj (mora biti pozitivan ceo broj).");
        if (end < start)
            return SetStreetError("Krajnji broj mora biti veći ili jednak početnom.");
        return true;
    }

    private void ClearStreetInputFields()
    {
        StreetName  = string.Empty;
        StreetStart = string.Empty;
        StreetEnd   = string.Empty;
    }

    private void AddStreet()
    {
        StreetError = string.Empty;
        if (!TryParseStreetInput(out string name, out int start, out int end)) return;
        Streets.Add(new StreetInputDto { StreetName = name, StartNumber = start, EndNumber = end });
        ClearStreetInputFields();
    }
}
