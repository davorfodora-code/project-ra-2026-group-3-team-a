using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictDbRepository : IDistrictRepository
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
            InsertSingleStreet(connection, street, districtId);
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
            InsertSingleImage(connection, image, districtId);
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

    public List<District> GetAll() => Search(null, null, null, null);

    public List<District> GetByCoordinatorId(long coordinatorId)
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
            WHERE d.coordinator_id = @coordinatorId
            ORDER BY d.name";

        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<District> districts = DistrictDbMapper.ReadDistrictsFromCommand(cmd);
        foreach (District district in districts)
        {
            LoadDistrictStreets(connection, district);
            LoadDistrictImages(connection, district);
        }
        return districts;
    }

    public List<District> Search(string? name, string? address, string? city, string? country)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = DistrictSearchQueryBuilder.Build(connection, name, address, city, country);
        List<District> districts = DistrictDbMapper.ReadDistrictsFromCommand(cmd);

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

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
