using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenMeetingService
{
    List<CitizenMeetingDto> GetMeetings(long citizenId);
    MeetingVoteResultDto CastOrUpdateVote(long citizenId, CastMeetingVoteDto dto);
}
