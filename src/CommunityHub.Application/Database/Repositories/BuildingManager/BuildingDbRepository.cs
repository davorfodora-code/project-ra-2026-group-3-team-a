using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class BuildingDbRepository : IBuildingRepository, ITenantBuildingSearchRepository
{
    public Building Create(Building building)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        long locationId = BuildingDbWriter.InsertLocation(connection, building.Location);
        long addressId  = BuildingDbWriter.InsertAddress(connection, building.Address);
        long buildingId = BuildingDbWriter.InsertBuilding(connection, addressId, locationId, building);
        BuildingDbWriter.InsertFloorsAndApartments(connection, buildingId, building.Floors);
        BuildingDbWriter.InsertImages(connection, buildingId, building.Images);
        return new Building(
            buildingId,
            new Address(addressId, building.Address.Street, building.Address.StreetNumber),
            building.Neighborhood,
            new Location(locationId, building.Location.City, building.Location.Country),
            building.NumberOfFloors,
            building.ManagerId);
    }

    public Building? GetById(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
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
            WHERE b.id = @buildingId";
        BuildingDbWriter.AddParameter(cmd, "@buildingId", buildingId);
        Building? building = null;
        using (IDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read())
                building = BuildingDbReader.ReadBuildingFromReader(reader);
        }
        if (building == null) return null;
        BuildingDbReader.LoadFloorsForBuilding(connection, building);
        BuildingDbReader.LoadImagesForBuildings(connection, new List<Building> { building });
        return building;
    }

    public List<Building> Search(string? address, string? neighborhood, string? city, string? country)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = BuildingDbReader.BuildSearchCommand(connection, address, neighborhood, city, country);
        List<Building> buildings = new();
        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                buildings.Add(BuildingDbReader.ReadBuildingFromReader(reader));
        }
        BuildingDbReader.LoadImagesForBuildings(connection, buildings);
        return buildings;
    }

    public List<Building> GetByManagerId(long managerId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
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
            WHERE b.manager_id = @managerId
            ORDER BY a.street, a.street_number";
        BuildingDbWriter.AddParameter(cmd, "@managerId", managerId);
        List<Building> buildings = new();
        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                buildings.Add(BuildingDbReader.ReadBuildingFromReader(reader));
        }
        BuildingDbReader.LoadImagesForBuildings(connection, buildings);
        return buildings;
    }
}
