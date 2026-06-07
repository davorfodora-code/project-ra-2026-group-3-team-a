using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public partial class BudgetDbRepository
{
    public List<BudgetCategory> GetCategories()
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name FROM budget_categories ORDER BY id";
        var list = new List<BudgetCategory>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new BudgetCategory(Convert.ToInt64(r["id"]), r["name"].ToString()!));
        return list;
    }

    public List<BudgetCategoryDto> GetCategoryBudgets(long districtId, DateTime start, DateTime end)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT bc.id, bc.name,
                   COALESCE(SUM(CASE WHEN dd.donated_at BETWEEN @start AND @end THEN dd.amount ELSE 0 END), 0)         AS total_donations,
                   COALESCE(SUM(CASE WHEN de.expense_date BETWEEN @startDate AND @endDate THEN de.amount ELSE 0 END), 0) AS total_expenses
            FROM budget_categories bc
            LEFT JOIN district_donations dd ON dd.category_id = bc.id AND dd.district_id = @districtId
            LEFT JOIN district_expenses  de ON de.category_id = bc.id AND de.district_id = @districtId
            GROUP BY bc.id, bc.name
            ORDER BY bc.id";

        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@start",      start);
        AddParam(cmd, "@end",        end);
        AddParam(cmd, "@startDate",  DateOnly.FromDateTime(start));
        AddParam(cmd, "@endDate",    DateOnly.FromDateTime(end));

        var list = new List<BudgetCategoryDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new BudgetCategoryDto
            {
                Id             = Convert.ToInt64(r["id"]),
                Name           = r["name"].ToString()!,
                TotalDonations = Convert.ToDecimal(r["total_donations"]),
                TotalExpenses  = Convert.ToDecimal(r["total_expenses"])
            });
        return list;
    }
}
