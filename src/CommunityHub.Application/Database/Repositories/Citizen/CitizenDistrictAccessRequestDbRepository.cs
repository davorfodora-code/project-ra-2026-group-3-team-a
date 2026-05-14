using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictAccessRequestDbRepository : ICitizenDistrictAccessRequestRepository
{
    public bool ExistsPending(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM district_access_requests
            WHERE user_id = @userId
              AND district_id = @districtId
              AND status = 'Pending'
            LIMIT 1";

        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@districtId", districtId);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    public long Create(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_access_requests (user_id, district_id, creation_date, status)
            VALUES (@userId, @districtId, @creationDate, 'Pending')
            RETURNING id";

        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@creationDate", DateTime.Now);

        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public List<DistrictAccessRequest> GetAllByUserId(long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT r.id, r.creation_date, r.status, r.rejection_reason,
                   u.id AS user_id, u.username, u.password, u.name, u.surname, u.birthday, u.role, u.address,
                   d.id AS district_id, d.name AS district_name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country
            FROM district_access_requests r
            JOIN users u ON u.id = r.user_id
            JOIN districts d ON d.id = r.district_id
            JOIN locations l ON l.id = d.location_id
            WHERE r.user_id = @userId
            ORDER BY r.creation_date DESC";

        AddParameter(cmd, "@userId", userId);

        List<DistrictAccessRequest> requests = new List<DistrictAccessRequest>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            requests.Add(MapAccessRequestRow(reader));
        }

        return requests;
    }

    private static DistrictAccessRequest MapAccessRequestRow(IDataReader reader)
    {
        User user = ReadUserFromAccessRequestRow(reader);
        Location location = ReadLocationFromAccessRequestRow(reader);
        District district = ReadDistrictFromAccessRequestRow(reader, location);

        DistrictAccessRequestStatus status = ReadAccessRequestStatus(reader);
        string? rejectionReason = ReadNullableString(reader, "rejection_reason");

        return new DistrictAccessRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            user,
            district,
            reader.GetDateTime(reader.GetOrdinal("creation_date")),
            status,
            rejectionReason);
    }

    private static User ReadUserFromAccessRequestRow(IDataReader reader)
    {
        string? address = ReadNullableString(reader, "address");

        return new User(
            reader.GetInt64(reader.GetOrdinal("user_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")),
            address);
    }

    private static Location ReadLocationFromAccessRequestRow(IDataReader reader)
    {
        return new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));
    }

    private static District ReadDistrictFromAccessRequestRow(IDataReader reader, Location location)
    {
        // Coordinator nije bitan za Citizen prikaz zahteva; prosleđujemo stub korisnika radi konstruktora.
        User coordinatorStub = new User(0, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        return new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinatorStub);
    }

    private static DistrictAccessRequestStatus ReadAccessRequestStatus(IDataReader reader)
    {
        string statusStr = reader.GetString(reader.GetOrdinal("status"));
        return Enum.Parse<DistrictAccessRequestStatus>(statusStr);
    }

    private static string? ReadNullableString(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return reader.GetString(ordinal);
    }

    public void Delete(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM district_access_requests WHERE id = @requestId";

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

