using System;
using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DistrictApprovalHelper
{
    internal static (long userId, long districtId) GetRequestUserAndDistrict(
        IDbConnection connection, long requestId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT user_id, district_id
            FROM district_access_requests
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);

        using IDataReader reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException("Zahtev nije pronadjen.");

        long userId     = reader.GetInt64(reader.GetOrdinal("user_id"));
        long districtId = reader.GetInt64(reader.GetOrdinal("district_id"));
        return (userId, districtId);
    }

    internal static void UpdateRequestStatus(IDbConnection connection, long requestId, string status)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_access_requests
            SET status = @status
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@status", status);
        cmd.ExecuteNonQuery();
    }

    internal static void InsertMembership(IDbConnection connection, long districtId, long userId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_memberships (district_id, user_id, join_date, trust_level)
            VALUES (@districtId, @userId, @joinDate, @trustLevel)";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@userId",     userId);
        AddParameter(cmd, "@joinDate",   DateTime.Now);
        AddParameter(cmd, "@trustLevel", CitizenTrustLevel.Nov.ToString());
        cmd.ExecuteNonQuery();
    }

    private static void AddParameter(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
