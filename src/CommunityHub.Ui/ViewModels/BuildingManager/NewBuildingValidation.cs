using System.Collections.ObjectModel;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Validacija forme za novu zgradu i konstrukcija CreateBuildingDto objekta.
// Izvuceno iz NewBuildingViewModel-a da se umanji slozenost klase (WMC).
internal static class NewBuildingValidation
{
    public static string? Validate(
        string addressInput, string neighborhood, string city, string country,
        string floorsText,
        ObservableCollection<string> imagePaths,
        ObservableCollection<FloorInputViewModel> floors)
    {
        if (string.IsNullOrWhiteSpace(addressInput))
            return "Unesite adresu zgrade.";
        if (addressInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            return "Adresa mora sadrzati ulicu i kucni broj (npr. Nemanjina 4).";
        if (string.IsNullOrWhiteSpace(neighborhood)) return "Unesite naziv naselja.";
        if (string.IsNullOrWhiteSpace(city))         return "Unesite grad.";
        if (string.IsNullOrWhiteSpace(country))      return "Unesite drzavu.";
        if (!int.TryParse(floorsText, out int f) || f <= 0)
            return "Broj spratova mora biti pozitivan broj.";
        if (imagePaths.Count == 0) return "Dodajte bar jednu sliku zgrade.";
        foreach (FloorInputViewModel floor in floors)
            if (floor.Apartments.Count == 0)
                return $"Sprat {floor.FloorNumber} mora imati bar jedan stan.";
        return null;
    }

    public static CreateBuildingDto BuildDto(
        long managerId,
        string addressInput, string neighborhood, string city, string country,
        string floorsText,
        ObservableCollection<string> imagePaths,
        ObservableCollection<FloorInputViewModel> floors)
    {
        string[] parts = addressInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string streetNumber = parts[^1];
        string street = string.Join(" ", parts[..^1]);
        int.TryParse(floorsText, out int floorsCount);
        CreateBuildingDto dto = new CreateBuildingDto
        {
            Street = street, StreetNumber = streetNumber,
            Neighborhood = neighborhood.Trim(), City = city.Trim(), Country = country.Trim(),
            NumberOfFloors = floorsCount, ManagerId = managerId,
            ImagePaths = new List<string>(imagePaths)
        };
        foreach (FloorInputViewModel floor in floors)
            dto.Floors.Add(new FloorInputDto { FloorNumber = floor.FloorNumber, ApartmentNumbers = floor.Apartments.ToList() });
        return dto;
    }
}
