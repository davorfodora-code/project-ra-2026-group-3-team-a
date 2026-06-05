using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public partial class BudgetDbRepository
{
    public List<DonationDto> GetDonations(long districtId, long categoryId, DateTime start, DateTime end)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT amount, donated_at
            FROM district_donations
            WHERE district_id = @districtId
              AND category_id = @categoryId
              AND donated_at BETWEEN @start AND @end
            ORDER BY donated_at DESC";

        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@categoryId", categoryId);
        AddParam(cmd, "@start",      start);
        AddParam(cmd, "@end",        end);

        var list = new List<DonationDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new DonationDto
            {
                Amount    = Convert.ToDecimal(r["amount"]),
                DonatedAt = Convert.ToDateTime(r["donated_at"])
            });
        return list;
    }

    public void AddDonation(long districtId, long categoryId, long citizenId, decimal amount, DateTime donatedAt)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_donations (district_id, category_id, citizen_id, amount, donated_at)
            VALUES (@districtId, @categoryId, @citizenId, @amount, @donatedAt)";
        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@categoryId", categoryId);
        AddParam(cmd, "@citizenId",  citizenId);
        AddParam(cmd, "@amount",     amount);
        AddParam(cmd, "@donatedAt",  DateTime.SpecifyKind(donatedAt, DateTimeKind.Unspecified));
        cmd.ExecuteNonQuery();
    }
}
