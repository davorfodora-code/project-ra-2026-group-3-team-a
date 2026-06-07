using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class IssueReportDbRepository : IIssueReportRepository
{
    public List<IssueReport> GetByBuildingId(long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ir.id, ir.description, ir.priority, ir.reported_at, ir.status,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM issue_reports ir
            JOIN users     u ON u.id = ir.tenant_id
            JOIN buildings b ON b.id = ir.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE ir.building_id = @buildingId
            ORDER BY
                CASE ir.priority WHEN 'Urgent' THEN 1 WHEN 'SolveSoon' THEN 2 ELSE 3 END,
                ir.reported_at DESC";
        AddParam(cmd, "@buildingId", buildingId);
        return ReadReports(cmd);
    }

    public List<IssueReport> GetByManagerId(long managerId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ir.id, ir.description, ir.priority, ir.reported_at, ir.status,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM issue_reports ir
            JOIN users     u ON u.id = ir.tenant_id
            JOIN buildings b ON b.id = ir.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE b.manager_id = @managerId
            ORDER BY
                CASE ir.priority WHEN 'Urgent' THEN 1 WHEN 'SolveSoon' THEN 2 ELSE 3 END,
                ir.reported_at DESC";
        AddParam(cmd, "@managerId", managerId);
        return ReadReports(cmd);
    }

    public void MarkAsPotentiallyResolved(long reportId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE issue_reports SET status = 'PotentiallyResolved' WHERE id = @id";
        AddParam(cmd, "@id", reportId);
        cmd.ExecuteNonQuery();
    }

    private static List<IssueReport> ReadReports(IDbCommand cmd)
    {
        var result = new List<IssueReport>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(ReadReport(r));
        return result;
    }

    private static IssueReport ReadReport(IDataReader r)
    {
        Building building = ReadBuilding(r, ReadAddress(r), ReadLocation(r));
        return new IssueReport(
            r.GetInt64(r.GetOrdinal("id")),
            ReadTenant(r), building,
            r.GetString(r.GetOrdinal("description")),
            r.GetString(r.GetOrdinal("priority")),
            r.GetDateTime(r.GetOrdinal("reported_at")),
            r.GetString(r.GetOrdinal("status")));
    }

    private static User ReadTenant(IDataReader r) => new User(
        r.GetInt64(r.GetOrdinal("tenant_id")),
        r.GetString(r.GetOrdinal("username")),
        r.GetString(r.GetOrdinal("password")),
        r.GetString(r.GetOrdinal("name")),
        r.GetString(r.GetOrdinal("surname")),
        r.GetDateTime(r.GetOrdinal("birthday")),
        r.GetString(r.GetOrdinal("role")));

    private static Address ReadAddress(IDataReader r) => new Address(
        r.GetInt64(r.GetOrdinal("address_id")),
        r.GetString(r.GetOrdinal("street")),
        r.GetString(r.GetOrdinal("street_number")));

    private static Location ReadLocation(IDataReader r) => new Location(
        r.GetInt64(r.GetOrdinal("location_id")),
        r.GetString(r.GetOrdinal("city")),
        r.GetString(r.GetOrdinal("country")));

    private static Building ReadBuilding(IDataReader r, Address address, Location location) => new Building(
        r.GetInt64(r.GetOrdinal("building_id")),
        address,
        r.GetString(r.GetOrdinal("neighborhood")),
        location,
        r.GetInt32(r.GetOrdinal("number_of_floors")),
        r.GetInt64(r.GetOrdinal("manager_id")));

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
