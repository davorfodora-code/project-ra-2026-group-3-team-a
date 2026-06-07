using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class TenantAssemblyDbRepository : ITenantAssemblyRepository
{
    public List<TenantAssemblyDto> GetByTenantId(long tenantId)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        // JOIN-ovi umjesto correlated subqueries da bi se izbjeglo duplo korišćenje istog parametra
        cmd.CommandText = @"
            SELECT ta.id, ta.building_id, ta.scheduled_at, ta.status,
                   a.street, a.street_number, l.city,
                   (SELECT COUNT(*) FROM assembly_attendances ac WHERE ac.assembly_id = ta.id) AS confirmed_count,
                   (SELECT COUNT(*) FROM apartments ap JOIN floors fl ON fl.id = ap.floor_id WHERE fl.building_id = ta.building_id) AS total_apartments,
                   tbm.apartment_number AS apt_num,
                   (taa.tenant_id IS NOT NULL) AS is_confirmed
            FROM tenant_assemblies ta
            JOIN buildings b ON b.id = ta.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            JOIN building_memberships tbm ON tbm.building_id = ta.building_id AND tbm.tenant_id = @tid
            LEFT JOIN assembly_attendances taa ON taa.assembly_id = ta.id AND taa.tenant_id = @tid2
            ORDER BY ta.scheduled_at DESC";
        AddParam(cmd, "@tid",  tenantId);
        AddParam(cmd, "@tid2", tenantId);
        var dict = new Dictionary<long, TenantAssemblyDto>();
        using (var r = cmd.ExecuteReader())
            while (r.Read()) { var d = MapRow(r); dict[d.Id] = d; }
        if (dict.Count > 0) LoadTopics(conn, dict);
        return [.. dict.Values];
    }

    private static void LoadTopics(IDbConnection conn, Dictionary<long, TenantAssemblyDto> dict)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT assembly_id, text FROM assembly_topics WHERE assembly_id IN ({string.Join(",", dict.Keys)}) ORDER BY id";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            if (dict.TryGetValue(r.GetInt64(0), out var d)) d.Topics.Add(r.GetString(1));
    }

    public int GetUpcomingCountByTenantId(long tenantId)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT COUNT(*) FROM tenant_assemblies ta
            JOIN building_memberships bm ON bm.building_id = ta.building_id AND bm.tenant_id = @tid
            WHERE ta.scheduled_at > NOW() AND ta.status IN ('Scheduled','Confirmed')";
        AddParam(cmd, "@tid", tenantId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<TenantBuildingDto> GetBuildingsByTenantId(long tenantId)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT b.id, a.street, a.street_number, l.city
            FROM building_memberships bm
            JOIN buildings b ON b.id = bm.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE bm.tenant_id = @tid";
        AddParam(cmd, "@tid", tenantId);
        var result = new List<TenantBuildingDto>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(new TenantBuildingDto
            {
                BuildingId  = r.GetInt64(0),
                DisplayName = $"{r.GetString(1)} {r.GetString(2)}, {r.GetString(3)}"
            });
        return result;
    }

    public void RegisterAttendance(long assemblyId, long tenantId, int apartmentNumber)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO assembly_attendances (assembly_id, apartment_number, tenant_id)
            SELECT @aid, @apt, @tid WHERE NOT EXISTS
            (SELECT 1 FROM assembly_attendances WHERE assembly_id = @aid2 AND tenant_id = @tid2)";
        AddParam(cmd, "@aid",  assemblyId);
        AddParam(cmd, "@apt",  apartmentNumber);
        AddParam(cmd, "@tid",  tenantId);
        AddParam(cmd, "@aid2", assemblyId);
        AddParam(cmd, "@tid2", tenantId);
        cmd.ExecuteNonQuery();
    }

    public void CancelAttendance(long assemblyId, long tenantId)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM assembly_attendances WHERE assembly_id = @aid AND tenant_id = @tid";
        AddParam(cmd, "@aid", assemblyId);
        AddParam(cmd, "@tid", tenantId);
        cmd.ExecuteNonQuery();
    }

    public void ProposeTopic(long assemblyId, long tenantId, string topic)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO assembly_topic_suggestions (assembly_id, tenant_id, topic) VALUES (@aid, @tid, @topic)";
        AddParam(cmd, "@aid",   assemblyId);
        AddParam(cmd, "@tid",   tenantId);
        AddParam(cmd, "@topic", topic);
        cmd.ExecuteNonQuery();
    }

    public void UpdateStatus(long assemblyId, string status)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE tenant_assemblies SET status = @status WHERE id = @id AND status <> 'Cancelled'";
        AddParam(cmd, "@status", status);
        AddParam(cmd, "@id",     assemblyId);
        cmd.ExecuteNonQuery();
    }

    public int GetConfirmedCount(long assemblyId)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM assembly_attendances WHERE assembly_id = @id";
        AddParam(cmd, "@id", assemblyId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetTotalApartments(long buildingId)
    {
        using var conn = PostgresConnection.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM apartments ap JOIN floors f ON f.id = ap.floor_id WHERE f.building_id = @id";
        AddParam(cmd, "@id", buildingId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static TenantAssemblyDto MapRow(IDataReader r) => new()
    {
        Id              = r.GetInt64(r.GetOrdinal("id")),
        BuildingId      = r.GetInt64(r.GetOrdinal("building_id")),
        BuildingAddress = $"{r.GetString(r.GetOrdinal("street"))} {r.GetString(r.GetOrdinal("street_number"))}, {r.GetString(r.GetOrdinal("city"))}",
        ScheduledAt     = r.GetDateTime(r.GetOrdinal("scheduled_at")),
        Status          = r.GetString(r.GetOrdinal("status")),
        ConfirmedCount  = Convert.ToInt32(r["confirmed_count"]),
        TotalApartments = Convert.ToInt32(r["total_apartments"]),
        IsTenantConfirmed = Convert.ToBoolean(r["is_confirmed"]),
        ApartmentNumber = Convert.ToInt32(r["apt_num"]),
    };

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter(); p.ParameterName = name; p.Value = value; cmd.Parameters.Add(p);
    }
}
