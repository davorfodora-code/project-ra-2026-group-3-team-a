using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

internal static class BuildingDbReader
{
    internal static Building ReadBuildingFromReader(IDataReader reader)
    {
        Address addr = new Address(
            reader.GetInt64(reader.GetOrdinal("address_id")),
            reader.GetString(reader.GetOrdinal("street")),
            reader.GetString(reader.GetOrdinal("street_number")));
        Location loc = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));
        Building building = new Building(
            reader.GetInt64(reader.GetOrdinal("id")),
            addr,
            reader.GetString(reader.GetOrdinal("neighborhood")),
            loc,
            reader.GetInt32(reader.GetOrdinal("number_of_floors")),
            reader.GetInt64(reader.GetOrdinal("manager_id")));
        building.TotalApartments = reader.GetInt32(reader.GetOrdinal("total_apartments"));
        return building;
    }

    internal static void LoadFloorsForBuilding(IDbConnection connection, Building building)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT f.floor_number, a.apartment_number
            FROM floors f
            JOIN apartments a ON a.floor_id = f.id
            WHERE f.building_id = @buildingId
            ORDER BY f.floor_number, a.apartment_number";
        BuildingDbWriter.AddParameter(cmd, "@buildingId", building.Id);
        Dictionary<int, Floor> floorMap = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int floorNum = reader.GetInt32(reader.GetOrdinal("floor_number"));
            int aptNum   = reader.GetInt32(reader.GetOrdinal("apartment_number"));
            if (!floorMap.TryGetValue(floorNum, out Floor? floor))
            {
                floor = new Floor(floorNum);
                floorMap[floorNum] = floor;
                building.AddFloor(floor);
            }
            floor.AddApartment(new Apartment(aptNum));
        }
    }

    internal static void LoadImagesForBuildings(IDbConnection connection, List<Building> buildings)
    {
        foreach (Building building in buildings)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, image_path FROM building_images WHERE building_id = @buildingId";
            BuildingDbWriter.AddParameter(cmd, "@buildingId", building.Id);
            using IDataReader imgReader = cmd.ExecuteReader();
            while (imgReader.Read())
            {
                building.AddImage(new BuildingImage(
                    imgReader.GetInt64(imgReader.GetOrdinal("id")),
                    imgReader.GetString(imgReader.GetOrdinal("image_path"))));
            }
        }
    }

    internal static IDbCommand BuildSearchCommand(IDbConnection connection, string? address, string? neighborhood, string? city, string? country)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT b.id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country,
                   (SELECT COUNT(*) FROM floors f
                    JOIN apartments ap ON ap.floor_id = f.id
                    WHERE f.building_id = b.id) AS total_apartments
            FROM buildings b
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(address))
        {
            cmd.CommandText += " AND (a.street || ' ' || a.street_number) ILIKE @address";
            BuildingDbWriter.AddParameter(cmd, "@address", "%" + address + "%");
        }
        if (!string.IsNullOrWhiteSpace(neighborhood))
        {
            cmd.CommandText += " AND b.neighborhood ILIKE @neighborhood";
            BuildingDbWriter.AddParameter(cmd, "@neighborhood", "%" + neighborhood + "%");
        }
        if (!string.IsNullOrWhiteSpace(city))
        {
            cmd.CommandText += " AND l.city ILIKE @city";
            BuildingDbWriter.AddParameter(cmd, "@city", "%" + city + "%");
        }
        if (!string.IsNullOrWhiteSpace(country))
        {
            cmd.CommandText += " AND l.country ILIKE @country";
            BuildingDbWriter.AddParameter(cmd, "@country", "%" + country + "%");
        }
        return cmd;
    }
}
