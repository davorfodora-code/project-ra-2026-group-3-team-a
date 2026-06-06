using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class NotificationDbRepository : INotificationRepository
{
    public List<NotificationDto> GetUnreadForTenant(long tenantId)
    {
        var list = new List<NotificationDto>();
        list.AddRange(GetReservationNotifications(tenantId));
        list.AddRange(GetMatchingAdNotifications(tenantId));
        return list.OrderByDescending(n => n.CreatedAt).ToList();
    }

    private static List<NotificationDto> GetReservationNotifications(long tenantId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT n.id, n.created_at,
                   a.category AS own_cat, a.is_offering AS own_offering,
                   u.name || ' ' || u.surname AS actor_name
            FROM notifications n
            JOIN advertisements a    ON a.id  = n.related_ad_id
            JOIN slot_reservations sr ON sr.id = n.related_slot_id
            JOIN users u             ON u.id  = sr.tenant_id
            WHERE n.tenant_id = @tid AND n.is_read = FALSE
              AND n.type = 'Rezervacija termina'
            ORDER BY n.created_at DESC";
        AddParam(cmd, "@tid", tenantId);
        var result = new List<NotificationDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(new NotificationDto
            {
                Id             = r.GetInt64(r.GetOrdinal("id")),
                Type           = "Rezervacija termina",
                CreatedAt      = r.GetDateTime(r.GetOrdinal("created_at")),
                ActorName      = r.GetString(r.GetOrdinal("actor_name")),
                OwnAdCategory  = r.GetString(r.GetOrdinal("own_cat")),
                OwnAdIsOffering = r.GetBoolean(r.GetOrdinal("own_offering"))
            });
        return result;
    }

    private static List<NotificationDto> GetMatchingAdNotifications(long tenantId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT DISTINCT ON (n.id)
                n.id, n.created_at, n.related_ad_id,
                ra.is_offering AS rel_offering, ra.description AS rel_desc,
                ra.start_date AS rel_start, ra.end_date AS rel_end,
                ru.name || ' ' || ru.surname AS actor_name,
                ra.category,
                oa.id AS own_ad_id, oa.is_offering AS own_offering,
                oa.start_date AS own_start, oa.end_date AS own_end
            FROM notifications n
            JOIN advertisements ra ON ra.id       = n.related_ad_id
            JOIN users ru          ON ru.id       = ra.tenant_id
            JOIN advertisements oa ON oa.tenant_id = n.tenant_id
                AND oa.category    = ra.category
                AND oa.is_offering != ra.is_offering
                AND oa.status      = 'Aktivan'
                AND oa.end_date   >= ra.start_date
                AND oa.start_date <= ra.end_date
            WHERE n.tenant_id = @tid AND n.is_read = FALSE
              AND n.type = 'Novi podudarni oglas'
            ORDER BY n.id, oa.id";
        AddParam(cmd, "@tid", tenantId);
        var result = new List<NotificationDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(new NotificationDto
            {
                Id                  = r.GetInt64(r.GetOrdinal("id")),
                Type                = "Novi podudarni oglas",
                CreatedAt           = r.GetDateTime(r.GetOrdinal("created_at")),
                ActorName           = r.GetString(r.GetOrdinal("actor_name")),
                OwnAdCategory       = r.GetString(r.GetOrdinal("category")),
                OwnAdIsOffering     = r.GetBoolean(r.GetOrdinal("own_offering")),
                RelatedAdId         = r.GetInt64(r.GetOrdinal("related_ad_id")),
                RelatedAdIsOffering = r.GetBoolean(r.GetOrdinal("rel_offering")),
                RelatedAdDescription = r.GetString(r.GetOrdinal("rel_desc")),
                RelatedAdStart      = r.GetDateTime(r.GetOrdinal("rel_start")),
                RelatedAdEnd        = r.GetDateTime(r.GetOrdinal("rel_end")),
                OwnAdId             = r.GetInt64(r.GetOrdinal("own_ad_id")),
                OwnAdStart          = r.GetDateTime(r.GetOrdinal("own_start")),
                OwnAdEnd            = r.GetDateTime(r.GetOrdinal("own_end"))
            });
        return result;
    }

    public void MarkAsRead(long notificationId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE notifications SET is_read = TRUE WHERE id = @id";
        AddParam(cmd, "@id", notificationId);
        cmd.ExecuteNonQuery();
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
