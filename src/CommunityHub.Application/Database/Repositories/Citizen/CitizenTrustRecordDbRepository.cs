using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenTrustRecordDbRepository : ICitizenTrustRecordRepository
{
    public CitizenTrustRecordEntry? GetByUserAndDistrict(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                dm.user_id AS citizen_id,
                dm.district_id,
                CONCAT(u.name, ' ', u.surname) AS citizen_full_name,
                dm.join_date,
                dm.trust_level,
                (
                    SELECT COUNT(*)
                    FROM district_events e
                    WHERE e.district_id = dm.district_id
                      AND e.organizer_id = dm.user_id
                      AND e.status = 'Zavrsen'
                ) AS organized_completed_events_count,
                (
                    SELECT COUNT(*)
                    FROM district_event_volunteer_attendances va
                    JOIN district_events e ON e.id = va.event_id
                    WHERE e.district_id = dm.district_id
                      AND va.volunteer_id = dm.user_id
                      AND va.attended = TRUE
                ) AS confirmed_volunteer_events_count
            FROM district_memberships dm
            JOIN users u ON u.id = dm.user_id
            WHERE dm.user_id = @userId
              AND dm.district_id = @districtId
            ORDER BY dm.join_date DESC
            LIMIT 1";

        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@districtId", districtId);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapEntry(reader) : null;
    }

    public List<CitizenTrustRecordEntry> GetByDistrict(long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                dm.user_id AS citizen_id,
                dm.district_id,
                CONCAT(u.name, ' ', u.surname) AS citizen_full_name,
                dm.join_date,
                dm.trust_level,
                (
                    SELECT COUNT(*)
                    FROM district_events e
                    WHERE e.district_id = dm.district_id
                      AND e.organizer_id = dm.user_id
                      AND e.status = 'Zavrsen'
                ) AS organized_completed_events_count,
                (
                    SELECT COUNT(*)
                    FROM district_event_volunteer_attendances va
                    JOIN district_events e ON e.id = va.event_id
                    WHERE e.district_id = dm.district_id
                      AND va.volunteer_id = dm.user_id
                      AND va.attended = TRUE
                ) AS confirmed_volunteer_events_count
            FROM district_memberships dm
            JOIN users u ON u.id = dm.user_id
            WHERE dm.district_id = @districtId
            ORDER BY u.name, u.surname";

        AddParameter(cmd, "@districtId", districtId);

        List<CitizenTrustRecordEntry> entries = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(MapEntry(reader));
        }

        return entries;
    }

    public void UpdateTrustLevel(long userId, long districtId, CitizenTrustLevel trustLevel)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_memberships
            SET trust_level = @trustLevel
            WHERE user_id = @userId
              AND district_id = @districtId";

        AddParameter(cmd, "@trustLevel", trustLevel.ToString());
        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@districtId", districtId);
        cmd.ExecuteNonQuery();
    }

    private static CitizenTrustRecordEntry MapEntry(IDataReader reader)
    {
        return new CitizenTrustRecordEntry
        {
            CitizenId = reader.GetInt64(reader.GetOrdinal("citizen_id")),
            DistrictId = reader.GetInt64(reader.GetOrdinal("district_id")),
            CitizenFullName = reader.GetString(reader.GetOrdinal("citizen_full_name")).Trim(),
            JoinDate = reader.GetDateTime(reader.GetOrdinal("join_date")),
            TrustLevel = Enum.Parse<CitizenTrustLevel>(reader.GetString(reader.GetOrdinal("trust_level"))),
            OrganizedCompletedEventsCount = reader.GetInt32(reader.GetOrdinal("organized_completed_events_count")),
            ConfirmedVolunteerEventsCount = reader.GetInt32(reader.GetOrdinal("confirmed_volunteer_events_count"))
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
