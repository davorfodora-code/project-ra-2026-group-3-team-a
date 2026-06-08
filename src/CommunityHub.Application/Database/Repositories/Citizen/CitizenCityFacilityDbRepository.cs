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

        cmd.CommandText = CitizenCityFacilitySql.FacilitiesByDistrict;
        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@citizenId", citizenId);

        return ReadList(cmd, MapFacility);
    }

    public CityFacilityListItemDto? GetFacilityById(long facilityId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = CitizenCityFacilitySql.FacilityById;
        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@citizenId", citizenId);
        AddParameter(cmd, "@districtId", 0L);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapFacility(reader) : null;
    }

    public void Vote(long facilityId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = CitizenCityFacilitySql.Vote;
        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@citizenId", citizenId);
        AddParameter(cmd, "@votedAt", DateTime.Now);

        cmd.ExecuteNonQuery();
    }

    public bool HasVote(long facilityId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = CitizenCityFacilitySql.HasVote;
        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@citizenId", citizenId);

        return cmd.ExecuteScalar() != null;
    }

    public bool FacilityBelongsToDistrict(long facilityId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = CitizenCityFacilitySql.FacilityBelongsToDistrict;
        AddParameter(cmd, "@facilityId", facilityId);
        AddParameter(cmd, "@districtId", districtId);

        return cmd.ExecuteScalar() != null;
    }

    public CityFacilityStatisticsDto GetStatistics(long districtId, int year, int? month)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = CitizenCityFacilitySql.StatisticsBase;
        if (month.HasValue)
        {
            cmd.CommandText += CitizenCityFacilitySql.StatisticsMonthFilter;
            AddParameter(cmd, "@month", month.Value);
        }

        cmd.CommandText += CitizenCityFacilitySql.StatisticsGroupAndOrder;
        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@year", year);

        return MapStatistics(ReadList(cmd, MapVisitStat));
    }

    public List<CityFacilityReservationHistoryDto> GetReservationHistory(long districtId, long facilityId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = CitizenCityFacilitySql.ReservationHistory;
        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@facilityId", facilityId);

        return ReadList(cmd, MapReservationHistory);
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

    private static CityFacilityStatisticsDto MapStatistics(List<CityFacilityVisitStatDto> visitStats)
    {
        return new CityFacilityStatisticsDto
        {
            TotalReservations = visitStats.Sum(stat => stat.VisitCount),
            FacilityStats = visitStats
        };
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

    private static CityFacilityVisitStatDto MapVisitStat(IDataReader reader)
    {
        return new CityFacilityVisitStatDto
        {
            FacilityId = reader.GetInt64(reader.GetOrdinal("facility_id")),
            FacilityName = reader.GetString(reader.GetOrdinal("facility_name")),
            VisitCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("visit_count")))
        };
    }

    private static CityFacilityReservationHistoryDto MapReservationHistory(IDataReader reader)
    {
        return new CityFacilityReservationHistoryDto
        {
            FacilityId = reader.GetInt64(reader.GetOrdinal("facility_id")),
            FacilityName = reader.GetString(reader.GetOrdinal("facility_name")),
            ReservedAt = reader.GetDateTime(reader.GetOrdinal("reserved_at")),
            Note = reader.GetString(reader.GetOrdinal("note"))
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