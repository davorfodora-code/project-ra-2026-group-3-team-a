using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

internal static class BuildingDbWriter
{
    internal static long InsertLocation(IDbConnection connection, Location location)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO locations (city, country)
            VALUES (@city, @country)
            RETURNING id";
        AddParameter(cmd, "@city", location.City);
        AddParameter(cmd, "@country", location.Country);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    internal static long InsertAddress(IDbConnection connection, Address address)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO addresses (street, street_number)
            VALUES (@street, @streetNumber)
            RETURNING id";
        AddParameter(cmd, "@street", address.Street);
        AddParameter(cmd, "@streetNumber", address.StreetNumber);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    internal static long InsertBuilding(IDbConnection connection, long addressId, long locationId, Building building)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO buildings (address_id, neighborhood, location_id, number_of_floors, manager_id)
            VALUES (@addressId, @neighborhood, @locationId, @numberOfFloors, @managerId)
            RETURNING id";
        AddParameter(cmd, "@addressId", addressId);
        AddParameter(cmd, "@neighborhood", building.Neighborhood);
        AddParameter(cmd, "@locationId", locationId);
        AddParameter(cmd, "@numberOfFloors", building.NumberOfFloors);
        AddParameter(cmd, "@managerId", building.ManagerId);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    internal static void InsertFloorsAndApartments(IDbConnection connection, long buildingId, IEnumerable<Floor> floors)
    {
        foreach (Floor floor in floors)
        {
            long floorId = InsertFloor(connection, buildingId, floor.FloorNumber);
            foreach (Apartment apartment in floor.Apartments)
                InsertApartment(connection, floorId, apartment.ApartmentNumber);
        }
    }

    private static long InsertFloor(IDbConnection connection, long buildingId, int floorNumber)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO floors (building_id, floor_number)
            VALUES (@buildingId, @floorNumber)
            RETURNING id";
        AddParameter(cmd, "@buildingId", buildingId);
        AddParameter(cmd, "@floorNumber", floorNumber);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    private static void InsertApartment(IDbConnection connection, long floorId, int apartmentNumber)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO apartments (floor_id, apartment_number)
            VALUES (@floorId, @apartmentNumber)";
        AddParameter(cmd, "@floorId", floorId);
        AddParameter(cmd, "@apartmentNumber", apartmentNumber);
        cmd.ExecuteNonQuery();
    }

    internal static void InsertImages(IDbConnection connection, long buildingId, IEnumerable<BuildingImage> images)
    {
        foreach (BuildingImage image in images)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO building_images (building_id, image_path)
                VALUES (@buildingId, @imagePath)";
            AddParameter(cmd, "@buildingId", buildingId);
            AddParameter(cmd, "@imagePath", image.ImagePath);
            cmd.ExecuteNonQuery();
        }
    }

    internal static void AddParameter(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
