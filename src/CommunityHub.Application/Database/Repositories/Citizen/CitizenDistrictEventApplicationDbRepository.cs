using System.Data;
using System.Linq;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictEventApplicationDbRepository : ICitizenDistrictEventApplicationRepository
{
    public bool ExistsByEventAndCitizen(long eventId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM district_event_applications
            WHERE event_id = @eventId
              AND citizen_id = @citizenId
            LIMIT 1";
        AddParameter(cmd, "@eventId", eventId);
        AddParameter(cmd, "@citizenId", citizenId);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    public long CreateApplication(long eventId, long citizenId, DateTime appliedAt, IEnumerable<long>? selectedItemIds)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        try
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

            long applicationId = Convert.ToInt64(appCmd.ExecuteScalar());

            foreach (long itemId in selectedItemIds?.Distinct() ?? Enumerable.Empty<long>())
            {
                IDbCommand updateItemCmd = connection.CreateCommand();
                updateItemCmd.Transaction = transaction;
                updateItemCmd.CommandText = @"
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

                IDbCommand linkCmd = connection.CreateCommand();
                linkCmd.Transaction = transaction;
                linkCmd.CommandText = @"
                    INSERT INTO district_event_application_items (application_id, required_item_id)
                    VALUES (@applicationId, @requiredItemId)";
                AddParameter(linkCmd, "@applicationId", applicationId);
                AddParameter(linkCmd, "@requiredItemId", itemId);
                linkCmd.ExecuteNonQuery();
            }

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

            long applicationId = Convert.ToInt64(appIdObj);

            foreach (long itemId in selectedItemIds?.Distinct() ?? Enumerable.Empty<long>())
            {
                IDbCommand updateItemCmd = connection.CreateCommand();
                updateItemCmd.Transaction = transaction;
                updateItemCmd.CommandText = @"
                    UPDATE district_event_required_items
                    SET assigned_volunteer_id = @citizenId
                    WHERE id = @itemId
                      AND event_id = @eventId
                      AND (assigned_volunteer_id IS NULL OR assigned_volunteer_id = @citizenId)";
                AddParameter(updateItemCmd, "@citizenId", citizenId);
                AddParameter(updateItemCmd, "@itemId", itemId);
                AddParameter(updateItemCmd, "@eventId", eventId);

                int affectedRows = updateItemCmd.ExecuteNonQuery();
                if (affectedRows == 0)
                {
                    throw new InvalidOperationException("Nije moguce preuzeti stavku jer je vec preuzeta ili ne postoji.");
                }

                IDbCommand linkCmd = connection.CreateCommand();
                linkCmd.Transaction = transaction;
                linkCmd.CommandText = @"
                    INSERT INTO district_event_application_items (application_id, required_item_id)
                    VALUES (@applicationId, @requiredItemId)
                    ON CONFLICT DO NOTHING";
                AddParameter(linkCmd, "@applicationId", applicationId);
                AddParameter(linkCmd, "@requiredItemId", itemId);
                linkCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void UpsertAttendance(long eventId, long volunteerId, bool attended, DateTime markedAt)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_event_volunteer_attendances (event_id, volunteer_id, attended, marked_at)
            VALUES (@eventId, @volunteerId, @attended, @markedAt)
            ON CONFLICT (event_id, volunteer_id)
            DO UPDATE SET attended = EXCLUDED.attended, marked_at = EXCLUDED.marked_at";

        AddParameter(cmd, "@eventId", eventId);
        AddParameter(cmd, "@volunteerId", volunteerId);
        AddParameter(cmd, "@attended", attended);
        AddParameter(cmd, "@markedAt", markedAt);
        cmd.ExecuteNonQuery();
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
