using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public partial class BudgetDbRepository
{
    public List<ExpenseDto> GetExpenses(long districtId, long categoryId, DateTime start, DateTime end)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, amount, description, expense_date
            FROM district_expenses
            WHERE district_id = @districtId
              AND category_id = @categoryId
              AND expense_date BETWEEN @start AND @end
            ORDER BY expense_date DESC";

        AddParam(cmd, "@districtId", districtId);
        AddParam(cmd, "@categoryId", categoryId);
        AddParam(cmd, "@start",      DateOnly.FromDateTime(start));
        AddParam(cmd, "@end",        DateOnly.FromDateTime(end));

        var list = new List<ExpenseDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new ExpenseDto
            {
                Id          = Convert.ToInt64(r["id"]),
                Amount      = Convert.ToDecimal(r["amount"]),
                Description = r["description"].ToString()!,
                ExpenseDate = ((DateOnly)r["expense_date"]).ToDateTime(TimeOnly.MinValue)
            });
        return list;
    }

    public ExpenseDto? GetExpenseById(long expenseId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, amount, description, expense_date FROM district_expenses WHERE id = @id";
        AddParam(cmd, "@id", expenseId);
        using IDataReader r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new ExpenseDto
        {
            Id          = Convert.ToInt64(r["id"]),
            Amount      = Convert.ToDecimal(r["amount"]),
            Description = r["description"].ToString()!,
            ExpenseDate = ((DateOnly)r["expense_date"]).ToDateTime(TimeOnly.MinValue)
        };
    }

    public long AddExpense(long districtId, long categoryId, decimal amount, string description, DateTime expenseDate)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_expenses (district_id, category_id, amount, description, expense_date)
            VALUES (@districtId, @categoryId, @amount, @description, @expenseDate)
            RETURNING id";
        AddParam(cmd, "@districtId",  districtId);
        AddParam(cmd, "@categoryId",  categoryId);
        AddParam(cmd, "@amount",      amount);
        AddParam(cmd, "@description", description);
        AddParam(cmd, "@expenseDate", DateOnly.FromDateTime(expenseDate));
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public void UpdateExpense(long expenseId, decimal amount, string description, DateTime expenseDate)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_expenses
            SET amount = @amount, description = @description, expense_date = @expenseDate
            WHERE id = @id";
        AddParam(cmd, "@id",          expenseId);
        AddParam(cmd, "@amount",      amount);
        AddParam(cmd, "@description", description);
        AddParam(cmd, "@expenseDate", DateOnly.FromDateTime(expenseDate));
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
