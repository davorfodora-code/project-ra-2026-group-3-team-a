using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class CommonRoomRequestDbRepository : ICommonRoomRequestRepository
{
    public List<CommonRoomRequest> GetByManagerId(long managerId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT r.id, r.requested_start_date, r.requested_end_date,
                   r.proposed_start_date, r.proposed_end_date, r.approved_date, r.status,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role,
                   cr.id AS room_id, cr.name AS room_name, cr.description, cr.floor_number, cr.is_multi_day,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM common_room_requests r
            JOIN users u ON u.id = r.tenant_id
            JOIN common_rooms cr ON cr.id = r.common_room_id
            JOIN buildings b ON b.id = cr.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE b.manager_id = @managerId
            ORDER BY r.requested_start_date DESC";
        AddParameter(cmd, "@managerId", managerId);
        List<CommonRoomRequest> requests = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            requests.Add(ReadCommonRoomRequest(reader));
        return requests;
    }

    private CommonRoomRequest ReadCommonRoomRequest(IDataReader reader)
    {
        User tenant       = ReadUserFromReader(reader);
        Building building = ReadBuildingFromReader(reader);
        CommonRoom room = new CommonRoom(
            reader.GetInt64(reader.GetOrdinal("room_id")),
            building,
            reader.GetString(reader.GetOrdinal("room_name")),
            reader.GetString(reader.GetOrdinal("description")),
            reader.GetInt32(reader.GetOrdinal("floor_number")),
            reader.GetBoolean(reader.GetOrdinal("is_multi_day")));
        DateTime? proposedStart = reader.IsDBNull(reader.GetOrdinal("proposed_start_date"))
            ? null : reader.GetDateTime(reader.GetOrdinal("proposed_start_date"));
        DateTime? proposedEnd = reader.IsDBNull(reader.GetOrdinal("proposed_end_date"))
            ? null : reader.GetDateTime(reader.GetOrdinal("proposed_end_date"));
        DateTime? approvedDate = reader.IsDBNull(reader.GetOrdinal("approved_date"))
            ? null : reader.GetDateTime(reader.GetOrdinal("approved_date"));
        return new CommonRoomRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            tenant,
            room,
            reader.GetDateTime(reader.GetOrdinal("requested_start_date")),
            reader.GetDateTime(reader.GetOrdinal("requested_end_date")),
            proposedStart,
            proposedEnd,
            approvedDate,
            reader.GetString(reader.GetOrdinal("status")));
    }

    public void ProposeDate(long requestId, DateTime proposedStart, DateTime proposedEnd)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE common_room_requests
            SET status               = 'ProposedCounter',
                proposed_start_date  = @proposedStart,
                proposed_end_date    = @proposedEnd
            WHERE id = @requestId";
        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@proposedStart", proposedStart);
        AddParameter(cmd, "@proposedEnd", proposedEnd);
        cmd.ExecuteNonQuery();
    }

    public void ApproveWithDate(long requestId, DateTime date)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE common_room_requests
            SET status              = 'Approved',
                proposed_start_date = @date,
                proposed_end_date   = @date,
                approved_date       = NOW()
            WHERE id = @requestId";
        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@date", date.Date);
        cmd.ExecuteNonQuery();
    }

    public List<(DateTime Start, DateTime End)> GetBookedRangesForRoom(long roomId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT COALESCE(proposed_start_date, requested_start_date) AS booked_start,
                   COALESCE(proposed_end_date,   requested_end_date)   AS booked_end
            FROM common_room_requests
            WHERE common_room_id = @roomId
              AND status IN ('Approved', 'ProposedCounter')";
        AddParameter(cmd, "@roomId", roomId);
        List<(DateTime, DateTime)> ranges = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            ranges.Add((reader.GetDateTime(0), reader.GetDateTime(1)));
        return ranges;
    }

    private User ReadUserFromReader(IDataReader reader) => new User(
        reader.GetInt64(reader.GetOrdinal("tenant_id")),
        reader.GetString(reader.GetOrdinal("username")),
        reader.GetString(reader.GetOrdinal("password")),
        reader.GetString(reader.GetOrdinal("name")),
        reader.GetString(reader.GetOrdinal("surname")),
        reader.GetDateTime(reader.GetOrdinal("birthday")),
        reader.GetString(reader.GetOrdinal("role")));

    private Building ReadBuildingFromReader(IDataReader reader)
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

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
