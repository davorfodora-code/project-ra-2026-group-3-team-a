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

    // Pomoćna metoda za dodavanje parametara — smanjuje ponavljanje koda
    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
