using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

internal static class CitizenDistrictEventDtoMapper
{
    public static DistrictEventListItemDto MapToListItemDto(DistrictEvent districtEvent, long citizenId)
    {
        return new DistrictEventListItemDto
        {
            EventId = districtEvent.Id,
            Name = districtEvent.Name,
            Description = districtEvent.Description,
            EventDate = districtEvent.EventDate,
            StartTime = districtEvent.StartTime,
            Duration = districtEvent.Duration,
            MinVolunteers = districtEvent.MinVolunteers,
            CurrentVolunteers = districtEvent.VolunteerApplications.Count,
            Status = districtEvent.Status,
            IsOrganizer = districtEvent.Organizer.Id == citizenId,
            RequiredItems = MapRequiredItems(districtEvent)
        };
    }

    public static DistrictEventDetailsDto MapToDetailsDto(DistrictEvent districtEvent, long citizenId)
    {
        return new DistrictEventDetailsDto
        {
            EventId = districtEvent.Id,
            DistrictId = districtEvent.District.Id,
            DistrictName = districtEvent.District.Name,
            OrganizerId = districtEvent.Organizer.Id,
            OrganizerName = $"{districtEvent.Organizer.Name} {districtEvent.Organizer.Surname}".Trim(),
            Name = districtEvent.Name,
            Description = districtEvent.Description,
            EventDate = districtEvent.EventDate,
            StartTime = districtEvent.StartTime,
            Duration = districtEvent.Duration,
            MinVolunteers = districtEvent.MinVolunteers,
            CurrentVolunteers = districtEvent.VolunteerApplications.Count,
            Status = districtEvent.Status,
            IsCitizenAlreadyApplied = districtEvent.VolunteerApplications.Any(a => a.Citizen.Id == citizenId),
            RequiredItems = MapRequiredItems(districtEvent)
        };
    }

    private static List<EventRequiredItemDto> MapRequiredItems(DistrictEvent districtEvent)
    {
        Dictionary<long, string> citizenNames = districtEvent.VolunteerApplications
            .GroupBy(a => a.Citizen.Id)
            .ToDictionary(
                g => g.Key,
                g => $"{g.First().Citizen.Name} {g.First().Citizen.Surname}".Trim());

        return districtEvent.RequiredItems
            .Select(item => new EventRequiredItemDto
            {
                ItemId = item.Id,
                Name = item.Name,
                AssignedVolunteerId = item.AssignedVolunteerId,
                AssignedVolunteerName = ResolveVolunteerName(citizenNames, item.AssignedVolunteerId)
            })
            .ToList();
    }

    private static string? ResolveVolunteerName(Dictionary<long, string> citizenNames, long? volunteerId)
    {
        if (!volunteerId.HasValue)
        {
            return null;
        }

        return citizenNames.TryGetValue(volunteerId.Value, out string? name) ? name : null;
    }
}
