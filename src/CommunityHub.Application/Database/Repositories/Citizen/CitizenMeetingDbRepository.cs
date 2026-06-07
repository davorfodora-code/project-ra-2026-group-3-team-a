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
        cmd.CommandText = MeetingsByDistrictQuery;

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@citizenId", citizenId);

        return ReadList(cmd, MapMeeting);
    }

    public CitizenMeetingDto? GetById(long meetingId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = MeetingByIdQuery;

        AddParameter(cmd, "@meetingId", meetingId);
        AddParameter(cmd, "@citizenId", citizenId);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapMeeting(reader) : null;
    }

    public void UpsertVote(long meetingId, long citizenId, DateTime preferredDate)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = UpsertVoteCommandText;

        AddParameter(cmd, "@meetingId", meetingId);
        AddParameter(cmd, "@citizenId", citizenId);
        AddParameter(cmd, "@preferredDate", preferredDate.Date);
        cmd.ExecuteNonQuery();
    }

    private static CitizenMeetingDto MapMeeting(IDataReader reader)
    {
        return new CitizenMeetingDto
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            DistrictId = reader.GetInt64(reader.GetOrdinal("district_id")),
            MeetingType = ReadEnum<MeetingType>(reader, "meeting_type"),
            MeetingTime = ReadMeetingTime(reader),
            DateRangeStart = reader.GetDateTime(reader.GetOrdinal("date_range_start")),
            DateRangeEnd = reader.GetDateTime(reader.GetOrdinal("date_range_end")),
            Status = ReadEnum<MeetingStatus>(reader, "status"),
            ScheduledDate = ReadNullableDateTime(reader, "scheduled_date"),
            MyPreferredDate = ReadNullableDateTime(reader, "my_preferred_date")
        };
    }

    private static List<T> ReadList<T>(IDbCommand command, Func<IDataReader, T> map)
    {
        List<T> items = new();
        using IDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(map(reader));
        }

        return items;
    }

    private static T ReadEnum<T>(IDataReader reader, string columnName)
        where T : struct
    {
        return Enum.Parse<T>(reader.GetString(reader.GetOrdinal(columnName)));
    }

    private static TimeSpan ReadMeetingTime(IDataReader reader)
    {
        object rawTime = reader.GetValue(reader.GetOrdinal("meeting_time"));
        return rawTime switch
        {
            TimeOnly time => time.ToTimeSpan(),
            TimeSpan timeSpan => timeSpan,
            _ => throw new InvalidCastException($"Neočekivani tip za meeting_time: {rawTime.GetType()}")
        };
    }

    private static DateTime? ReadNullableDateTime(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }

    private const string MeetingSelect = @"
            SELECT m.id, m.district_id, m.meeting_type, m.meeting_time,
                   m.date_range_start, m.date_range_end, m.status, m.scheduled_date,
                   v.preferred_date AS my_preferred_date
            FROM district_meetings m
            LEFT JOIN district_meeting_votes v
                ON v.meeting_id = m.id AND v.citizen_id = @citizenId";

    private const string MeetingsByDistrictQuery = MeetingSelect + @"
            WHERE m.district_id = @districtId
            ORDER BY
                CASE m.status
                    WHEN 'UPripremi' THEN 0
                    WHEN 'Zakazan' THEN 1
                    ELSE 2
                END,
                m.date_range_start DESC";

    private const string MeetingByIdQuery = MeetingSelect + @"
            WHERE m.id = @meetingId";

    private const string UpsertVoteCommandText = @"
            INSERT INTO district_meeting_votes (meeting_id, citizen_id, preferred_date)
            VALUES (@meetingId, @citizenId, @preferredDate)
            ON CONFLICT (meeting_id, citizen_id)
            DO UPDATE SET preferred_date = EXCLUDED.preferred_date";
}
