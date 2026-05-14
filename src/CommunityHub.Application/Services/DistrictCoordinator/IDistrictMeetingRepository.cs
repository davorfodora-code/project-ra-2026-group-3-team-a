using System;
using System.Collections.Generic;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface IDistrictMeetingRepository
{
    DistrictMeeting Create(DistrictMeeting meeting);
    List<DistrictMeeting> GetByCoordinatorId(long coordinatorId);
    Dictionary<DateTime, int> GetVoteResults(long meetingId);
    void UpdateStatus(long meetingId, MeetingStatus status, DateTime? scheduledDate);
}
