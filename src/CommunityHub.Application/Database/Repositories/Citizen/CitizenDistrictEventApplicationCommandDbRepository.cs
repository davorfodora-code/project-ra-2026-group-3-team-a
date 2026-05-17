using System.Data;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventApplicationCommandDbRepository
{
    public long CreateApplication(long eventId, long citizenId, DateTime appliedAt, IEnumerable<long>? selectedItemIds)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            long applicationId = CreateApplicationRecord(connection, transaction, eventId, citizenId, appliedAt);
            AssignItemsToApplication(connection, transaction, eventId, citizenId, applicationId, selectedItemIds, allowReassignToSameCitizen: false);
            transaction.Commit();
            return applicationId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void AddItemsToExistingApplication(long eventId, long citizenId, IEnumerable<long>? selectedItemIds)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            long applicationId = GetExistingApplicationId(connection, transaction, eventId, citizenId);
            AssignItemsToApplication(connection, transaction, eventId, citizenId, applicationId, selectedItemIds, allowReassignToSameCitizen: true);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static long CreateApplicationRecord(IDbConnection connection, IDbTransaction transaction, long eventId, long citizenId, DateTime appliedAt)
    {
        IDbCommand appCmd = connection.CreateCommand();
        appCmd.Transaction = transaction;
        appCmd.CommandText = @"
            INSERT INTO district_event_applications (event_id, citizen_id, applied_at)
            VALUES (@eventId, @citizenId, @appliedAt)
            RETURNING id";
        AddParameter(appCmd, "@eventId", eventId);
        AddParameter(appCmd, "@citizenId", citizenId);
        AddParameter(appCmd, "@appliedAt", appliedAt);
        return Convert.ToInt64(appCmd.ExecuteScalar());
    }

    private static long GetExistingApplicationId(IDbConnection connection, IDbTransaction transaction, long eventId, long citizenId)
    {
        IDbCommand getAppCmd = connection.CreateCommand();
        getAppCmd.Transaction = transaction;
        getAppCmd.CommandText = @"
            SELECT id
            FROM district_event_applications
            WHERE event_id = @eventId
              AND citizen_id = @citizenId
            LIMIT 1";
        AddParameter(getAppCmd, "@eventId", eventId);
        AddParameter(getAppCmd, "@citizenId", citizenId);

        object? appIdObj = getAppCmd.ExecuteScalar();
        if (appIdObj == null || appIdObj == DBNull.Value)
        {
            throw new InvalidOperationException("Prijava volontera nije pronadjena.");
        }

        return Convert.ToInt64(appIdObj);
    }

    private static void AssignItemsToApplication(
        IDbConnection connection,
        IDbTransaction transaction,
        long eventId,
        long citizenId,
        long applicationId,
        IEnumerable<long>? selectedItemIds,
        bool allowReassignToSameCitizen)
    {
        foreach (long itemId in selectedItemIds?.Distinct() ?? Enumerable.Empty<long>())
        {
            AssignItemToVolunteer(connection, transaction, eventId, citizenId, itemId, allowReassignToSameCitizen);
            LinkItemToApplication(connection, transaction, applicationId, itemId, allowDuplicateLink: allowReassignToSameCitizen);
        }
    }

    private static void AssignItemToVolunteer(
        IDbConnection connection,
        IDbTransaction transaction,
        long eventId,
        long citizenId,
        long itemId,
        bool allowReassignToSameCitizen)
    {
        IDbCommand updateItemCmd = connection.CreateCommand();
        updateItemCmd.Transaction = transaction;
        updateItemCmd.CommandText = allowReassignToSameCitizen
            ? @"
                UPDATE district_event_required_items
                SET assigned_volunteer_id = @citizenId
                WHERE id = @itemId
                  AND event_id = @eventId
                  AND (assigned_volunteer_id IS NULL OR assigned_volunteer_id = @citizenId)"
            : @"
                UPDATE district_event_required_items
                SET assigned_volunteer_id = @citizenId
                WHERE id = @itemId
                  AND event_id = @eventId
                  AND assigned_volunteer_id IS NULL";

        AddParameter(updateItemCmd, "@citizenId", citizenId);
        AddParameter(updateItemCmd, "@itemId", itemId);
        AddParameter(updateItemCmd, "@eventId", eventId);

        int affectedRows = updateItemCmd.ExecuteNonQuery();
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Nije moguce preuzeti stavku jer je vec preuzeta ili ne postoji.");
        }
    }

    private static void LinkItemToApplication(
        IDbConnection connection,
        IDbTransaction transaction,
        long applicationId,
        long itemId,
        bool allowDuplicateLink)
    {
        IDbCommand linkCmd = connection.CreateCommand();
        linkCmd.Transaction = transaction;
        linkCmd.CommandText = allowDuplicateLink
            ? @"
                INSERT INTO district_event_application_items (application_id, required_item_id)
                VALUES (@applicationId, @requiredItemId)
                ON CONFLICT DO NOTHING"
            : @"
                INSERT INTO district_event_application_items (application_id, required_item_id)
                VALUES (@applicationId, @requiredItemId)";

        AddParameter(linkCmd, "@applicationId", applicationId);
        AddParameter(linkCmd, "@requiredItemId", itemId);
        linkCmd.ExecuteNonQuery();
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
