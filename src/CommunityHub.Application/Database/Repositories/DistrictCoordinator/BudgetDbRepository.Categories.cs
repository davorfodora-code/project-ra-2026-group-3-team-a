using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public partial class BudgetDbRepository
{
    public List<BudgetCategory> GetCategories() => BudgetCategoryCatalog.All.ToList();

    public List<BudgetCategoryDto> GetCategoryBudgets(long districtId, DateTime start, DateTime end)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT bc.id, bc.name,
                   COALESCE(SUM(CASE WHEN dd.donated_at BETWEEN @start AND @end THEN dd.amount ELSE 0 END), 0) AS total_donations,
                   COALESCE(SUM(CASE WHEN de.spent_at BETWEEN @start AND @end THEN de.amount ELSE 0 END), 0) AS total_expenses
            FROM ({BudgetCategoryCatalog.CategoriesSqlValues}) bc
            LEFT JOIN district_donations dd ON dd.category = bc.name AND dd.district_id = @districtId
            LEFT JOIN district_expenses  de ON de.category = bc.name AND de.district_id = @districtId
            GROUP BY bc.id, bc.name
            ORDER BY bc.id";

        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@start",      start);
        AddParam(cmd, "@end",        end);

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
