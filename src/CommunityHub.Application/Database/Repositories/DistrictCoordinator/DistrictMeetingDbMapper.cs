using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DistrictMeetingDbMapper
{
    internal const string GetByCoordinatorQuery = @"
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

    internal static Dictionary<DateTime, int> ReadVoteResults(IDbCommand cmd)
    {
        Dictionary<DateTime, int> results = new Dictionary<DateTime, int>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            DateTime date = reader.GetDateTime(reader.GetOrdinal("preferred_date"));
            int count     = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("vote_count")));
            results[date] = count;
        }
        return results;
    }

    internal static Location MapLocation(IDataReader reader) =>
        new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

    internal static District MapDistrict(IDataReader reader, long coordinatorId)
    {
        User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");
        return new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("description")),
            MapLocation(reader),
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);
    }

    internal static TimeSpan ParseMeetingTime(IDataReader reader)
    {
        object rawTime = reader.GetValue(reader.GetOrdinal("meeting_time"));
        return rawTime switch
        {
            TimeOnly t  => t.ToTimeSpan(),
            TimeSpan ts => ts,
            _           => throw new InvalidCastException($"Neočekivani tip za meeting_time: {rawTime.GetType()}")
        };
    }

    internal static DateTime? ReadScheduledDate(IDataReader reader)
    {
        int ord = reader.GetOrdinal("scheduled_date");
        return reader.IsDBNull(ord) ? null : reader.GetDateTime(ord);
    }

    internal static DistrictMeeting MapMeeting(IDataReader reader, long coordinatorId)
    {
        District district    = MapDistrict(reader, coordinatorId);
        MeetingType type     = Enum.Parse<MeetingType>(reader.GetString(reader.GetOrdinal("meeting_type")));
        TimeSpan meetingTime = ParseMeetingTime(reader);
        MeetingStatus status = Enum.Parse<MeetingStatus>(reader.GetString(reader.GetOrdinal("status")));
        DateTime? scheduled  = ReadScheduledDate(reader);
        int voteCount        = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("vote_count")));

        return new DistrictMeeting(
            reader.GetInt64(reader.GetOrdinal("id")),
            district, type, meetingTime,
            reader.GetDateTime(reader.GetOrdinal("date_range_start")),
            reader.GetDateTime(reader.GetOrdinal("date_range_end")),
            status, scheduled, voteCount);
    }
}
