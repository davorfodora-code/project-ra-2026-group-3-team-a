using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class CommonRoomRequestTenantDbRepository : ICommonRoomRequestTenantRepository
{
    public List<MyCommonRoomRequestDto> GetByTenantAndBuilding(long tenantId, long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT r.id, r.requested_start_date, r.requested_end_date,
                   r.proposed_start_date, r.proposed_end_date, r.approved_date, r.status,
                   cr.id AS room_id, cr.name AS room_name, cr.is_multi_day
            FROM common_room_requests r
            JOIN common_rooms cr ON cr.id = r.common_room_id
            WHERE r.tenant_id = @tenantId
              AND cr.building_id = @buildingId
            ORDER BY r.id DESC";
        AddParameter(cmd, "@tenantId", tenantId);
        AddParameter(cmd, "@buildingId", buildingId);

        var list = new List<MyCommonRoomRequestDto>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapCommonRoomRequest(reader));
        return list;
    }

    private static MyCommonRoomRequestDto MapCommonRoomRequest(IDataReader reader) =>
        new MyCommonRoomRequestDto
        {
            RequestId          = reader.GetInt64(reader.GetOrdinal("id")),
            RoomId             = reader.GetInt64(reader.GetOrdinal("room_id")),
            RoomName           = reader.GetString(reader.GetOrdinal("room_name")),
            IsMultiDay         = reader.GetBoolean(reader.GetOrdinal("is_multi_day")),
            RequestedStartDate = reader.GetDateTime(reader.GetOrdinal("requested_start_date")),
            RequestedEndDate   = reader.GetDateTime(reader.GetOrdinal("requested_end_date")),
            ProposedStartDate  = reader.IsDBNull(reader.GetOrdinal("proposed_start_date"))
                                    ? null : reader.GetDateTime(reader.GetOrdinal("proposed_start_date")),
            ProposedEndDate    = reader.IsDBNull(reader.GetOrdinal("proposed_end_date"))
                                    ? null : reader.GetDateTime(reader.GetOrdinal("proposed_end_date")),
            ApprovedDate       = reader.IsDBNull(reader.GetOrdinal("approved_date"))
                                    ? null : reader.GetDateTime(reader.GetOrdinal("approved_date")),
            Status             = reader.GetString(reader.GetOrdinal("status")),
        };

    public void Create(CreateCommonRoomRequestDto dto)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO common_room_requests
                (tenant_id, common_room_id, requested_start_date, requested_end_date, status)
            VALUES (@tenantId, @roomId, @startDate, @endDate, 'Pending')";
        AddParameter(cmd, "@tenantId",   dto.TenantId);
        AddParameter(cmd, "@roomId",     dto.RoomId);
        AddParameter(cmd, "@startDate",  dto.StartDate.Date);
        AddParameter(cmd, "@endDate",    dto.EndDate.Date);
        cmd.ExecuteNonQuery();
    }

    public void Cancel(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE common_room_requests
            SET status = 'Cancelled'
            WHERE id = @requestId
              AND status IN ('Pending', 'ProposedCounter')";
        AddParameter(cmd, "@requestId", requestId);
        cmd.ExecuteNonQuery();
    }

    public void AcceptProposedDate(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE common_room_requests
            SET status               = 'Pending',
                requested_start_date = proposed_start_date,
                requested_end_date   = proposed_end_date,
                proposed_start_date  = NULL,
                proposed_end_date    = NULL
            WHERE id = @requestId
              AND status = 'ProposedCounter'";
        AddParameter(cmd, "@requestId", requestId);
        cmd.ExecuteNonQuery();
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
