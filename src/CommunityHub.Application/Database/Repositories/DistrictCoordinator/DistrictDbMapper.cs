using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DistrictDbMapper
{
    internal static void LoadStreets(IDbConnection connection, District district)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, street_name, start_number, end_number
            FROM district_streets
            WHERE district_id = @districtId";

        DbHelper.AddParameter(cmd, "@districtId", district.Id);

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            district.AddStreet(new DistrictStreet(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("street_name")),
                reader.GetInt32(reader.GetOrdinal("start_number")),
                reader.GetInt32(reader.GetOrdinal("end_number"))));
        }
    }

    internal static void LoadImages(IDbConnection connection, District district)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, url
            FROM images
            WHERE district_id = @districtId";

        DbHelper.AddParameter(cmd, "@districtId", district.Id);

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            district.AddImage(new Image(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("url"))));
        }
    }


    internal static Location MapLocation(IDataReader reader) =>
        new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

    internal static User MapCoordinator(IDataReader reader) =>
        new User(
            reader.GetInt64(reader.GetOrdinal("coordinator_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("coordinator_name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            "DistrictCoordinator");

    internal static District MapDistrictSearchRow(IDataReader reader) =>
        new District(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("description")),
            MapLocation(reader),
            reader.GetDecimal(reader.GetOrdinal("budget")),
            MapCoordinator(reader));

    internal static List<District> ReadDistrictsFromCommand(IDbCommand cmd)
    {
        List<District> districts = new List<District>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            districts.Add(MapDistrictSearchRow(reader));
        return districts;
    }

    internal static void ParseAddressFilter(string input, out string streetPart, out int? number)
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
}
