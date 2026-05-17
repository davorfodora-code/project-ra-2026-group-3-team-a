using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenMeetingRepository
{
    List<CitizenMeetingDto> GetByDistrict(long districtId, long citizenId);
    CitizenMeetingDto? GetById(long meetingId, long citizenId);
    void UpsertVote(long meetingId, long citizenId, DateTime preferredDate);
}
