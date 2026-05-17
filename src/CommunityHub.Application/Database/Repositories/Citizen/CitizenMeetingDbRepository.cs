using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenMeetingDbRepository : ICitizenMeetingRepository
{
    public List<CitizenMeetingDto> GetByDistrict(long districtId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT m.id, m.district_id, m.meeting_type, m.meeting_time,
                   m.date_range_start, m.date_range_end, m.status, m.scheduled_date,
                   v.preferred_date AS my_preferred_date
            FROM district_meetings m
            LEFT JOIN district_meeting_votes v
                ON v.meeting_id = m.id AND v.citizen_id = @citizenId
            WHERE m.district_id = @districtId
            ORDER BY
                CASE m.status
                    WHEN 'UPripremi' THEN 0
                    WHEN 'Zakazan' THEN 1
                    ELSE 2
                END,
                m.date_range_start DESC";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@citizenId", citizenId);

        List<CitizenMeetingDto> meetings = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            meetings.Add(MapMeeting(reader));
        }

        return meetings;
    }

    public CitizenMeetingDto? GetById(long meetingId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT m.id, m.district_id, m.meeting_type, m.meeting_time,
                   m.date_range_start, m.date_range_end, m.status, m.scheduled_date,
                   v.preferred_date AS my_preferred_date
            FROM district_meetings m
            LEFT JOIN district_meeting_votes v
                ON v.meeting_id = m.id AND v.citizen_id = @citizenId
            WHERE m.id = @meetingId";

        AddParameter(cmd, "@meetingId", meetingId);
        AddParameter(cmd, "@citizenId", citizenId);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapMeeting(reader) : null;
    }

    public void UpsertVote(long meetingId, long citizenId, DateTime preferredDate)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_meeting_votes (meeting_id, citizen_id, preferred_date)
            VALUES (@meetingId, @citizenId, @preferredDate)
            ON CONFLICT (meeting_id, citizen_id)
            DO UPDATE SET preferred_date = EXCLUDED.preferred_date";

        AddParameter(cmd, "@meetingId", meetingId);
        AddParameter(cmd, "@citizenId", citizenId);
        AddParameter(cmd, "@preferredDate", preferredDate.Date);
        cmd.ExecuteNonQuery();
    }

    private static CitizenMeetingDto MapMeeting(IDataReader reader)
    {
        MeetingType meetingType = Enum.Parse<MeetingType>(
            reader.GetString(reader.GetOrdinal("meeting_type")));

        object rawTime = reader.GetValue(reader.GetOrdinal("meeting_time"));
        TimeSpan meetingTime = rawTime switch
        {
            TimeOnly t => t.ToTimeSpan(),
            TimeSpan ts => ts,
            _ => throw new InvalidCastException($"Neočekivani tip za meeting_time: {rawTime.GetType()}")
        };

        MeetingStatus status = Enum.Parse<MeetingStatus>(
            reader.GetString(reader.GetOrdinal("status")));

        int scheduledOrd = reader.GetOrdinal("scheduled_date");
        DateTime? scheduledDate = reader.IsDBNull(scheduledOrd)
            ? null
            : reader.GetDateTime(scheduledOrd);

        int myVoteOrd = reader.GetOrdinal("my_preferred_date");
        DateTime? myPreferredDate = reader.IsDBNull(myVoteOrd)
            ? null
            : reader.GetDateTime(myVoteOrd);

        return new CitizenMeetingDto
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            DistrictId = reader.GetInt64(reader.GetOrdinal("district_id")),
            MeetingType = meetingType,
            MeetingTime = meetingTime,
            DateRangeStart = reader.GetDateTime(reader.GetOrdinal("date_range_start")),
            DateRangeEnd = reader.GetDateTime(reader.GetOrdinal("date_range_end")),
            Status = status,
            ScheduledDate = scheduledDate,
            MyPreferredDate = myPreferredDate
        };
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
