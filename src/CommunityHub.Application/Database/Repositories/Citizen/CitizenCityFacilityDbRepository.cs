using System.Data;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenCityFacilityDbRepository : ICitizenCityFacilityRepository
{
    public List<CityFacilityListItemDto> GetFacilitiesByDistrict(long districtId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT
                f.id,
                f.district_id,
                f.name,
                f.description,
                f.address,
                COALESCE(v.vote_count, 0) AS vote_count,
                CASE WHEN my_vote.id IS NULL THEN FALSE ELSE TRUE END AS has_my_vote,
                last_res.last_reserved_at
            FROM city_facilities f
            LEFT JOIN (
                SELECT facility_id, COUNT(*) AS vote_count
                FROM city_facility_interest_votes
                GROUP BY facility_id
            ) v ON v.facility_id = f.id
            LEFT JOIN city_facility_interest_votes my_vote
                ON my_vote.facility_id = f.id AND my_vote.citizen_id = @citizenId
            LEFT JOIN (
                SELECT facility_id, MAX(reserved_at) AS last_reserved_at
                FROM city_facility_reservations
                GROUP BY facility_id
            ) last_res ON last_res.facility_id = f.id
            WHERE f.district_id = @districtId
            ORDER BY vote_count DESC, f.name";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@citizenId", citizenId);

        List<CityFacilityListItemDto> facilities = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            facilities.Add(MapFacility(reader));
        }

        return facilities;
    }

    public CityFacilityListItemDto? GetFacilityById(long facilityId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT
                f.id,
                f.district_id,
                f.name,
                f.description,
                f.address,
                COALESCE(v.vote_count, 0) AS vote_count,
                CASE WHEN my_vote.id IS NULL THEN FALSE ELSE TRUE END AS has_my_vote,
                last_res.last_reserved_at
            FROM city_facilities f
            LEFT JOIN (
                SELECT facility_id, COUNT(*) AS vote_count
                FROM city_facility_interest_votes
                GROUP BY facility_id
            ) v ON v.facility_id = f.id
            LEFT JOIN city_facility_interest_votes my_vote
                ON my_vote.facility_id = f.id AND my_vote.citizen_id = @citizenId
            LEFT JOIN (
                SELECT facility_id, MAX(reserved_at) AS last_reserved_at
                FROM city_facility_reservations
                GROUP BY facility_id
            ) last_res ON last_res.facility_id = f.id
            WHERE f.id = @facilityId";

        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@citizenId", citizenId);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapFacility(reader) : null;
    }

    public void Vote(long facilityId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO city_facility_interest_votes (facility_id, citizen_id, voted_at)
            VALUES (@facilityId, @citizenId, @votedAt)
            ON CONFLICT (facility_id, citizen_id) DO NOTHING";

        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@citizenId", citizenId);
        AddParameter(cmd, "@votedAt", DateTime.Now);

        cmd.ExecuteNonQuery();
    }

    public bool HasVote(long facilityId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT 1
            FROM city_facility_interest_votes
            WHERE facility_id = @facilityId AND citizen_id = @citizenId
            LIMIT 1";

        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@citizenId", citizenId);

        return cmd.ExecuteScalar() != null;
    }

    public bool FacilityBelongsToDistrict(long facilityId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT 1
            FROM city_facilities
            WHERE id = @facilityId AND district_id = @districtId
            LIMIT 1";

        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@districtId", districtId);

        return cmd.ExecuteScalar() != null;
    }

    public CityFacilityStatisticsDto GetStatistics(long districtId, int year, int? month)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT
                f.id AS facility_id,
                f.name AS facility_name,
                COUNT(r.id) AS visit_count
            FROM city_facilities f
            JOIN city_facility_reservations r ON r.facility_id = f.id
            WHERE r.district_id = @districtId
              AND EXTRACT(YEAR FROM r.reserved_at) = @year";

        if (month.HasValue)
        {
            cmd.CommandText += " AND EXTRACT(MONTH FROM r.reserved_at) = @month";
            AddParameter(cmd, "@month", month.Value);
        }

        cmd.CommandText += @"
            GROUP BY f.id, f.name
            ORDER BY visit_count DESC, f.name";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@year", year);

        CityFacilityStatisticsDto stats = new();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int count = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("visit_count")));
            stats.TotalReservations += count;

            stats.FacilityStats.Add(new CityFacilityVisitStatDto
            {
                FacilityId = reader.GetInt64(reader.GetOrdinal("facility_id")),
                FacilityName = reader.GetString(reader.GetOrdinal("facility_name")),
                VisitCount = count
            });
        }

        return stats;
    }

    public List<CityFacilityReservationHistoryDto> GetReservationHistory(long districtId, long facilityId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT
                f.id AS facility_id,
                f.name AS facility_name,
                r.reserved_at,
                COALESCE(r.note, '') AS note
            FROM city_facility_reservations r
            JOIN city_facilities f ON f.id = r.facility_id
            WHERE r.district_id = @districtId
              AND r.facility_id = @facilityId
            ORDER BY r.reserved_at DESC";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@facilityId", facilityId);

        List<CityFacilityReservationHistoryDto> history = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            history.Add(new CityFacilityReservationHistoryDto
            {
                FacilityId = reader.GetInt64(reader.GetOrdinal("facility_id")),
                FacilityName = reader.GetString(reader.GetOrdinal("facility_name")),
                ReservedAt = reader.GetDateTime(reader.GetOrdinal("reserved_at")),
                Note = reader.GetString(reader.GetOrdinal("note"))
            });
        }

        return history;
    }

    private static CityFacilityListItemDto MapFacility(IDataReader reader)
    {
        int lastReservationOrdinal = reader.GetOrdinal("last_reserved_at");

        return new CityFacilityListItemDto
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            DistrictId = reader.GetInt64(reader.GetOrdinal("district_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            Address = reader.GetString(reader.GetOrdinal("address")),
            VoteCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("vote_count"))),
            HasMyVote = reader.GetBoolean(reader.GetOrdinal("has_my_vote")),
            LastReservationDate = reader.IsDBNull(lastReservationOrdinal)
                ? null
                : reader.GetDateTime(lastReservationOrdinal)
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