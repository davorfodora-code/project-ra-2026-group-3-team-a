using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class EventAttendanceViewModel
{
    public IReadOnlyList<VolunteerAttendanceDto> BuildPayload(IEnumerable<VolunteerAttendanceDto> rows)
    {
        return rows.ToList();
    }
}
