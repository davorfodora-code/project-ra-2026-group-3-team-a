using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DistrictInsertHelper
{
    internal static long InsertLocation(IDbConnection connection, Location location)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO locations (city, country)
            VALUES (@city, @country)
            RETURNING id";

        DbHelper.AddParameter(cmd, "@city", location.City);
        DbHelper.AddParameter(cmd, "@country", location.Country);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    internal static long InsertDistrict(IDbConnection connection, District district, long locationId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO districts (name, description, location_id, budget, coordinator_id)
            VALUES (@name, @description, @locationId, @budget, @coordinatorId)
            RETURNING id";

        DbHelper.AddParameter(cmd, "@name", district.Name);
        DbHelper.AddParameter(cmd, "@description", district.Description);
        DbHelper.AddParameter(cmd, "@locationId", locationId);
        DbHelper.AddParameter(cmd, "@budget", district.Budget);
        DbHelper.AddParameter(cmd, "@coordinatorId", district.Coordinator.Id);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    internal static void InsertStreets(IDbConnection connection, IEnumerable<DistrictStreet> streets, long districtId)
    {
        foreach (DistrictStreet street in streets)
            InsertSingleStreet(connection, street, districtId);
    }

    internal static void InsertImages(IDbConnection connection, IEnumerable<Image> images, long districtId)
    {
        foreach (Image image in images)
            InsertSingleImage(connection, image, districtId);
    }

    private static void InsertSingleStreet(IDbConnection connection, DistrictStreet street, long districtId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_streets (district_id, street_name, start_number, end_number)
            VALUES (@districtId, @streetName, @startNumber, @endNumber)";

        DbHelper.AddParameter(cmd, "@districtId", districtId);
        DbHelper.AddParameter(cmd, "@streetName", street.StreetName);
        DbHelper.AddParameter(cmd, "@startNumber", street.StartNumber);
        DbHelper.AddParameter(cmd, "@endNumber", street.EndNumber);
        cmd.ExecuteNonQuery();
    }

    private static void InsertSingleImage(IDbConnection connection, Image image, long districtId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO images (url, district_id)
            VALUES (@url, @districtId)";

        DbHelper.AddParameter(cmd, "@url", image.Url);
        DbHelper.AddParameter(cmd, "@districtId", districtId);
        cmd.ExecuteNonQuery();
    }
}
