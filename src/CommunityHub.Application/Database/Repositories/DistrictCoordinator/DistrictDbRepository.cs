using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictDbRepository
{
    public District Create(District district)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        long locationId = InsertLocation(connection, district.Location);
        long districtId = InsertDistrict(connection, district, locationId);
        InsertStreets(connection, district.Streets, districtId);
        InsertImages(connection, district.Images, districtId);

        Location newLocation = new Location(locationId, district.Location.City, district.Location.Country);
        return new District(districtId, district.Name, district.Description, newLocation, district.Budget, district.Coordinator);
    }

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

    private long InsertDistrict(IDbConnection connection, District district, long locationId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO districts (name, description, location_id, budget, coordinator_id)
            VALUES (@name, @description, @locationId, @budget, @coordinatorId)
            RETURNING id";

        AddParameter(cmd, "@name", district.Name);
        AddParameter(cmd, "@description", district.Description);
        AddParameter(cmd, "@locationId", locationId);
        AddParameter(cmd, "@budget", district.Budget);
        AddParameter(cmd, "@coordinatorId", district.Coordinator.Id);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    private void InsertStreets(IDbConnection connection, IEnumerable<DistrictStreet> streets, long districtId)
    {
        foreach (DistrictStreet street in streets)
        {
            InsertSingleStreet(connection, street, districtId);
        }
    }

    private void InsertSingleStreet(IDbConnection connection, DistrictStreet street, long districtId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_streets (district_id, street_name, start_number, end_number)
            VALUES (@districtId, @streetName, @startNumber, @endNumber)";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@streetName", street.StreetName);
        AddParameter(cmd, "@startNumber", street.StartNumber);
        AddParameter(cmd, "@endNumber", street.EndNumber);
        cmd.ExecuteNonQuery();
    }

    private void InsertImages(IDbConnection connection, IEnumerable<Image> images, long districtId)
    {
        foreach (Image image in images)
        {
            InsertSingleImage(connection, image, districtId);
        }
    }

    private void InsertSingleImage(IDbConnection connection, Image image, long districtId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO images (url, district_id)
            VALUES (@url, @districtId)";

        AddParameter(cmd, "@url", image.Url);
        AddParameter(cmd, "@districtId", districtId);
        cmd.ExecuteNonQuery();
    }

    public List<District> GetAll()
    {
        return Search(null, null, null, null);
    }

    public List<District> Search(string? name, string? address, string? city, string? country)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = CreateDistrictSearchCommand(connection, name, address, city, country);
        List<District> districts = ReadDistrictsFromSearchCommand(cmd);

        foreach (District district in districts)
        {
            LoadDistrictStreets(connection, district);
            LoadDistrictImages(connection, district);
        }

        return districts;
    }

    private IDbCommand CreateDistrictSearchCommand(
        IDbConnection connection,
        string? name,
        string? address,
        string? city,
        string? country)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT d.id, d.name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country,
                   u.id AS coordinator_id, u.username, u.password, u.name AS coordinator_name, u.surname, u.birthday
            FROM districts d
            JOIN locations l ON l.id = d.location_id
            JOIN users u ON u.id = d.coordinator_id
            WHERE 1=1";

        AppendDistrictNameFilter(cmd, name);
        AppendCityFilter(cmd, city);
        AppendCountryFilter(cmd, country);
        AppendAddressFilter(cmd, address);

        return cmd;
    }

    private void AppendDistrictNameFilter(IDbCommand cmd, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        cmd.CommandText += " AND d.name ILIKE @name";
        AddParameter(cmd, "@name", "%" + name.Trim() + "%");
    }

    private void AppendCityFilter(IDbCommand cmd, string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return;
        }

        cmd.CommandText += " AND l.city ILIKE @city";
        AddParameter(cmd, "@city", "%" + city.Trim() + "%");
    }

    private void AppendCountryFilter(IDbCommand cmd, string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return;
        }

        cmd.CommandText += " AND l.country ILIKE @country";
        AddParameter(cmd, "@country", "%" + country.Trim() + "%");
    }

    private void AppendAddressFilter(IDbCommand cmd, string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return;
        }

        ParseAddressFilter(address, out string streetPart, out int? number);
        if (string.IsNullOrWhiteSpace(streetPart))
        {
            return;
        }

        cmd.CommandText += @"
                    AND EXISTS (
                        SELECT 1
                        FROM district_streets ds
                        WHERE ds.district_id = d.id
                          AND ds.street_name ILIKE @street";

        AddParameter(cmd, "@street", "%" + streetPart + "%");

        if (number.HasValue)
        {
            cmd.CommandText += " AND @addressNumber BETWEEN ds.start_number AND ds.end_number";
            AddParameter(cmd, "@addressNumber", number.Value);
        }

        cmd.CommandText += ")";
    }

    private static List<District> ReadDistrictsFromSearchCommand(IDbCommand cmd)
    {
        List<District> districts = new List<District>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            districts.Add(MapDistrictSearchRow(reader));
        }

        return districts;
    }

    private static District MapDistrictSearchRow(IDataReader reader)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

        User coordinator = new User(
            reader.GetInt64(reader.GetOrdinal("coordinator_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("coordinator_name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            "DistrictCoordinator");

        return new District(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);
    }

    private void LoadDistrictStreets(IDbConnection connection, District district)
    {
        IDbCommand streetCmd = connection.CreateCommand();
        streetCmd.CommandText = @"
            SELECT id, street_name, start_number, end_number
            FROM district_streets
            WHERE district_id = @districtId";

        AddParameter(streetCmd, "@districtId", district.Id);

        using IDataReader streetReader = streetCmd.ExecuteReader();
        while (streetReader.Read())
        {
            district.AddStreet(new DistrictStreet(
                streetReader.GetInt64(streetReader.GetOrdinal("id")),
                streetReader.GetString(streetReader.GetOrdinal("street_name")),
                streetReader.GetInt32(streetReader.GetOrdinal("start_number")),
                streetReader.GetInt32(streetReader.GetOrdinal("end_number"))));
        }
    }

    private void LoadDistrictImages(IDbConnection connection, District district)
    {
        IDbCommand imageCmd = connection.CreateCommand();
        imageCmd.CommandText = @"
            SELECT id, url
            FROM images
            WHERE district_id = @districtId";

        AddParameter(imageCmd, "@districtId", district.Id);

        using IDataReader imageReader = imageCmd.ExecuteReader();
        while (imageReader.Read())
        {
            district.AddImage(new Image(
                imageReader.GetInt64(imageReader.GetOrdinal("id")),
                imageReader.GetString(imageReader.GetOrdinal("url"))));
        }
    }

    private static void ParseAddressFilter(string input, out string streetPart, out int? number)
    {
        string value = input.Trim();
        number = null;

        int lastSpaceIndex = value.LastIndexOf(' ');
        if (lastSpaceIndex > 0)
        {
            string lastToken = value[(lastSpaceIndex + 1)..];
            if (int.TryParse(lastToken, out int parsedNumber))
            {
                number = parsedNumber;
                streetPart = value[..lastSpaceIndex].Trim();
                return;
            }
        }

        streetPart = value;
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
