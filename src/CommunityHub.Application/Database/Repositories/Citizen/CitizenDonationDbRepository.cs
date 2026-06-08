using System.Data;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDonationDbRepository : ICitizenDonationRepository
{
    public DonationDashboardDto GetDashboard(long districtId, long citizenId, IReadOnlyCollection<string> categories)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        DonationDashboardDto dashboard = GetDistrictBudget(connection, districtId);
        Dictionary<string, decimal> donatedByCategory = GetDonatedByCategory(connection, districtId);
        Dictionary<string, decimal> spentByCategory = GetSpentByCategory(connection, districtId);

        dashboard.CategoryBudgets = categories
            .Select(category => new DonationBudgetCategoryDto
            {
                Category = category,
                DonatedAmount = donatedByCategory.GetValueOrDefault(category),
                SpentAmount = spentByCategory.GetValueOrDefault(category)
            })
            .ToList();

        dashboard.ExpenseHistory = GetExpenseHistory(connection, districtId);
        dashboard.MyDonations = GetMyDonations(connection, districtId, citizenId);

        return dashboard;
    }

    public void CreateDonation(long districtId, long citizenId, decimal amount, string category, DateTime donatedAt)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            IDbCommand insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
                INSERT INTO district_donations
                    (district_id, citizen_id, category, amount, donated_at)
                VALUES
                    (@districtId, @citizenId, @category, @amount, @donatedAt)";

            AddParameter(insertCommand, "@districtId", districtId);
            AddParameter(insertCommand, "@citizenId", citizenId);
            AddParameter(insertCommand, "@category", category);
            AddParameter(insertCommand, "@amount", amount);
            AddParameter(insertCommand, "@donatedAt", donatedAt);
            insertCommand.ExecuteNonQuery();

            IDbCommand updateBudgetCommand = connection.CreateCommand();
            updateBudgetCommand.Transaction = transaction;
            updateBudgetCommand.CommandText = @"
                UPDATE districts
                SET budget = budget + @amount
                WHERE id = @districtId";

            AddParameter(updateBudgetCommand, "@amount", amount);
            AddParameter(updateBudgetCommand, "@districtId", districtId);
            updateBudgetCommand.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static DonationDashboardDto GetDistrictBudget(IDbConnection connection, long districtId)
    {
        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT name, budget
            FROM districts
            WHERE id = @districtId";

        AddParameter(command, "@districtId", districtId);

        using IDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new DonationDashboardDto();
        }

        return new DonationDashboardDto
        {
            DistrictName = reader.GetString(reader.GetOrdinal("name")),
            TotalBudget = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("budget")))
        };
    }

    private static Dictionary<string, decimal> GetDonatedByCategory(IDbConnection connection, long districtId)
    {
        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT category, SUM(amount) AS total_amount
            FROM district_donations
            WHERE district_id = @districtId
            GROUP BY category";

        AddParameter(command, "@districtId", districtId);

        Dictionary<string, decimal> result = new();
        using IDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result[reader.GetString(reader.GetOrdinal("category"))] =
                Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("total_amount")));
        }

        return result;
    }

    private static Dictionary<string, decimal> GetSpentByCategory(IDbConnection connection, long districtId)
    {
        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT category, SUM(amount) AS total_amount
            FROM district_expenses
            WHERE district_id = @districtId
            GROUP BY category";

        AddParameter(command, "@districtId", districtId);

        Dictionary<string, decimal> result = new();
        using IDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result[reader.GetString(reader.GetOrdinal("category"))] =
                Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("total_amount")));
        }

        return result;
    }

    private static List<DonationExpenseHistoryDto> GetExpenseHistory(IDbConnection connection, long districtId)
    {
        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, category, amount, description, spent_at
            FROM district_expenses
            WHERE district_id = @districtId
            ORDER BY spent_at DESC";

        AddParameter(command, "@districtId", districtId);

        List<DonationExpenseHistoryDto> expenses = new();
        using IDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            expenses.Add(new DonationExpenseHistoryDto
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                Amount = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("amount"))),
                Description = reader.GetString(reader.GetOrdinal("description")),
                SpentAt = reader.GetDateTime(reader.GetOrdinal("spent_at"))
            });
        }

        return expenses;
    }

    private static List<DonationHistoryDto> GetMyDonations(IDbConnection connection, long districtId, long citizenId)
    {
        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, category, amount, donated_at
            FROM district_donations
            WHERE district_id = @districtId
              AND citizen_id = @citizenId
            ORDER BY donated_at DESC";

        AddParameter(command, "@districtId", districtId);
        AddParameter(command, "@citizenId", citizenId);

        List<DonationHistoryDto> donations = new();
        using IDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            donations.Add(new DonationHistoryDto
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                Amount = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("amount"))),
                DonatedAt = reader.GetDateTime(reader.GetOrdinal("donated_at"))
            });
        }

        return donations;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
