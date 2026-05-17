using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class BuildingAccessRequestDbRepository : IBuildingAccessRequestRepository
{
    public bool ExistsPending(long tenantId, long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM building_access_requests
            WHERE tenant_id = @tenantId
              AND building_id = @buildingId
              AND status = 'Pending'
            LIMIT 1";

        AddParameter(cmd, "@tenantId", tenantId);
        AddParameter(cmd, "@buildingId", buildingId);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    public long Create(long tenantId, long buildingId, int apartmentNumber)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO building_access_requests (tenant_id, building_id, apartment_number, created_at, status)
            VALUES (@tenantId, @buildingId, @apartmentNumber, @createdAt, 'Pending')
            RETURNING id";

        AddParameter(cmd, "@tenantId", tenantId);
        AddParameter(cmd, "@buildingId", buildingId);
        AddParameter(cmd, "@apartmentNumber", apartmentNumber);
        AddParameter(cmd, "@createdAt", DateTime.Now);

        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public List<BuildingAccessRequest> GetAllByTenant(long tenantId)
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
            WHERE r.tenant_id = @tenantId
            ORDER BY r.created_at DESC";

        AddParameter(cmd, "@tenantId", tenantId);

        List<BuildingAccessRequest> requests = new List<BuildingAccessRequest>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            requests.Add(MapRequest(reader));
        }

        return requests;
    }

    private static BuildingAccessRequest MapRequest(IDataReader reader)
    {
        User tenant = MapTenant(reader);
        Building building = MapBuilding(reader);
        string? rejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason"))
            ? null
            : reader.GetString(reader.GetOrdinal("rejection_reason"));

        return new BuildingAccessRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            tenant,
            building,
            reader.GetInt32(reader.GetOrdinal("apartment_number")),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetString(reader.GetOrdinal("status")),
            rejectionReason);
    }

    private static User MapTenant(IDataReader reader)
    {
        return new User(
            reader.GetInt64(reader.GetOrdinal("tenant_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")));
    }

    private static Building MapBuilding(IDataReader reader)
    {
        Address address = new Address(
            reader.GetInt64(reader.GetOrdinal("address_id")),
            reader.GetString(reader.GetOrdinal("street")),
            reader.GetString(reader.GetOrdinal("street_number")));

        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

        return new Building(
            reader.GetInt64(reader.GetOrdinal("building_id")),
            address,
            reader.GetString(reader.GetOrdinal("neighborhood")),
            location,
            reader.GetInt32(reader.GetOrdinal("number_of_floors")),
            reader.GetInt64(reader.GetOrdinal("manager_id")));
    }

    public List<int> GetApartmentNumbers(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT ap.apartment_number
            FROM apartments ap
            JOIN floors f ON f.id = ap.floor_id
            WHERE f.building_id = @buildingId
            ORDER BY ap.apartment_number";
        AddParameter(cmd, "@buildingId", buildingId);
        var numbers = new List<int>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            numbers.Add(reader.GetInt32(0));
        return numbers;
    }

    public void Delete(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM building_access_requests WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
        cmd.ExecuteNonQuery();
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
