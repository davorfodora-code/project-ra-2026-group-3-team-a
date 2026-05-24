using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictMeetingDbRepository : IDistrictMeetingRepository
{
    public DistrictMeeting Create(DistrictMeeting meeting)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_meetings
                (district_id, meeting_type, meeting_time, date_range_start, date_range_end, status)
            VALUES
                (@districtId, @meetingType, @meetingTime, @dateRangeStart, @dateRangeEnd, @status)
            RETURNING id";
        DbHelper.AddParameter(cmd, "@districtId",    meeting.District.Id);
        DbHelper.AddParameter(cmd, "@meetingType",   meeting.MeetingType.ToString());
        DbHelper.AddParameter(cmd, "@meetingTime",   meeting.MeetingTime);
        DbHelper.AddParameter(cmd, "@dateRangeStart", meeting.DateRangeStart.Date);
        DbHelper.AddParameter(cmd, "@dateRangeEnd",  meeting.DateRangeEnd.Date);
        DbHelper.AddParameter(cmd, "@status",        meeting.Status.ToString());
        long id = Convert.ToInt64(cmd.ExecuteScalar());
        return new DistrictMeeting(id, meeting.District, meeting.MeetingType,
            meeting.MeetingTime, meeting.DateRangeStart, meeting.DateRangeEnd, meeting.Status, null);
    }

    public List<DistrictMeeting> GetByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = DistrictMeetingDbMapper.GetByCoordinatorQuery;
        DbHelper.AddParameter(cmd, "@coordinatorId", coordinatorId);
        List<DistrictMeeting> meetings = new List<DistrictMeeting>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) meetings.Add(DistrictMeetingDbMapper.MapMeeting(reader, coordinatorId));
        return meetings;
    }

    public Dictionary<DateTime, int> GetVoteResults(long meetingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT preferred_date, COUNT(*) AS vote_count
            FROM district_meeting_votes
            WHERE meeting_id = @meetingId
            GROUP BY preferred_date
            ORDER BY vote_count DESC, preferred_date";
        DbHelper.AddParameter(cmd, "@meetingId", meetingId);
        return DistrictMeetingDbMapper.ReadVoteResults(cmd);
    }

    public void UpdateStatus(long meetingId, MeetingStatus status, DateTime? scheduledDate)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_meetings
            SET status = @status, scheduled_date = @scheduledDate
            WHERE id = @meetingId";
        DbHelper.AddParameter(cmd, "@meetingId",     meetingId);
        DbHelper.AddParameter(cmd, "@status",        status.ToString());
        DbHelper.AddParameter(cmd, "@scheduledDate", (object?)scheduledDate?.Date ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }
}
