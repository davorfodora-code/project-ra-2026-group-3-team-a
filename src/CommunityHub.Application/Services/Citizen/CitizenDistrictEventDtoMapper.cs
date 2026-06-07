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
        Dictionary<long, string> citizenNames = BuildCitizenNameLookup(districtEvent);
        Dictionary<long, long> itemOwnerFromApplications = BuildItemOwnerLookup(districtEvent);

        return districtEvent.RequiredItems
            .Select(item => MapRequiredItem(item, citizenNames, itemOwnerFromApplications))
            .ToList();
    }

    private static Dictionary<long, string> BuildCitizenNameLookup(DistrictEvent districtEvent)
    {
        return districtEvent.VolunteerApplications
            .GroupBy(a => a.Citizen.Id)
            .ToDictionary(g => g.Key, g => FormatCitizenName(g.First().Citizen));
    }

    private static Dictionary<long, long> BuildItemOwnerLookup(DistrictEvent districtEvent)
    {
        return districtEvent.VolunteerApplications
            .SelectMany(a => a.SelectedItemIds.Select(itemId => (itemId, a.Citizen.Id)))
            .GroupBy(x => x.itemId)
            .ToDictionary(g => g.Key, g => g.First().Item2);
    }

    private static EventRequiredItemDto MapRequiredItem(
        DistrictEventRequiredItem item,
        Dictionary<long, string> citizenNames,
        Dictionary<long, long> itemOwnerFromApplications)
    {
        long? volunteerId = ResolveVolunteerId(item, itemOwnerFromApplications);

        return new EventRequiredItemDto
        {
            ItemId = item.Id,
            Name = item.Name,
            AssignedVolunteerId = volunteerId,
            AssignedVolunteerName = ResolveVolunteerName(citizenNames, volunteerId)
        };
    }

    private static long? ResolveVolunteerId(
        DistrictEventRequiredItem item,
        Dictionary<long, long> itemOwnerFromApplications)
    {
        if (item.AssignedVolunteerId.HasValue)
        {
            return item.AssignedVolunteerId;
        }

        return itemOwnerFromApplications.TryGetValue(item.Id, out long ownerId) ? ownerId : null;
    }

    private static string FormatCitizenName(User citizen)
    {
        return $"{citizen.Name} {citizen.Surname}".Trim();
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
