using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories;

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

        // 2. Upiši naselje
        IDbCommand districtCmd = connection.CreateCommand();
        districtCmd.CommandText = @"
            INSERT INTO districts (name)
            VALUES (@name)
            RETURNING id";

        AddParameter(districtCmd, "@name", building.District.Name);
        long districtId = Convert.ToInt64(districtCmd.ExecuteScalar());

        // 3. Upiši adresu
        IDbCommand addressCmd = connection.CreateCommand();
        addressCmd.CommandText = @"
            INSERT INTO addresses (street, street_number)
            VALUES (@street, @streetNumber)
            RETURNING id";

        AddParameter(addressCmd, "@street", building.Address.Street);
        AddParameter(addressCmd, "@streetNumber", building.Address.StreetNumber);
        long addressId = Convert.ToInt64(addressCmd.ExecuteScalar());

        // 4. Upiši zgradu
        IDbCommand buildingCmd = connection.CreateCommand();
        buildingCmd.CommandText = @"
            INSERT INTO buildings (address_id, district_id, location_id, number_of_floors, manager_id)
            VALUES (@addressId, @districtId, @locationId, @numberOfFloors, @managerId)
            RETURNING id";

        AddParameter(buildingCmd, "@addressId", addressId);
        AddParameter(buildingCmd, "@districtId", districtId);
        AddParameter(buildingCmd, "@locationId", locationId);
        AddParameter(buildingCmd, "@numberOfFloors", building.NumberOfFloors);
        AddParameter(buildingCmd, "@managerId", building.ManagerId);
        long buildingId = Convert.ToInt64(buildingCmd.ExecuteScalar());

        // 5. Upiši spratove i stanove
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

        // 6. Upiši slike
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
            new District(districtId, building.District.Name),
            new Location(locationId, building.Location.City, building.Location.Country),
            building.NumberOfFloors,
            building.ManagerId
        );
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
