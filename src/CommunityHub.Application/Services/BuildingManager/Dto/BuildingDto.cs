namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class BuildingDto
{
    public long Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int NumberOfFloors { get; set; }
    public int TotalApartments { get; set; }
    public List<string> ImagePaths { get; set; } = new();
    public List<FloorSummaryDto> Floors { get; set; } = new();

    public string AddressDisplay => $"{Street} {StreetNumber}";
    // Putanja prve slike — null ako nema slika
    public string? FirstImagePath => ImagePaths.Count > 0 ? ImagePaths[0] : null;
    public string LocationDisplay => $"{City}, {Country}";
    public int ImageCount => ImagePaths.Count;

    // Koristi se za prikaz u ComboBox-u (DisplayMemberPath="DisplayText")
    public string DisplayText => $"{Street} {StreetNumber}, {Neighborhood} — {City}";
}
