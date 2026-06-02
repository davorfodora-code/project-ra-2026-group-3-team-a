using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class AssemblyDbRepository : IAssemblyRepository
{
    // Vraća sve skupštine za zgrade kojima upravlja dati upravnik (sa temama)
    public List<TenantAssembly> GetByManagerId(long managerId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ta.id, ta.scheduled_at, ta.status,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country,
                   t.id AS topic_id, t.text AS topic_text, t.is_additional
            FROM tenant_assemblies ta
            JOIN buildings  b ON b.id = ta.building_id
            JOIN addresses  a ON a.id = b.address_id
            JOIN locations  l ON l.id = b.location_id
            LEFT JOIN assembly_topics t ON t.assembly_id = ta.id
            WHERE b.manager_id = @managerId
            ORDER BY ta.scheduled_at DESC, t.id ASC";
        AddParam(cmd, "@managerId", managerId);

        var dict   = new Dictionary<long, TenantAssembly>();
        var order  = new List<long>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
        {
            long id = r.GetInt64(r.GetOrdinal("id"));
            if (!dict.TryGetValue(id, out TenantAssembly? assembly))
            {
                assembly = ReadAssembly(r);
                dict[id] = assembly;
                order.Add(id);
            }
            if (!r.IsDBNull(r.GetOrdinal("topic_id")))
                assembly.AddTopic(new AssemblyTopic(
                    r.GetInt64(r.GetOrdinal("topic_id")),
                    r.GetString(r.GetOrdinal("topic_text")),
                    r.GetBoolean(r.GetOrdinal("is_additional"))));
        }
        return order.Select(id => dict[id]).ToList();
    }

    // Vraća jednu skupštinu sa temama
    public TenantAssembly? GetById(long assemblyId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ta.id, ta.scheduled_at, ta.status,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country,
                   t.id AS topic_id, t.text AS topic_text, t.is_additional
            FROM tenant_assemblies ta
            JOIN buildings  b ON b.id = ta.building_id
            JOIN addresses  a ON a.id = b.address_id
            JOIN locations  l ON l.id = b.location_id
            LEFT JOIN assembly_topics t ON t.assembly_id = ta.id
            WHERE ta.id = @assemblyId
            ORDER BY t.id ASC";
        AddParam(cmd, "@assemblyId", assemblyId);

        TenantAssembly? assembly = null;
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
        {
            assembly ??= ReadAssembly(r);
            if (!r.IsDBNull(r.GetOrdinal("topic_id")))
                assembly.AddTopic(new AssemblyTopic(
                    r.GetInt64(r.GetOrdinal("topic_id")),
                    r.GetString(r.GetOrdinal("topic_text")),
                    r.GetBoolean(r.GetOrdinal("is_additional"))));
        }
        return assembly;
    }

    // Proverava da li vec postoji skupstina u istom terminu sa statusom Scheduled/Confirmed
    public bool HasTimeConflict(long buildingId, DateTime scheduledAt)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM tenant_assemblies
            WHERE building_id = @buildingId
              AND scheduled_at = @scheduledAt
              AND status IN ('Scheduled', 'Confirmed')";
        AddParam(cmd, "@buildingId", buildingId);
        AddParam(cmd, "@scheduledAt", scheduledAt);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    // Kreira skupstinu i njene pocetne teme, vraca domain objekat sa ID-em
    public TenantAssembly Create(TenantAssembly assembly)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO tenant_assemblies (building_id, scheduled_at, status)
            VALUES (@buildingId, @scheduledAt, @status)
            RETURNING id";
        AddParam(cmd, "@buildingId", assembly.Building.Id);
        AddParam(cmd, "@scheduledAt", assembly.ScheduledAt);
        AddParam(cmd, "@status", assembly.Status);
        long id = (long)cmd.ExecuteScalar()!;

        foreach (AssemblyTopic topic in assembly.Topics)
        {
            IDbCommand topicCmd = conn.CreateCommand();
            topicCmd.CommandText = @"
                INSERT INTO assembly_topics (assembly_id, text, is_additional)
                VALUES (@assemblyId, @text, @isAdditional)";
            AddParam(topicCmd, "@assemblyId", id);
            AddParam(topicCmd, "@text", topic.Text);
            AddParam(topicCmd, "@isAdditional", topic.IsAdditional);
            topicCmd.ExecuteNonQuery();
        }

        TenantAssembly created = new TenantAssembly(id, assembly.Building, assembly.ScheduledAt, assembly.Status);
        foreach (AssemblyTopic topic in assembly.Topics)
            created.AddTopic(new AssemblyTopic(topic.Text, topic.IsAdditional));
        return created;
    }

    // Vraca prisustvo po stanovima (svi stanovi zgrade + da li je potvrdjeno za tu skupstinu)
    public List<(int ApartmentNumber, bool IsConfirmed)> GetAttendanceSummary(long assemblyId, long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT DISTINCT ap.apartment_number,
                   EXISTS (
                       SELECT 1 FROM assembly_attendances aa
                       WHERE aa.assembly_id = @assemblyId
                         AND aa.apartment_number = ap.apartment_number
                   ) AS is_confirmed
            FROM apartments ap
            JOIN floors f ON f.id = ap.floor_id
            WHERE f.building_id = @buildingId
            ORDER BY ap.apartment_number";
        AddParam(cmd, "@assemblyId", assemblyId);
        AddParam(cmd, "@buildingId", buildingId);
        var result = new List<(int, bool)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add((r.GetInt32(0), r.GetBoolean(1)));
        return result;
    }

    private TenantAssembly ReadAssembly(IDataReader r)
    {
        Address  address  = new Address(r.GetInt64(r.GetOrdinal("address_id")),
                                        r.GetString(r.GetOrdinal("street")),
                                        r.GetString(r.GetOrdinal("street_number")));
        Location location = new Location(r.GetInt64(r.GetOrdinal("location_id")),
                                         r.GetString(r.GetOrdinal("city")),
                                         r.GetString(r.GetOrdinal("country")));
        Building building = new Building(r.GetInt64(r.GetOrdinal("building_id")),
                                         address,
                                         r.GetString(r.GetOrdinal("neighborhood")),
                                         location,
                                         r.GetInt32(r.GetOrdinal("number_of_floors")),
                                         r.GetInt64(r.GetOrdinal("manager_id")));
        return new TenantAssembly(r.GetInt64(r.GetOrdinal("id")),
                                  building,
                                  r.GetDateTime(r.GetOrdinal("scheduled_at")),
                                  r.GetString(r.GetOrdinal("status")));
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
