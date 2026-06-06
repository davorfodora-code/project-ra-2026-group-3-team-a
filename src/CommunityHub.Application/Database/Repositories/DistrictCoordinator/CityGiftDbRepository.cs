using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class CityGiftDbRepository : ICityGiftRepository
{
    private static void AddParam(IDbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }

    public List<CityGiftDto> GetAllGifts(long coordinatorId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT g.id, g.name, g.amount, g.deadline, g.awarded, g.winner_district_id,
                   d.name AS winner_name,
                   EXISTS(
                       SELECT 1 FROM city_gift_registrations r
                       WHERE r.gift_id = g.id AND r.coordinator_id = @coordinatorId
                   ) AS has_registered
            FROM city_gifts g
            LEFT JOIN districts d ON d.id = g.winner_district_id
            ORDER BY g.awarded ASC, g.deadline ASC";
        AddParam(cmd, "@coordinatorId", coordinatorId);

        var list = new List<CityGiftDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new CityGiftDto
            {
                Id                 = Convert.ToInt64(r["id"]),
                Name               = r["name"].ToString() ?? string.Empty,
                Amount             = Convert.ToDecimal(r["amount"]),
                Deadline           = Convert.ToDateTime(r["deadline"]),
                Awarded            = Convert.ToBoolean(r["awarded"]),
                WinnerDistrictId   = r["winner_district_id"] is DBNull ? null : Convert.ToInt64(r["winner_district_id"]),
                WinnerDistrictName = r["winner_name"] is DBNull ? null : r["winner_name"].ToString(),
                HasRegistered      = Convert.ToBoolean(r["has_registered"])
            });
        return list;
    }

    public List<(long Id, decimal Amount)> GetPendingGifts()
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, amount FROM city_gifts
            WHERE awarded = FALSE AND deadline <= NOW()";

        var list = new List<(long, decimal)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add((Convert.ToInt64(r["id"]), Convert.ToDecimal(r["amount"])));
        return list;
    }

    public List<(long DistrictId, long CoordinatorId)> GetRegistrations(long giftId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT district_id, coordinator_id FROM city_gift_registrations
            WHERE gift_id = @giftId";
        AddParam(cmd, "@giftId", giftId);

        var list = new List<(long, long)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add((Convert.ToInt64(r["district_id"]), Convert.ToInt64(r["coordinator_id"])));
        return list;
    }

    public List<long> GetRegisteredDistrictIds(long giftId, long coordinatorId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT district_id FROM city_gift_registrations
            WHERE gift_id = @giftId AND coordinator_id = @coordinatorId";
        AddParam(cmd, "@giftId",        giftId);
        AddParam(cmd, "@coordinatorId", coordinatorId);

        var list = new List<long>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(Convert.ToInt64(r["district_id"]));
        return list;
    }

    public void Register(long giftId, long districtId, long coordinatorId, DateTime registeredAt)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO city_gift_registrations (gift_id, district_id, coordinator_id, registered_at)
            VALUES (@giftId, @districtId, @coordinatorId, @registeredAt)";
        AddParam(cmd, "@giftId",        giftId);
        AddParam(cmd, "@districtId",    districtId);
        AddParam(cmd, "@coordinatorId", coordinatorId);
        AddParam(cmd, "@registeredAt",  registeredAt);
        cmd.ExecuteNonQuery();
    }

    public decimal GetDistrictTotalBudget(long districtId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT
                COALESCE((SELECT SUM(amount) FROM district_donations WHERE district_id = @districtId), 0)
                - COALESCE((SELECT SUM(amount) FROM district_expenses WHERE district_id = @districtId), 0)";
        AddParam(cmd, "@districtId", districtId);
        return Convert.ToDecimal(cmd.ExecuteScalar()!);
    }

    public List<long> GetCategoryIds()
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id FROM budget_categories ORDER BY id";

        var list = new List<long>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(Convert.ToInt64(r["id"]));
        return list;
    }

    public void AddGiftDonation(long districtId, long categoryId, long coordinatorId, decimal amount, DateTime donatedAt)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_donations (district_id, category_id, citizen_id, amount, donated_at)
            VALUES (@districtId, @categoryId, @citizenId, @amount, @donatedAt)";
        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@categoryId", categoryId);
        AddParam(cmd, "@citizenId",  coordinatorId);
        AddParam(cmd, "@amount",     amount);
        AddParam(cmd, "@donatedAt",  donatedAt);
        cmd.ExecuteNonQuery();
    }

    public void MarkAwarded(long giftId, long? winnerDistrictId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE city_gifts SET awarded = TRUE, winner_district_id = @winnerId WHERE id = @giftId";
        AddParam(cmd, "@winnerId", (object?)winnerDistrictId ?? DBNull.Value);
        AddParam(cmd, "@giftId",   giftId);
        cmd.ExecuteNonQuery();
    }
}
