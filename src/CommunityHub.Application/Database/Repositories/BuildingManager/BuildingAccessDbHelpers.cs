using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

internal static class BuildingAccessDbHelpers
{
    internal static User ReadUserFromReader(IDataReader reader, string idColumn) =>
        new User(
            reader.GetInt64(reader.GetOrdinal(idColumn)),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")));

    internal static Building ReadBuildingFromReader(IDataReader reader)
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

    internal static (long tenantId, long buildingId, int apartmentNumber, int floorNumber)
        LoadRequestData(IDbConnection connection, long requestId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT r.tenant_id, r.building_id, r.apartment_number, f.floor_number
            FROM building_access_requests r
            JOIN buildings b ON b.id = r.building_id
            JOIN floors f ON f.building_id = b.id
            JOIN apartments ap ON ap.floor_id = f.id AND ap.apartment_number = r.apartment_number
            WHERE r.id = @requestId";
        AddParameter(cmd, "@requestId", requestId);
        using IDataReader reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException("Zahtev nije pronađen.");
        return (
            reader.GetInt64(reader.GetOrdinal("tenant_id")),
            reader.GetInt64(reader.GetOrdinal("building_id")),
            reader.GetInt32(reader.GetOrdinal("apartment_number")),
            reader.GetInt32(reader.GetOrdinal("floor_number")));
    }

    internal static void UpdateRequestStatus(IDbConnection connection, long requestId, string status)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE building_access_requests
            SET status = @status
            WHERE id = @requestId";
        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@status", status);
        cmd.ExecuteNonQuery();
    }

    internal static void CreateMembership(IDbConnection connection, long buildingId, long tenantId, int floorNumber, int apartmentNumber)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO building_memberships (building_id, tenant_id, floor_number, apartment_number, joined_at)
            VALUES (@buildingId, @tenantId, @floorNumber, @apartmentNumber, @joinedAt)";
        AddParameter(cmd, "@buildingId", buildingId);
        AddParameter(cmd, "@tenantId", tenantId);
        AddParameter(cmd, "@floorNumber", floorNumber);
        AddParameter(cmd, "@apartmentNumber", apartmentNumber);
        AddParameter(cmd, "@joinedAt", DateTime.Now);
        cmd.ExecuteNonQuery();
    }

    internal static BuildingAccessRequest ReadAccessRequest(IDataReader reader)
    {
        User tenant       = ReadUserFromReader(reader, "tenant_id");
        Building building = ReadBuildingFromReader(reader);
        string? rejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason"))
            ? null : reader.GetString(reader.GetOrdinal("rejection_reason"));
        return new BuildingAccessRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            tenant,
            building,
            reader.GetInt32(reader.GetOrdinal("apartment_number")),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetString(reader.GetOrdinal("status")),
            rejectionReason);
    }

    internal static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
