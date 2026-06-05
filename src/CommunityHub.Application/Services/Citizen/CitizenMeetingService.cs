using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenMeetingService : ICitizenMeetingService
{
    private readonly ICitizenMeetingRepository _meetingRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;

    public CitizenMeetingService(
        ICitizenMeetingRepository meetingRepository,
        ICitizenDistrictMembershipRepository membershipRepository)
    {
        _meetingRepository = meetingRepository;
        _membershipRepository = membershipRepository;
    }

    public List<CitizenMeetingDto> GetMeetings(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return new List<CitizenMeetingDto>();
        }

        return _meetingRepository.GetByDistrict(districtId.Value, citizenId);
    }

    public MeetingVoteResultDto CastOrUpdateVote(long citizenId, CastMeetingVoteDto dto)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return Fail("Niste član kvarta.");
        }

        CitizenMeetingDto? meeting = _meetingRepository.GetById(dto.MeetingId, citizenId);
        if (meeting == null)
        {
            return Fail("Sastanak nije pronađen.");
        }

        if (meeting.DistrictId != districtId.Value)
        {
            return Fail("Sastanak ne pripada vašem kvartu.");
        }

        if (meeting.Status != MeetingStatus.UPripremi)
        {
            return Fail("Glasanje je moguće samo za sastanke u pripremi.");
        }

        if (!meeting.CanVote)
        {
            return Fail("Glasanje je zatvoreno (24h pre početka opsega).");
        }

        DateTime preferredDate = dto.PreferredDate.Date;
        if (preferredDate < meeting.DateRangeStart.Date || preferredDate > meeting.DateRangeEnd.Date)
        {
            return Fail("Izabrani datum nije u ponuđenom opsegu.");
        }

        _meetingRepository.UpsertVote(dto.MeetingId, citizenId, preferredDate);
        return new MeetingVoteResultDto
        {
            Success = true,
            Message = "Vaš glas je sačuvan."
        };
    }

    private static MeetingVoteResultDto Fail(string message) =>
        new() { Success = false, Message = message };
}
