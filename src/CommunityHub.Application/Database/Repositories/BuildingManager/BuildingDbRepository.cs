using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class BuildingDbRepository
{
    public Building Create(Building building)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        // 1. Upiši lokaciju
        IDbCommand locationCmd = connection.CreateCommand();
        locationCmd.CommandText = @"
            INSERT INTO locations (city, country)
            VALUES (@city, @country)
            RETURNING id";

        AddParameter(locationCmd, "@city", building.Location.City);
        AddParameter(locationCmd, "@country", building.Location.Country);
        long locationId = Convert.ToInt64(locationCmd.ExecuteScalar());

        // 2. Upiši adresu
        IDbCommand addressCmd = connection.CreateCommand();
        addressCmd.CommandText = @"
            INSERT INTO addresses (street, street_number)
            VALUES (@street, @streetNumber)
            RETURNING id";

        AddParameter(addressCmd, "@street", building.Address.Street);
        AddParameter(addressCmd, "@streetNumber", building.Address.StreetNumber);
        long addressId = Convert.ToInt64(addressCmd.ExecuteScalar());

        // 3. Upiši zgradu (naselje se upisuje direktno kao tekst, nije zaseban entitet)
        IDbCommand buildingCmd = connection.CreateCommand();
        buildingCmd.CommandText = @"
            INSERT INTO buildings (address_id, neighborhood, location_id, number_of_floors, manager_id)
            VALUES (@addressId, @neighborhood, @locationId, @numberOfFloors, @managerId)
            RETURNING id";

        AddParameter(buildingCmd, "@addressId", addressId);
        AddParameter(buildingCmd, "@neighborhood", building.Neighborhood);
        AddParameter(buildingCmd, "@locationId", locationId);
        AddParameter(buildingCmd, "@numberOfFloors", building.NumberOfFloors);
        AddParameter(buildingCmd, "@managerId", building.ManagerId);
        long buildingId = Convert.ToInt64(buildingCmd.ExecuteScalar());

        // 4. Upiši spratove i stanove
        foreach (Floor floor in building.Floors)
        {
            IDbCommand floorCmd = connection.CreateCommand();
            floorCmd.CommandText = @"
                INSERT INTO floors (building_id, floor_number)
                VALUES (@buildingId, @floorNumber)
                RETURNING id";

            AddParameter(floorCmd, "@buildingId", buildingId);
            AddParameter(floorCmd, "@floorNumber", floor.FloorNumber);
            long floorId = Convert.ToInt64(floorCmd.ExecuteScalar());

            foreach (Apartment apartment in floor.Apartments)
            {
                IDbCommand aptCmd = connection.CreateCommand();
                aptCmd.CommandText = @"
                    INSERT INTO apartments (floor_id, apartment_number)
                    VALUES (@floorId, @apartmentNumber)
                    RETURNING id";

                AddParameter(aptCmd, "@floorId", floorId);
                AddParameter(aptCmd, "@apartmentNumber", apartment.ApartmentNumber);
                aptCmd.ExecuteScalar();
            }
        }

        // 5. Upiši slike
        foreach (BuildingImage image in building.Images)
        {
            IDbCommand imgCmd = connection.CreateCommand();
            imgCmd.CommandText = @"
                INSERT INTO building_images (building_id, image_path)
                VALUES (@buildingId, @imagePath)
                RETURNING id";

            AddParameter(imgCmd, "@buildingId", buildingId);
            AddParameter(imgCmd, "@imagePath", image.ImagePath);
            imgCmd.ExecuteScalar();
        }

        // Vrati Building objekat sa dodeljenim Id-ovima
        return new Building(
            buildingId,
            new Address(addressId, building.Address.Street, building.Address.StreetNumber),
            building.Neighborhood,
            new Location(locationId, building.Location.City, building.Location.Country),
            building.NumberOfFloors,
            building.ManagerId
        );
    }

    public List<Building> Search(string? address, string? neighborhood, string? city, string? country)
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

        List<Building> buildings = new List<Building>();

        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
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

                buildings.Add(building);
            }
        }

        foreach (Building building in buildings)
        {
            IDbCommand imgCmd = connection.CreateCommand();
            imgCmd.CommandText = "SELECT id, image_path FROM building_images WHERE building_id = @buildingId";
            AddParameter(imgCmd, "@buildingId", building.Id);

            using IDataReader imgReader = imgCmd.ExecuteReader();
            while (imgReader.Read())
            {
                building.AddImage(new BuildingImage(
                    imgReader.GetInt64(imgReader.GetOrdinal("id")),
                    imgReader.GetString(imgReader.GetOrdinal("image_path"))));
            }
        }

        return buildings;
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
