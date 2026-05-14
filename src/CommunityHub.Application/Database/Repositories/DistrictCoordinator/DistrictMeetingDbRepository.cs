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

        AddParameter(cmd, "@districtId",     meeting.District.Id);
        AddParameter(cmd, "@meetingType",     meeting.MeetingType.ToString());
        AddParameter(cmd, "@meetingTime",     meeting.MeetingTime);
        AddParameter(cmd, "@dateRangeStart",  meeting.DateRangeStart.Date);
        AddParameter(cmd, "@dateRangeEnd",    meeting.DateRangeEnd.Date);
        AddParameter(cmd, "@status",          meeting.Status.ToString());

        long id = Convert.ToInt64(cmd.ExecuteScalar());
        return new DistrictMeeting(id, meeting.District, meeting.MeetingType,
            meeting.MeetingTime, meeting.DateRangeStart, meeting.DateRangeEnd,
            meeting.Status, null);
    }

    public List<DistrictMeeting> GetByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT m.id, m.meeting_type, m.meeting_time, m.date_range_start,
                   m.date_range_end, m.status, m.scheduled_date,
                   d.id AS district_id, d.name AS district_name,
                   d.description, d.budget,
                   l.id AS location_id, l.city, l.country,
                   COALESCE(v.vote_count, 0) AS vote_count
            FROM district_meetings m
            JOIN districts d ON d.id = m.district_id
            JOIN locations l ON l.id = d.location_id
            LEFT JOIN (
                SELECT meeting_id, COUNT(*) AS vote_count
                FROM district_meeting_votes
                GROUP BY meeting_id
            ) v ON v.meeting_id = m.id
            WHERE d.coordinator_id = @coordinatorId
            ORDER BY m.date_range_start DESC";

        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<DistrictMeeting> meetings = new List<DistrictMeeting>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            meetings.Add(MapMeeting(reader, coordinatorId));
        }

        return meetings;
    }

    // Vraća datume glasova za dati sastanak — key: datum, value: broj glasova
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

        AddParameter(cmd, "@meetingId", meetingId);

        Dictionary<DateTime, int> results = new Dictionary<DateTime, int>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            DateTime date  = reader.GetDateTime(reader.GetOrdinal("preferred_date"));
            int voteCount  = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("vote_count")));
            results[date]  = voteCount;
        }

        return results;
    }

    public void UpdateStatus(long meetingId, MeetingStatus status, DateTime? scheduledDate)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_meetings
            SET status = @status,
                scheduled_date = @scheduledDate
            WHERE id = @meetingId";

        AddParameter(cmd, "@meetingId",     meetingId);
        AddParameter(cmd, "@status",        status.ToString());
        AddParameter(cmd, "@scheduledDate", (object?)scheduledDate?.Date ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private static DistrictMeeting MapMeeting(IDataReader reader, long coordinatorId)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

        User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        District district = new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);

        MeetingType meetingType = Enum.Parse<MeetingType>(
            reader.GetString(reader.GetOrdinal("meeting_type")));

        object rawTime = reader.GetValue(reader.GetOrdinal("meeting_time"));
        TimeSpan meetingTime = rawTime switch
        {
            TimeOnly t  => t.ToTimeSpan(),
            TimeSpan ts => ts,
            _           => throw new InvalidCastException($"Neočekivani tip za meeting_time: {rawTime.GetType()}")
        };

        MeetingStatus status = Enum.Parse<MeetingStatus>(
            reader.GetString(reader.GetOrdinal("status")));

        int scheduledOrd = reader.GetOrdinal("scheduled_date");
        DateTime? scheduledDate = reader.IsDBNull(scheduledOrd)
            ? null
            : reader.GetDateTime(scheduledOrd);

        int voteCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("vote_count")));

        return new DistrictMeeting(
            reader.GetInt64(reader.GetOrdinal("id")),
            district, meetingType, meetingTime,
            reader.GetDateTime(reader.GetOrdinal("date_range_start")),
            reader.GetDateTime(reader.GetOrdinal("date_range_end")),
            status, scheduledDate, voteCount);
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
