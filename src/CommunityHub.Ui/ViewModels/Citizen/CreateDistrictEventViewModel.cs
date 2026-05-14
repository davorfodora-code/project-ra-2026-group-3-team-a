using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CreateDistrictEventViewModel
{
    public CreateDistrictEventDto BuildDto(
        long districtId,
        string name,
        string description,
        DateTime eventDate,
        TimeSpan startTime,
        int durationMinutes,
        int minVolunteers,
        IEnumerable<string> requiredItems)
    {
        return new CreateDistrictEventDto
        {
            DistrictId = districtId,
            Name = name.Trim(),
            Description = description.Trim(),
            EventDate = eventDate.Date,
            StartTime = startTime,
            Duration = TimeSpan.FromMinutes(durationMinutes),
            MinVolunteers = minVolunteers,
            RequiredItems = requiredItems
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .Distinct()
                .ToList()
        };
    }
}
