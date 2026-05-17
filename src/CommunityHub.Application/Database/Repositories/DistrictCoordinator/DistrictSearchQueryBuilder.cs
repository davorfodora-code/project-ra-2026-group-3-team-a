using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DistrictSearchQueryBuilder
{
    internal static IDbCommand Build(
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

        AppendNameFilter(cmd, name);
        AppendCityFilter(cmd, city);
        AppendCountryFilter(cmd, country);
        AppendAddressFilter(cmd, address);

        return cmd;
    }

    private static void AppendNameFilter(IDbCommand cmd, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        cmd.CommandText += " AND d.name ILIKE @name";
        AddParameter(cmd, "@name", "%" + name.Trim() + "%");
    }

    private static void AppendCityFilter(IDbCommand cmd, string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return;
        cmd.CommandText += " AND l.city ILIKE @city";
        AddParameter(cmd, "@city", "%" + city.Trim() + "%");
    }

    private static void AppendCountryFilter(IDbCommand cmd, string? country)
    {
        if (string.IsNullOrWhiteSpace(country)) return;
        cmd.CommandText += " AND l.country ILIKE @country";
        AddParameter(cmd, "@country", "%" + country.Trim() + "%");
    }

    private static void AppendAddressFilter(IDbCommand cmd, string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        DistrictDbMapper.ParseAddressFilter(address, out string streetPart, out int? number);
        if (string.IsNullOrWhiteSpace(streetPart)) return;

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

    private static void AddParameter(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
