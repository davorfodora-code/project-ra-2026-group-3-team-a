using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories;

public class BuildingAccessRequestDbRepository
{
    // Vraća sve zahteve za pristup zgradama kojima upravlja dati upravnik
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

        AddParameter(cmd, "@managerId", managerId);

        List<BuildingAccessRequest> requests = new List<BuildingAccessRequest>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            User tenant = new User(
                reader.GetInt64(reader.GetOrdinal("tenant_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role")));

            Address address = new Address(
                reader.GetInt64(reader.GetOrdinal("address_id")),
                reader.GetString(reader.GetOrdinal("street")),
                reader.GetString(reader.GetOrdinal("street_number")));

            Location location = new Location(
                reader.GetInt64(reader.GetOrdinal("location_id")),
                reader.GetString(reader.GetOrdinal("city")),
                reader.GetString(reader.GetOrdinal("country")));

            Building building = new Building(
                reader.GetInt64(reader.GetOrdinal("building_id")),
                address,
                reader.GetString(reader.GetOrdinal("neighborhood")),
                location,
                reader.GetInt32(reader.GetOrdinal("number_of_floors")),
                reader.GetInt64(reader.GetOrdinal("manager_id")));

            string? rejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason"))
                ? null
                : reader.GetString(reader.GetOrdinal("rejection_reason"));

            BuildingAccessRequest request = new BuildingAccessRequest(
                reader.GetInt64(reader.GetOrdinal("id")),
                tenant,
                building,
                reader.GetInt32(reader.GetOrdinal("apartment_number")),
                reader.GetDateTime(reader.GetOrdinal("created_at")),
                reader.GetString(reader.GetOrdinal("status")),
                rejectionReason);

            requests.Add(request);
        }

        return requests;
    }

    // Odobrava zahtev: menja status na "Accepted" i kreira članstvo u zgradi
    public void Approve(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        // 1. Učitaj podatke zahteva
        IDbCommand selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT r.tenant_id, r.building_id, r.apartment_number,
                   f.floor_number
            FROM building_access_requests r
            JOIN buildings b ON b.id = r.building_id
            JOIN floors f ON f.building_id = b.id
            JOIN apartments ap ON ap.floor_id = f.id AND ap.apartment_number = r.apartment_number
            WHERE r.id = @requestId";

        AddParameter(selectCmd, "@requestId", requestId);

        long tenantId;
        long buildingId;
        int apartmentNumber;
        int floorNumber;

        using (IDataReader reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read())
                throw new InvalidOperationException("Zahtev nije pronađen.");

            tenantId = reader.GetInt64(reader.GetOrdinal("tenant_id"));
            buildingId = reader.GetInt64(reader.GetOrdinal("building_id"));
            apartmentNumber = reader.GetInt32(reader.GetOrdinal("apartment_number"));
            floorNumber = reader.GetInt32(reader.GetOrdinal("floor_number"));
        }

        DateTime now = DateTime.Now;

        // 2. Ažuriraj status zahteva
        IDbCommand updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE building_access_requests
            SET status = 'Accepted'
            WHERE id = @requestId";

        AddParameter(updateCmd, "@requestId", requestId);
        updateCmd.ExecuteNonQuery();

        // 3. Kreiraj članstvo u zgradi
        IDbCommand membershipCmd = connection.CreateCommand();
        membershipCmd.CommandText = @"
            INSERT INTO building_memberships (building_id, tenant_id, floor_number, apartment_number, joined_at)
            VALUES (@buildingId, @tenantId, @floorNumber, @apartmentNumber, @joinedAt)";

        AddParameter(membershipCmd, "@buildingId", buildingId);
        AddParameter(membershipCmd, "@tenantId", tenantId);
        AddParameter(membershipCmd, "@floorNumber", floorNumber);
        AddParameter(membershipCmd, "@apartmentNumber", apartmentNumber);
        AddParameter(membershipCmd, "@joinedAt", now);
        membershipCmd.ExecuteNonQuery();
    }

    // Odbija zahtev: menja status na "Rejected" i beleži opciono obrazloženje
    public void Reject(long requestId, string? reason)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE building_access_requests
            SET status = 'Rejected',
                rejection_reason = @reason
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@reason", (object?)reason ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    // Vraća sva članstva za datu zgradu
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

        AddParameter(cmd, "@buildingId", buildingId);

        List<BuildingMembership> memberships = new List<BuildingMembership>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            User tenant = new User(
                reader.GetInt64(reader.GetOrdinal("tenant_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role")));

            // Building objekat nije potreban ovde jer se prikazuje za poznatu zgradu
            BuildingMembership membership = new BuildingMembership(
                reader.GetInt64(reader.GetOrdinal("id")),
                null!,
                tenant,
                reader.GetInt32(reader.GetOrdinal("floor_number")),
                reader.GetInt32(reader.GetOrdinal("apartment_number")),
                reader.GetDateTime(reader.GetOrdinal("joined_at")));

            memberships.Add(membership);
        }

        return memberships;
    }

    // Vraća sve zgrade kojima upravlja dati upravnik (za izbor u pregledu članstava)
    public List<Building> GetBuildingsByManagerId(long managerId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT b.id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM buildings b
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE b.manager_id = @managerId
            ORDER BY a.street, a.street_number";

        AddParameter(cmd, "@managerId", managerId);

        List<Building> buildings = new List<Building>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Address address = new Address(
                reader.GetInt64(reader.GetOrdinal("address_id")),
                reader.GetString(reader.GetOrdinal("street")),
                reader.GetString(reader.GetOrdinal("street_number")));

            Location location = new Location(
                reader.GetInt64(reader.GetOrdinal("location_id")),
                reader.GetString(reader.GetOrdinal("city")),
                reader.GetString(reader.GetOrdinal("country")));

            Building building = new Building(
                reader.GetInt64(reader.GetOrdinal("id")),
                address,
                reader.GetString(reader.GetOrdinal("neighborhood")),
                location,
                reader.GetInt32(reader.GetOrdinal("number_of_floors")),
                reader.GetInt64(reader.GetOrdinal("manager_id")));

            buildings.Add(building);
        }

        return buildings;
    }

    // Pomoćna metoda za dodavanje parametara
    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
