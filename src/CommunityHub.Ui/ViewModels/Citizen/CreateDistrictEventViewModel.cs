using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CreateDistrictEventViewModel
{
    public CreateDistrictEventDto BuildDto(CreateDistrictEventFormInput input)
    {
        return new CreateDistrictEventDto
        {
            DistrictId = input.DistrictId,
            Name = input.Name.Trim(),
            Description = input.Description.Trim(),
            EventDate = input.EventDate.Date,
            StartTime = input.StartTime,
            Duration = TimeSpan.FromMinutes(input.DurationMinutes),
            MinVolunteers = input.MinVolunteers,
            RequiredItems = input.RequiredItems
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .Distinct()
                .ToList()
        };
    }
}

public class CreateDistrictEventFormInput
{
    public long DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public int MinVolunteers { get; set; }
    public IEnumerable<string> RequiredItems { get; set; } = Enumerable.Empty<string>();
}
