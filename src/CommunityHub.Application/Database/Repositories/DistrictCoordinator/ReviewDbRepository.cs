using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class ReviewDbRepository : IReviewRepository
{
    public List<DistrictReview> GetByDistrictId(long districtId, DateTime since)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, district_id, rating, justification AS comment, created_at
            FROM district_coordinator_reviews
            WHERE district_id = @districtId
              AND created_at  >= @since
              AND status = 'Active'
            ORDER BY created_at DESC";

        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@since",      since);

        var list = new List<DistrictReview>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new DistrictReview(
                id:         Convert.ToInt64(r["id"]),
                districtId: Convert.ToInt64(r["district_id"]),
                rating:     Convert.ToInt32(r["rating"]),
                comment:    r["comment"] == DBNull.Value ? null : r["comment"].ToString(),
                createdAt:  Convert.ToDateTime(r["created_at"])));
        }
        return list;
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
