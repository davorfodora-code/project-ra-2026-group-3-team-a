using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

// Pomocne metode i SQL konstante koje koristi AssemblyDbRepository.
internal static class AssemblyDbHelpers
{
    // Zajednicki SELECT za skupstine + osnovni building/address/location join + LEFT JOIN tema
    public const string SelectAssemblyJoins = @"
            SELECT ta.id, ta.scheduled_at, ta.status,
                   b.id AS building_id, b.neighborhood, b.number_of_floors, b.manager_id,
                   a.id AS address_id, a.street, a.street_number,
                   l.id AS location_id, l.city, l.country,
                   t.id AS topic_id, t.text AS topic_text, t.is_additional
            FROM tenant_assemblies ta
            JOIN buildings  b ON b.id = ta.building_id
            JOIN addresses  a ON a.id = b.address_id
            JOIN locations  l ON l.id = b.location_id
            LEFT JOIN assembly_topics t ON t.assembly_id = ta.id ";

    // Mapira jedan red iz SelectAssemblyJoins u domain TenantAssembly objekat
    public static TenantAssembly ReadAssembly(IDataReader r)
    {
        Address  address  = new Address(r.GetInt64(r.GetOrdinal("address_id")),
                                        r.GetString(r.GetOrdinal("street")),
                                        r.GetString(r.GetOrdinal("street_number")));
        Location location = new Location(r.GetInt64(r.GetOrdinal("location_id")),
                                         r.GetString(r.GetOrdinal("city")),
                                         r.GetString(r.GetOrdinal("country")));
        Building building = new Building(r.GetInt64(r.GetOrdinal("building_id")),
                                         address,
                                         r.GetString(r.GetOrdinal("neighborhood")),
                                         location,
                                         r.GetInt32(r.GetOrdinal("number_of_floors")),
                                         r.GetInt64(r.GetOrdinal("manager_id")));
        return new TenantAssembly(r.GetInt64(r.GetOrdinal("id")),
                                  building,
                                  r.GetDateTime(r.GetOrdinal("scheduled_at")),
                                  r.GetString(r.GetOrdinal("status")));
    }

    // Cita topic_id/text/is_additional kolone (mogu biti NULL u LEFT JOIN-u)
    public static AssemblyTopic? TryReadTopic(IDataReader r)
    {
        int idOrd = r.GetOrdinal("topic_id");
        if (r.IsDBNull(idOrd)) return null;
        return new AssemblyTopic(
            r.GetInt64(idOrd),
            r.GetString(r.GetOrdinal("topic_text")),
            r.GetBoolean(r.GetOrdinal("is_additional")));
    }

    public static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
