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
            ItemAssignmentContext context = new(connection, transaction, eventId, citizenId, applicationId, AllowReassignToSameCitizen: false);
            AssignItemsToApplication(context, selectedItemIds);
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
            ItemAssignmentContext context = new(connection, transaction, eventId, citizenId, applicationId, AllowReassignToSameCitizen: true);
            AssignItemsToApplication(context, selectedItemIds);
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

    private static void AssignItemsToApplication(ItemAssignmentContext context, IEnumerable<long>? selectedItemIds)
    {
        foreach (long itemId in selectedItemIds?.Distinct() ?? Enumerable.Empty<long>())
        {
            AssignItemToVolunteer(context, itemId);
            LinkItemToApplication(context, itemId);
        }
    }

    private static void AssignItemToVolunteer(ItemAssignmentContext context, long itemId)
    {
        IDbCommand updateItemCmd = CreateAssignmentCommand(context, itemId);
        int affectedRows = updateItemCmd.ExecuteNonQuery();
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Nije moguce preuzeti stavku jer je vec preuzeta ili ne postoji.");
        }
    }

    private static IDbCommand CreateAssignmentCommand(ItemAssignmentContext context, long itemId)
    {
        IDbCommand updateItemCmd = context.Connection.CreateCommand();
        updateItemCmd.Transaction = context.Transaction;
        updateItemCmd.CommandText = context.AllowReassignToSameCitizen
            ? ReassignItemToSameCitizenCommandText
            : AssignNewItemCommandText;

        AddParameter(updateItemCmd, "@citizenId", context.CitizenId);
        AddParameter(updateItemCmd, "@itemId", itemId);
        AddParameter(updateItemCmd, "@eventId", context.EventId);
        return updateItemCmd;
    }

    private static void LinkItemToApplication(ItemAssignmentContext context, long itemId)
    {
        if (!context.AllowReassignToSameCitizen && IsItemLinkedToAnotherApplication(context, itemId))
        {
            throw new InvalidOperationException("Nije moguce preuzeti stavku jer je vec preuzeta ili ne postoji.");
        }

        IDbCommand linkCmd = context.Connection.CreateCommand();
        linkCmd.Transaction = context.Transaction;
        linkCmd.CommandText = context.AllowReassignToSameCitizen
            ? LinkExistingApplicationItemCommandText
            : LinkNewApplicationItemCommandText;

        AddParameter(linkCmd, "@applicationId", context.ApplicationId);
        AddParameter(linkCmd, "@requiredItemId", itemId);
        linkCmd.ExecuteNonQuery();
    }

    private static bool IsItemLinkedToAnotherApplication(ItemAssignmentContext context, long itemId)
    {
        IDbCommand checkCmd = context.Connection.CreateCommand();
        checkCmd.Transaction = context.Transaction;
        checkCmd.CommandText = @"
            SELECT 1
            FROM district_event_application_items
            WHERE required_item_id = @requiredItemId
              AND application_id <> @applicationId
            LIMIT 1";
        AddParameter(checkCmd, "@requiredItemId", itemId);
        AddParameter(checkCmd, "@applicationId", context.ApplicationId);
        return checkCmd.ExecuteScalar() != null;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }

    private sealed record ItemAssignmentContext(
        IDbConnection Connection,
        IDbTransaction Transaction,
        long EventId,
        long CitizenId,
        long ApplicationId,
        bool AllowReassignToSameCitizen);

    private const string ReassignItemToSameCitizenCommandText = @"
                UPDATE district_event_required_items ri
                SET assigned_volunteer_id = @citizenId
                WHERE ri.id = @itemId
                  AND ri.event_id = @eventId
                  AND (ri.assigned_volunteer_id IS NULL OR ri.assigned_volunteer_id = @citizenId)
                  AND NOT EXISTS (
                      SELECT 1
                      FROM district_event_application_items ai
                      JOIN district_event_applications a ON a.id = ai.application_id
                      WHERE ai.required_item_id = ri.id
                        AND a.event_id = @eventId
                        AND a.citizen_id <> @citizenId
                  )";

    private const string AssignNewItemCommandText = @"
                UPDATE district_event_required_items ri
                SET assigned_volunteer_id = @citizenId
                WHERE ri.id = @itemId
                  AND ri.event_id = @eventId
                  AND ri.assigned_volunteer_id IS NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM district_event_application_items ai
                      WHERE ai.required_item_id = ri.id
                  )";

    private const string LinkExistingApplicationItemCommandText = @"
                INSERT INTO district_event_application_items (application_id, required_item_id)
                VALUES (@applicationId, @requiredItemId)
                ON CONFLICT (required_item_id) DO NOTHING";

    private const string LinkNewApplicationItemCommandText = @"
                INSERT INTO district_event_application_items (application_id, required_item_id)
                VALUES (@applicationId, @requiredItemId)";
}
