using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class VolunteerSignupViewModel
{
    public VolunteerSignupDto BuildDto(long eventId, IEnumerable<long> selectedItemIds)
    {
        return new VolunteerSignupDto
        {
            EventId = eventId,
            SelectedItemIds = selectedItemIds.Distinct().ToList()
        };
    }
}
