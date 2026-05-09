using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class BuildingDbRepository
{
    public Building Create(Building building)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        long locationId = InsertLocation(connection, building.Location);
        long addressId = InsertAddress(connection, building.Address);
        long buildingId = InsertBuilding(connection, addressId, locationId, building);
        InsertFloorsAndApartments(connection, buildingId, building.Floors);
        InsertImages(connection, buildingId, building.Images);

        return new Building(
            buildingId,
            new Address(addressId, building.Address.Street, building.Address.StreetNumber),
            building.Neighborhood,
            new Location(locationId, building.Location.City, building.Location.Country),
            building.NumberOfFloors,
            building.ManagerId
        );
    }

    // Učitava jednu zgradu sa spratovima, stanovima i slikama
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
        AddParameter(cmd, "@buildingId", buildingId);

        Building? building = null;
        using (IDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read())
                building = ReadBuildingFromReader(reader);
        }

        if (building == null) return null;

        LoadFloorsForBuilding(connection, building);
        LoadImagesForBuildings(connection, new List<Building> { building });
        return building;
    }

    public List<Building> Search(string? address, string? neighborhood, string? city, string? country)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = BuildSearchCommand(connection, address, neighborhood, city, country);
        List<Building> buildings = new List<Building>();

        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                buildings.Add(ReadBuildingFromReader(reader));
        }

        LoadImagesForBuildings(connection, buildings);
        return buildings;
    }

    // Upisuje lokaciju i vraća njen id
    private long InsertLocation(IDbConnection connection, Location location)
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

    // Upisuje adresu i vraća njen id
    private long InsertAddress(IDbConnection connection, Address address)
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

    // Upisuje zgradu i vraća njen id
    private long InsertBuilding(IDbConnection connection, long addressId, long locationId, Building building)
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

    // Upisuje sve spratove i njihove stanove za datu zgradu
    private void InsertFloorsAndApartments(IDbConnection connection, long buildingId, IEnumerable<Floor> floors)
    {
        foreach (Floor floor in floors)
        {
            long floorId = InsertFloor(connection, buildingId, floor.FloorNumber);
            foreach (Apartment apartment in floor.Apartments)
                InsertApartment(connection, floorId, apartment.ApartmentNumber);
        }
    }

    // Upisuje jedan sprat i vraća njegov id
    private long InsertFloor(IDbConnection connection, long buildingId, int floorNumber)
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

    // Upisuje jedan stan
    private void InsertApartment(IDbConnection connection, long floorId, int apartmentNumber)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO apartments (floor_id, apartment_number)
            VALUES (@floorId, @apartmentNumber)";

        AddParameter(cmd, "@floorId", floorId);
        AddParameter(cmd, "@apartmentNumber", apartmentNumber);
        cmd.ExecuteNonQuery();
    }

    // Upisuje sve slike za datu zgradu
    private void InsertImages(IDbConnection connection, long buildingId, IEnumerable<BuildingImage> images)
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

    // Pravi SELECT upit za pretragu zgrada sa dinamičkim filterima
    private IDbCommand BuildSearchCommand(IDbConnection connection, string? address, string? neighborhood, string? city, string? country)
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
            AddParameter(cmd, "@address", "%" + address + "%");
        }

        if (!string.IsNullOrWhiteSpace(neighborhood))
        {
            cmd.CommandText += " AND b.neighborhood ILIKE @neighborhood";
            AddParameter(cmd, "@neighborhood", "%" + neighborhood + "%");
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            cmd.CommandText += " AND l.city ILIKE @city";
            AddParameter(cmd, "@city", "%" + city + "%");
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            cmd.CommandText += " AND l.country ILIKE @country";
            AddParameter(cmd, "@country", "%" + country + "%");
        }

        return cmd;
    }

    // Čita jedan Building objekat iz reader-a
    private Building ReadBuildingFromReader(IDataReader reader)
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

    // Učitava spratove i stanove za datu zgradu i dodaje ih na Building objekat
    private void LoadFloorsForBuilding(IDbConnection connection, Building building)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT f.floor_number, a.apartment_number
            FROM floors f
            JOIN apartments a ON a.floor_id = f.id
            WHERE f.building_id = @buildingId
            ORDER BY f.floor_number, a.apartment_number";
        AddParameter(cmd, "@buildingId", building.Id);

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

    // Učitava slike za svaku zgradu iz liste
    private void LoadImagesForBuildings(IDbConnection connection, List<Building> buildings)
    {
        foreach (Building building in buildings)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, image_path FROM building_images WHERE building_id = @buildingId";
            AddParameter(cmd, "@buildingId", building.Id);

            using IDataReader imgReader = cmd.ExecuteReader();
            while (imgReader.Read())
            {
                building.AddImage(new BuildingImage(
                    imgReader.GetInt64(imgReader.GetOrdinal("id")),
                    imgReader.GetString(imgReader.GetOrdinal("image_path"))));
            }
        }
    }

    // Pomoćna metoda za dodavanje parametara — smanjuje ponavljanje koda
    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
