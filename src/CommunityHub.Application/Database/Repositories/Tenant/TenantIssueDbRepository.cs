using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class TenantIssueDbRepository : ITenantIssueRepository
{
    public List<IssueReportDto> GetByTenantId(long tenantId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ir.id, ir.building_id, ir.description, ir.priority, ir.reported_at, ir.status,
                   a.street, a.street_number, l.city
            FROM issue_reports ir
            JOIN buildings b ON b.id = ir.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE ir.tenant_id = @tenantId
            ORDER BY ir.reported_at DESC";
        AddParam(cmd, "@tenantId", tenantId);
        var result = new List<IssueReportDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(MapRow(r));
        return result;
    }

    public void Create(CreateIssueReportDto dto)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO issue_reports (tenant_id, building_id, description, priority, reported_at, status)
            VALUES (@tenantId, @buildingId, @description, @priority, @reportedAt, 'Unresolved')";
        AddParam(cmd, "@tenantId",    dto.TenantId);
        AddParam(cmd, "@buildingId",  dto.BuildingId);
        AddParam(cmd, "@description", dto.Description);
        AddParam(cmd, "@priority",    dto.Priority);
        AddParam(cmd, "@reportedAt",  DateTime.Now);
        cmd.ExecuteNonQuery();
    }

    public void ConfirmResolved(long issueId) => UpdateStatus(issueId, "Resolved");

    public void DenyResolved(long issueId) => UpdateStatus(issueId, "Unresolved");

    private static void UpdateStatus(long issueId, string status)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE issue_reports SET status = @status WHERE id = @id";
        AddParam(cmd, "@status", status);
        AddParam(cmd, "@id",     issueId);
        cmd.ExecuteNonQuery();
    }

    private static IssueReportDto MapRow(IDataReader r) => new()
    {
        Id              = r.GetInt64(r.GetOrdinal("id")),
        BuildingId      = r.GetInt64(r.GetOrdinal("building_id")),
        BuildingAddress = $"{r.GetString(r.GetOrdinal("street"))} {r.GetString(r.GetOrdinal("street_number"))}, {r.GetString(r.GetOrdinal("city"))}",
        Description     = r.GetString(r.GetOrdinal("description")),
        Priority        = r.GetString(r.GetOrdinal("priority")),
        ReportedAt      = r.GetDateTime(r.GetOrdinal("reported_at")),
        Status          = r.GetString(r.GetOrdinal("status")),
    };

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
