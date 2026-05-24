using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictDbRepository : IDistrictRepository
{
    public District Create(District district)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        long locationId = DistrictInsertHelper.InsertLocation(connection, district.Location);
        long districtId = DistrictInsertHelper.InsertDistrict(connection, district, locationId);
        DistrictInsertHelper.InsertStreets(connection, district.Streets, districtId);
        DistrictInsertHelper.InsertImages(connection, district.Images, districtId);

        Location newLocation = new Location(locationId, district.Location.City, district.Location.Country);
        return new District(districtId, district.Name, district.Description, newLocation, district.Budget, district.Coordinator);
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

        DbHelper.AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<District> districts = DistrictDbMapper.ReadDistrictsFromCommand(cmd);
        foreach (District district in districts)
        {
            DistrictDbMapper.LoadStreets(connection, district);
            DistrictDbMapper.LoadImages(connection, district);
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
            DistrictDbMapper.LoadStreets(connection, district);
            DistrictDbMapper.LoadImages(connection, district);
        }

        return districts;
    }
}
