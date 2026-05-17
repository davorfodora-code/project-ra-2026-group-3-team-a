using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class BuildingAccessRequestDbRepository : IBuildingAccessRequestRepository
{
    public List<BuildingAccessRequest> GetByManagerId(long managerId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT r.id, r.apartment_number, r.created_at, r.status, r.rejection_reason,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM building_access_requests r
            JOIN users u ON u.id = r.tenant_id
            JOIN buildings b ON b.id = r.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE b.manager_id = @managerId
            ORDER BY r.created_at DESC";
        BuildingAccessDbHelpers.AddParameter(cmd, "@managerId", managerId);
        List<BuildingAccessRequest> requests = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            requests.Add(BuildingAccessDbHelpers.ReadAccessRequest(reader));
        return requests;
    }

    public void Approve(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        (long tenantId, long buildingId, int apartmentNumber, int floorNumber) =
            BuildingAccessDbHelpers.LoadRequestData(connection, requestId);
        BuildingAccessDbHelpers.UpdateRequestStatus(connection, requestId, "Accepted");
        BuildingAccessDbHelpers.CreateMembership(connection, buildingId, tenantId, floorNumber, apartmentNumber);
    }

    public void Reject(long requestId, string? reason)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE building_access_requests
            SET status = 'Rejected',
                rejection_reason = @reason
            WHERE id = @requestId";
        BuildingAccessDbHelpers.AddParameter(cmd, "@requestId", requestId);
        BuildingAccessDbHelpers.AddParameter(cmd, "@reason", (object?)reason ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public List<BuildingMembership> GetMembershipsByBuildingId(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT m.id, m.floor_number, m.apartment_number, m.joined_at,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role
            FROM building_memberships m
            JOIN users u ON u.id = m.tenant_id
            WHERE m.building_id = @buildingId
            ORDER BY m.floor_number, m.apartment_number";
        BuildingAccessDbHelpers.AddParameter(cmd, "@buildingId", buildingId);
        List<BuildingMembership> memberships = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            User tenant = BuildingAccessDbHelpers.ReadUserFromReader(reader, "tenant_id");
            memberships.Add(new BuildingMembership(
                reader.GetInt64(reader.GetOrdinal("id")),
                null!,
                tenant,
                reader.GetInt32(reader.GetOrdinal("floor_number")),
                reader.GetInt32(reader.GetOrdinal("apartment_number")),
                reader.GetDateTime(reader.GetOrdinal("joined_at"))));
        }
        return memberships;
    }

    public List<Building> GetBuildingsByManagerId(long managerId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM buildings b
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE b.manager_id = @managerId
            ORDER BY a.street, a.street_number";
        BuildingAccessDbHelpers.AddParameter(cmd, "@managerId", managerId);
        List<Building> buildings = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            buildings.Add(BuildingAccessDbHelpers.ReadBuildingFromReader(reader));
        return buildings;
    }
}
