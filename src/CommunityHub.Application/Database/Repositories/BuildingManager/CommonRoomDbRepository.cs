using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class CommonRoomDbRepository : ICommonRoomRepository
{
    // Vraća sve zajednicke prostorije za datu zgradu
    public List<CommonRoom> GetByBuildingId(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT cr.id, cr.name, cr.description, cr.floor_number, cr.is_multi_day,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country
            FROM common_rooms cr
            JOIN buildings b ON b.id = cr.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE cr.building_id = @buildingId
            ORDER BY cr.floor_number, cr.name";

        AddParameter(cmd, "@buildingId", buildingId);

        List<CommonRoom> rooms = new();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Building building = ReadBuildingFromReader(reader);
            rooms.Add(new CommonRoom(
                reader.GetInt64(reader.GetOrdinal("id")),
                building,
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("description")),
                reader.GetInt32(reader.GetOrdinal("floor_number")),
                reader.GetBoolean(reader.GetOrdinal("is_multi_day"))));
        }

        return rooms;
    }

    // Dodaje novu zajednicku prostoriju i vraca je sa dodeljenim ID-em
    public CommonRoom Add(CommonRoom room)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO common_rooms (building_id, name, description, floor_number, is_multi_day)
            VALUES (@buildingId, @name, @description, @floorNumber, @isMultiDay)
            RETURNING id";

        AddParameter(cmd, "@buildingId", room.Building.Id);
        AddParameter(cmd, "@name", room.Name);
        AddParameter(cmd, "@description", room.Description);
        AddParameter(cmd, "@floorNumber", room.FloorNumber);
        AddParameter(cmd, "@isMultiDay", room.IsMultiDay);

        long id = (long)cmd.ExecuteScalar()!;

        return new CommonRoom(id, room.Building, room.Name, room.Description, room.FloorNumber, room.IsMultiDay);
    }

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
