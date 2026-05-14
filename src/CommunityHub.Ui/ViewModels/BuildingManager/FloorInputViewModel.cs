using System.Collections.ObjectModel;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za jedan sprat u formi nove zgrade. Drzi listu stanova i UI stanje
// za inline dodavanje novog stana (otvoren TextBox + njegov tekst).
public class FloorInputViewModel : BaseViewModel
{
    private bool _isAddingApartment;
    private string _newApartmentText = string.Empty;

    public FloorInputViewModel(int floorNumber)
    {
        FloorNumber = floorNumber;
    }

    public int FloorNumber { get; }
    public string Label => $"Sprat {FloorNumber}";
    public ObservableCollection<int> Apartments { get; } = new();

    public bool IsAddingApartment
    {
        get => _isAddingApartment;
        set => SetProperty(ref _isAddingApartment, value);
    }

    public string NewApartmentText
    {
        get => _newApartmentText;
        set => SetProperty(ref _newApartmentText, value);
    }

    public void StartAddingApartment()
    {
        NewApartmentText = string.Empty;
        IsAddingApartment = true;
    }

    public void ConfirmApartment()
    {
        if (int.TryParse(_newApartmentText.Trim(), out int n) && n > 0 && !Apartments.Contains(n))
        {
            Apartments.Add(n);
            // Drzi listu sortiranom — ObservableCollection nema Sort, pa rebild
            List<int> sorted = Apartments.OrderBy(x => x).ToList();
            Apartments.Clear();
            foreach (int x in sorted) Apartments.Add(x);
        }
        NewApartmentText = string.Empty;
        IsAddingApartment = false;
    }

    public void RemoveApartment(int number) => Apartments.Remove(number);
}
