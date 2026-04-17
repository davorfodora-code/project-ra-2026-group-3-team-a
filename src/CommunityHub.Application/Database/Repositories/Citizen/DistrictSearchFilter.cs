namespace CommunityHub.Application.Database.Repositories.Citizen;

public sealed record DistrictSearchFilter(
    string? Name,
    string? Address,
    string? City,
    string? Country);
