using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public partial class BudgetDbRepository
{
    public List<ExpenseDto> GetExpenses(long districtId, long categoryId, DateTime start, DateTime end)
    {
        string? category = BudgetCategoryCatalog.ResolveName(categoryId)
            ?? throw new ArgumentException($"Nepoznata kategorija: {categoryId}", nameof(categoryId));

        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, amount, description, spent_at
            FROM district_expenses
            WHERE district_id = @districtId
              AND category = @category
              AND spent_at BETWEEN @start AND @end
            ORDER BY spent_at DESC";

        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@category",   category);
        AddParam(cmd, "@start",      start);
        AddParam(cmd, "@end",        end);

        var list = new List<ExpenseDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new ExpenseDto
            {
                Id          = Convert.ToInt64(r["id"]),
                Amount      = Convert.ToDecimal(r["amount"]),
                Description = r["description"].ToString()!,
                ExpenseDate = Convert.ToDateTime(r["spent_at"])
            });
        return list;
    }

    public ExpenseDto? GetExpenseById(long expenseId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, amount, description, spent_at FROM district_expenses WHERE id = @id";
        AddParam(cmd, "@id", expenseId);
        using IDataReader r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new ExpenseDto
        {
            Id          = Convert.ToInt64(r["id"]),
            Amount      = Convert.ToDecimal(r["amount"]),
            Description = r["description"].ToString()!,
            ExpenseDate = Convert.ToDateTime(r["spent_at"])
        };
    }

    public long AddExpense(long districtId, long categoryId, decimal amount, string description, DateTime expenseDate)
    {
        string? category = BudgetCategoryCatalog.ResolveName(categoryId)
            ?? throw new ArgumentException($"Nepoznata kategorija: {categoryId}", nameof(categoryId));

        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_expenses (district_id, category, amount, description, spent_at)
            VALUES (@districtId, @category, @amount, @description, @expenseDate)
            RETURNING id";
        AddParam(cmd, "@districtId",  districtId);
        AddParam(cmd, "@category",   category);
        AddParam(cmd, "@amount",      amount);
        AddParam(cmd, "@description", description);
        AddParam(cmd, "@expenseDate", expenseDate);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public void UpdateExpense(long expenseId, decimal amount, string description, DateTime expenseDate)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_expenses
            SET amount = @amount, description = @description, spent_at = @expenseDate
            WHERE id = @id";
        AddParam(cmd, "@id",          expenseId);
        AddParam(cmd, "@amount",      amount);
        AddParam(cmd, "@description", description);
        AddParam(cmd, "@expenseDate", expenseDate);
        cmd.ExecuteNonQuery();
    }

    public void DeleteExpense(long expenseId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM district_expenses WHERE id = @id";
        AddParam(cmd, "@id", expenseId);
        cmd.ExecuteNonQuery();
    }
}
