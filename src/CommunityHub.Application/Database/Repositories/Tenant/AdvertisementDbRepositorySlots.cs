using System.Data;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public partial class AdvertisementDbRepository
{
    public List<SlotDto> GetAvailableSlots(long adId, DateTime rangeStart, DateTime rangeEnd)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, start_time, end_time
            FROM advertisement_slots
            WHERE advertisement_id = @adId
              AND is_available = TRUE
              AND start_time::date >= @rangeStart::date
              AND start_time::date <= @rangeEnd::date
            ORDER BY start_time";
        AddParameter(cmd, "@adId", adId);
        AddParameter(cmd, "@rangeStart", rangeStart.Date);
        AddParameter(cmd, "@rangeEnd", rangeEnd.Date);
        List<SlotDto> slots = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            slots.Add(new SlotDto
            {
                Id        = reader.GetInt64(reader.GetOrdinal("id")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("start_time")),
                EndTime   = reader.GetDateTime(reader.GetOrdinal("end_time"))
            });
        return slots;
    }

    public void ReserveSlot(long slotId, long requesterAdId, long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        using IDbTransaction tx = connection.BeginTransaction();
        CheckSlotAvailabilityOrThrow(connection, tx, slotId);
        MarkSlotUnavailable(connection, tx, slotId);
        long reservationId = InsertReservation(connection, tx, slotId, requesterAdId, tenantId);
        InsertReservationNotification(connection, tx, slotId, reservationId);
        tx.Commit();
    }

    private static void CheckSlotAvailabilityOrThrow(IDbConnection connection, IDbTransaction tx, long slotId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT is_available FROM advertisement_slots WHERE id = @slotId FOR UPDATE";
        AddParameter(cmd, "@slotId", slotId);
        object? available = cmd.ExecuteScalar();
        if (available == null || available == DBNull.Value || !(bool)available)
        {
            tx.Rollback();
            throw new InvalidOperationException("Odabrani termin više nije dostupan.");
        }
    }

    private static void MarkSlotUnavailable(IDbConnection connection, IDbTransaction tx, long slotId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "UPDATE advertisement_slots SET is_available = FALSE WHERE id = @slotId";
        AddParameter(cmd, "@slotId", slotId);
        cmd.ExecuteNonQuery();
    }

    private static long InsertReservation(IDbConnection connection, IDbTransaction tx,
        long slotId, long requesterAdId, long tenantId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO slot_reservations (slot_id, requester_ad_id, tenant_id)
            VALUES (@slotId, @requesterAdId, @tenantId)
            RETURNING id";
        AddParameter(cmd, "@slotId", slotId);
        AddParameter(cmd, "@requesterAdId", requesterAdId);
        AddParameter(cmd, "@tenantId", tenantId);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    private static void InsertReservationNotification(IDbConnection connection, IDbTransaction tx,
        long slotId, long reservationId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO notifications (tenant_id, type, related_ad_id, related_slot_id, is_read, created_at)
            SELECT a.tenant_id, 'Rezervacija termina', a.id, @reservationId, FALSE, @now
            FROM advertisement_slots s
            JOIN advertisements a ON a.id = s.advertisement_id
            WHERE s.id = @slotId";
        AddParameter(cmd, "@reservationId", reservationId);
        AddParameter(cmd, "@now", DateTime.UtcNow);
        AddParameter(cmd, "@slotId", slotId);
        cmd.ExecuteNonQuery();
    }
}
