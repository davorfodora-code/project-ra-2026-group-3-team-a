namespace CommunityHub.Application.Dto.BuildingManager;

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

    // Koristi se za prikaz u ComboBox-u (DisplayMemberPath="DisplayText")
    public string DisplayText => $"{Street} {StreetNumber}, {Neighborhood} — {City}";
}
