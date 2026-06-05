namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class AttendanceItemDto
{
    public int ApartmentNumber { get; init; }
    public bool IsConfirmed { get; init; }

    public string Display => IsConfirmed
        ? $"Stan {ApartmentNumber}: potvrđeno"
        : $"Stan {ApartmentNumber}: nije potvrđeno";
}
