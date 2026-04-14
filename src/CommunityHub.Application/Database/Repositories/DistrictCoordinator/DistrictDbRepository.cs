using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictDbRepository
{
    public District Create(District district)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        // 1. Upiši lokaciju
        IDbCommand locationCmd = connection.CreateCommand();
        locationCmd.CommandText = @"
            INSERT INTO locations (city, country)
            VALUES (@city, @country)
            RETURNING id";

        AddParameter(locationCmd, "@city", district.Location.City);
        AddParameter(locationCmd, "@country", district.Location.Country);
        long locationId = Convert.ToInt64(locationCmd.ExecuteScalar());

        // 2. Upiši kvart
        IDbCommand districtCmd = connection.CreateCommand();
        districtCmd.CommandText = @"
            INSERT INTO districts (name, description, location_id, budget, coordinator_id)
            VALUES (@name, @description, @locationId, @budget, @coordinatorId)
            RETURNING id";

        AddParameter(districtCmd, "@name", district.Name);
        AddParameter(districtCmd, "@description", district.Description);
        AddParameter(districtCmd, "@locationId", locationId);
        AddParameter(districtCmd, "@budget", district.Budget);
        AddParameter(districtCmd, "@coordinatorId", district.Coordinator.Id);
        long districtId = Convert.ToInt64(districtCmd.ExecuteScalar());

        // 3. Upiši ulice
        foreach (DistrictStreet street in district.Streets)
        {
            IDbCommand streetCmd = connection.CreateCommand();
            streetCmd.CommandText = @"
                INSERT INTO district_streets (district_id, street_name, start_number, end_number)
                VALUES (@districtId, @streetName, @startNumber, @endNumber)
                RETURNING id";

            AddParameter(streetCmd, "@districtId", districtId);
            AddParameter(streetCmd, "@streetName", street.StreetName);
            AddParameter(streetCmd, "@startNumber", street.StartNumber);
            AddParameter(streetCmd, "@endNumber", street.EndNumber);
            streetCmd.ExecuteScalar();
        }

        // 4. Upiši slike
        foreach (Image image in district.Images)
        {
            IDbCommand imageCmd = connection.CreateCommand();
            imageCmd.CommandText = @"
                INSERT INTO images (url, district_id)
                VALUES (@url, @districtId)
                RETURNING id";

            AddParameter(imageCmd, "@url", image.Url);
            AddParameter(imageCmd, "@districtId", districtId);
            imageCmd.ExecuteScalar();
        }

        // Vrati District objekat sa dodeljenim Id-ovima
        return new District(
            districtId,
            district.Name,
            district.Description,
            new Location(locationId, district.Location.City, district.Location.Country),
            district.Budget,
            district.Coordinator
        );
    }

    public List<District> GetAll()
    {
        return Search(null, null, null, null);
    }

    public List<District> Search(string? name, string? address, string? city, string? country)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT d.id, d.name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country,
                   u.id AS coordinator_id, u.username, u.password, u.name AS coordinator_name, u.surname, u.birthday
            FROM districts d
            JOIN locations l ON l.id = d.location_id
            JOIN users u ON u.id = d.coordinator_id
            WHERE 1=1";

        if (!string.IsNullOrWhiteSpace(name))
        {
            cmd.CommandText += " AND d.name ILIKE @name";
            AddParameter(cmd, "@name", "%" + name.Trim() + "%");
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            cmd.CommandText += " AND l.city ILIKE @city";
            AddParameter(cmd, "@city", "%" + city.Trim() + "%");
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            cmd.CommandText += " AND l.country ILIKE @country";
            AddParameter(cmd, "@country", "%" + country.Trim() + "%");
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            ParseAddressFilter(address, out string streetPart, out int? number);

            if (!string.IsNullOrWhiteSpace(streetPart))
            {
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
        }

        List<District> districts = new List<District>();

        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
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

                District district = new District(
                    reader.GetInt64(reader.GetOrdinal("id")),
                    reader.GetString(reader.GetOrdinal("name")),
                    reader.GetString(reader.GetOrdinal("description")),
                    location,
                    reader.GetDecimal(reader.GetOrdinal("budget")),
                    coordinator);

                districts.Add(district);
            }
        }

        foreach (District district in districts)
        {
            LoadDistrictStreets(connection, district);
            LoadDistrictImages(connection, district);
        }

        return districts;
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
