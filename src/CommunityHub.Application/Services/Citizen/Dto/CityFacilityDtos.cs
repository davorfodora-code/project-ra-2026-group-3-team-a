using System.Globalization;

namespace CommunityHub.Application.Dto.Citizen;

public class CityFacilityListItemDto
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public bool HasMyVote { get; set; }
    public DateTime? LastReservationDate { get; set; }

    public string LastReservationDisplay =>
        LastReservationDate.HasValue
            ? LastReservationDate.Value.ToString("dd.MM.yyyy. HH:mm")
            : "Nije bilo obilazaka";
}

public class CityFacilityReservationHistoryDto
{
    public long FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public string Note { get; set; } = string.Empty;

    public string ReservedAtDisplay => ReservedAt.ToString("dd.MM.yyyy. HH:mm");
}

public class CityFacilityVisitStatDto
{
    public long FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
}

public class CityFacilityStatisticsDto
{
    public int TotalReservations { get; set; }
    public List<CityFacilityVisitStatDto> FacilityStats { get; set; } = new();
}

public class CityFacilityActionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static CityFacilityActionResultDto Ok(string message) =>
        new() { Success = true, Message = message };

    public static CityFacilityActionResultDto Fail(string message) =>
        new() { Success = false, Message = message };
}