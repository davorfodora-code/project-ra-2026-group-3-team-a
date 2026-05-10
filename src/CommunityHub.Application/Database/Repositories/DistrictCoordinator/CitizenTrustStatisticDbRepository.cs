using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Dto.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class CitizenTrustStatisticDbRepository
{
    
    public List<CitizenTrustStatisticDto> GetByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT d.id AS district_id, d.name AS district_name,
                   COUNT(*) FILTER (WHERE m.trust_level = 'Nov')          AS nov_count,
                   COUNT(*) FILTER (WHERE m.trust_level = 'Neaktivan')    AS neaktivan_count,
                   COUNT(*) FILTER (WHERE m.trust_level = 'Aktivan')      AS aktivan_count,
                   COUNT(*) FILTER (WHERE m.trust_level = 'Istaknuti')    AS istaknuti_count,
                   COUNT(*) FILTER (WHERE m.trust_level = 'OdPoverenja')  AS od_poverenja_count
            FROM districts d
            LEFT JOIN district_memberships m ON m.district_id = d.id
            WHERE d.coordinator_id = @coordinatorId
            GROUP BY d.id, d.name
            ORDER BY d.name";

        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<CitizenTrustStatisticDto> result = new List<CitizenTrustStatisticDto>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(MapRow(reader));
        }

        return result;
    }

    private static CitizenTrustStatisticDto MapRow(IDataReader reader)
    {
        return new CitizenTrustStatisticDto
        {
            DistrictId       = reader.GetInt64(reader.GetOrdinal("district_id")),
            DistrictName     = reader.GetString(reader.GetOrdinal("district_name")),
            NovCount         = (int)reader.GetInt64(reader.GetOrdinal("nov_count")),
            NeaktivanCount   = (int)reader.GetInt64(reader.GetOrdinal("neaktivan_count")),
            AktivanCount     = (int)reader.GetInt64(reader.GetOrdinal("aktivan_count")),
            IstaknutiCount   = (int)reader.GetInt64(reader.GetOrdinal("istaknuti_count")),
            OdPoverenjaCount = (int)reader.GetInt64(reader.GetOrdinal("od_poverenja_count"))
        };
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
