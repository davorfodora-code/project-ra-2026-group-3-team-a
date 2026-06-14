using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class SchedulingDbRepository : ISchedulingRepository
{
    public List<MyBookingGroupDto> GetMyBookings(long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                oa.id                                    AS owner_ad_id,
                sr.requester_ad_id,
                ou.name || ' ' || ou.surname             AS owner_name,
                oa.is_offering                           AS owner_is_offering,
                oa.description                           AS owner_description,
                oa.start_date                            AS owner_start_date,
                oa.end_date                              AS owner_end_date,
                oa.category                              AS owner_category,
                ra.is_offering                           AS requester_is_offering,
                s.id                                     AS slot_id,
                s.start_time,
                s.end_time
            FROM slot_reservations sr
            JOIN advertisement_slots s  ON s.id  = sr.slot_id
            JOIN advertisements oa      ON oa.id = s.advertisement_id
            JOIN users ou               ON ou.id = oa.tenant_id
            JOIN advertisements ra      ON ra.id = sr.requester_ad_id
            WHERE sr.tenant_id = @tenantId
            ORDER BY oa.id, ra.id, s.start_time";
        AddParameter(cmd, "@tenantId", tenantId);

        var groups = new Dictionary<(long, long), MyBookingGroupDto>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            long ownerAdId     = reader.GetInt64(reader.GetOrdinal("owner_ad_id"));
            long requesterAdId = reader.GetInt64(reader.GetOrdinal("requester_ad_id"));
            var key = (ownerAdId, requesterAdId);
            if (!groups.TryGetValue(key, out var group))
                groups[key] = group = CreateBookingGroup(reader, ownerAdId, requesterAdId);
            group.Slots.Add(MapBookedSlot(reader));
        }
        return [.. groups.Values];
    }

    private static MyBookingGroupDto CreateBookingGroup(IDataReader reader, long ownerAdId, long requesterAdId) =>
        new MyBookingGroupDto
        {
            OwnerAdId           = ownerAdId,
            RequesterAdId       = requesterAdId,
            OwnerName           = reader.GetString(reader.GetOrdinal("owner_name")),
            IsOwnerOffering     = reader.GetBoolean(reader.GetOrdinal("owner_is_offering")),
            OwnerDescription    = reader.GetString(reader.GetOrdinal("owner_description")),
            OwnerStartDate      = reader.GetDateTime(reader.GetOrdinal("owner_start_date")),
            OwnerEndDate        = reader.GetDateTime(reader.GetOrdinal("owner_end_date")),
            OwnerCategory       = reader.GetString(reader.GetOrdinal("owner_category")),
            IsRequesterOffering = reader.GetBoolean(reader.GetOrdinal("requester_is_offering")),
        };

    public List<BookedOnMyPostDto> GetBookedOnMyPosts(long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                ma.id                                    AS my_ad_id,
                ma.is_offering                           AS my_is_offering,
                ma.start_date                            AS my_start_date,
                ma.end_date                              AS my_end_date,
                ma.category                              AS my_category,
                ma.status                                AS my_status,
                bu.name || ' ' || bu.surname             AS booker_name,
                ra.is_offering                           AS booker_is_offering,
                ra.description                           AS booker_description,
                sr.tenant_id                             AS booker_tenant_id,
                s.id                                     AS slot_id,
                s.start_time,
                s.end_time
            FROM slot_reservations sr
            JOIN advertisement_slots s  ON s.id  = sr.slot_id
            JOIN advertisements ma      ON ma.id = s.advertisement_id
            JOIN users bu               ON bu.id = sr.tenant_id
            JOIN advertisements ra      ON ra.id = sr.requester_ad_id
            WHERE ma.tenant_id = @tenantId
            ORDER BY ma.id, sr.tenant_id, s.start_time";
        AddParameter(cmd, "@tenantId", tenantId);

        var posts      = new Dictionary<long, BookedOnMyPostDto>();
        var bookerKeys = new Dictionary<(long, long), BookerEntryDto>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            long myAdId         = reader.GetInt64(reader.GetOrdinal("my_ad_id"));
            long bookerTenantId = reader.GetInt64(reader.GetOrdinal("booker_tenant_id"));
            if (!posts.TryGetValue(myAdId, out var post))
                posts[myAdId] = post = CreateBookedOnMyPost(reader, myAdId);
            var bookerKey = (myAdId, bookerTenantId);
            if (!bookerKeys.TryGetValue(bookerKey, out var booker))
            {
                bookerKeys[bookerKey] = booker = CreateBookerEntry(reader);
                post.Bookers.Add(booker);
            }
            booker.Slots.Add(MapBookedSlot(reader));
        }
        return [.. posts.Values];
    }

    private static BookedOnMyPostDto CreateBookedOnMyPost(IDataReader reader, long myAdId) =>
        new BookedOnMyPostDto
        {
            MyAdId         = myAdId,
            IsMyAdOffering = reader.GetBoolean(reader.GetOrdinal("my_is_offering")),
            MyStartDate    = reader.GetDateTime(reader.GetOrdinal("my_start_date")),
            MyEndDate      = reader.GetDateTime(reader.GetOrdinal("my_end_date")),
            MyCategory     = reader.GetString(reader.GetOrdinal("my_category")),
            MyStatus       = reader.GetString(reader.GetOrdinal("my_status")),
        };

    private static BookerEntryDto CreateBookerEntry(IDataReader reader) =>
        new BookerEntryDto
        {
            BookerName          = reader.GetString(reader.GetOrdinal("booker_name")),
            IsBookerOffering    = reader.GetBoolean(reader.GetOrdinal("booker_is_offering")),
            BookerAdDescription = reader.GetString(reader.GetOrdinal("booker_description")),
        };

    private static BookedSlotDto MapBookedSlot(IDataReader reader) =>
        new BookedSlotDto
        {
            SlotId    = reader.GetInt64(reader.GetOrdinal("slot_id")),
            StartTime = reader.GetDateTime(reader.GetOrdinal("start_time")),
            EndTime   = reader.GetDateTime(reader.GetOrdinal("end_time")),
        };

    public List<ReportAppointmentDto> GetReportAppointments(long tenantId, DateTime from, DateTime to)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT s.start_time, s.end_time,
                   ou.name || ' ' || ou.surname AS other_person_name,
                   ra.is_offering               AS i_am_offering,
                   oa.category,
                   oa.description               AS ad_description,
                   'Moja rezervacija'           AS source
            FROM slot_reservations sr
            JOIN advertisement_slots s  ON s.id  = sr.slot_id
            JOIN advertisements oa      ON oa.id = s.advertisement_id
            JOIN users ou               ON ou.id = oa.tenant_id
            JOIN advertisements ra      ON ra.id = sr.requester_ad_id
            WHERE sr.tenant_id = @tenantId
              AND s.start_time >= @from
              AND s.start_time <= @to

            UNION ALL

            SELECT s.start_time, s.end_time,
                   bu.name || ' ' || bu.surname AS other_person_name,
                   ma.is_offering               AS i_am_offering,
                   ma.category,
                   ma.description               AS ad_description,
                   'Na mom oglasu'              AS source
            FROM slot_reservations sr
            JOIN advertisement_slots s  ON s.id  = sr.slot_id
            JOIN advertisements ma      ON ma.id = s.advertisement_id
            JOIN users bu               ON bu.id = sr.tenant_id
            WHERE ma.tenant_id = @tenantId
              AND sr.tenant_id != @tenantId
              AND s.start_time >= @from
              AND s.start_time <= @to

            ORDER BY start_time";

        AddParameter(cmd, "@tenantId", tenantId);
        AddParameter(cmd, "@from",     from);
        AddParameter(cmd, "@to",       to);

        var result = new List<ReportAppointmentDto>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ReportAppointmentDto
            {
                SlotStart       = reader.GetDateTime(reader.GetOrdinal("start_time")),
                SlotEnd         = reader.GetDateTime(reader.GetOrdinal("end_time")),
                OtherPersonName = reader.GetString(reader.GetOrdinal("other_person_name")),
                IAmOffering     = reader.GetBoolean(reader.GetOrdinal("i_am_offering")),
                Category        = reader.GetString(reader.GetOrdinal("category")),
                AdDescription   = reader.GetString(reader.GetOrdinal("ad_description")),
                Source          = reader.GetString(reader.GetOrdinal("source")),
            });
        }
        return result;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
