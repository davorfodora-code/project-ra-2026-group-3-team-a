using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class CommonRoomTenantDbRepository : ICommonRoomTenantRepository
{
    public List<CommonRoomDto> GetByBuildingId(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, name, description, floor_number, is_multi_day
            FROM common_rooms
            WHERE building_id = @buildingId
            ORDER BY floor_number, name";
        AddParameter(cmd, "@buildingId", buildingId);

        var rooms = new List<CommonRoomDto>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            rooms.Add(new CommonRoomDto
            {
                RoomId      = reader.GetInt64(reader.GetOrdinal("id")),
                BuildingId  = buildingId,
                Name        = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                FloorNumber = reader.GetInt32(reader.GetOrdinal("floor_number")),
                IsMultiDay  = reader.GetBoolean(reader.GetOrdinal("is_multi_day")),
            });
        }
        return rooms;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
