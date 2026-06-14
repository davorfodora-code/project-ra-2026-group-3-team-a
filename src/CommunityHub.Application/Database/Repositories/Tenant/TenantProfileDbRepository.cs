using System.Data;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class TenantProfileDbRepository : ITenantProfileRepository
{
    public List<TenantMembershipDto> GetMemberships(long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT b.id, a.street, a.street_number, b.neighborhood, l.city,
                   m.floor_number, m.apartment_number, m.joined_at
            FROM building_memberships m
            JOIN buildings b ON b.id = m.building_id
            JOIN addresses a ON a.id = b.address_id
            JOIN locations l ON l.id = b.location_id
            WHERE m.tenant_id = @tenantId
            ORDER BY m.joined_at DESC";

        AddParameter(cmd, "@tenantId", tenantId);

        var list = new List<TenantMembershipDto>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new TenantMembershipDto
            {
                BuildingId      = reader.GetInt64(0),
                Street          = reader.GetString(1),
                StreetNumber    = reader.GetString(2),
                Neighborhood    = reader.GetString(3),
                City            = reader.GetString(4),
                FloorNumber     = reader.GetInt32(5),
                ApartmentNumber = reader.GetInt32(6),
                JoinedAt        = reader.GetDateTime(7)
            });

        return list;
    }

    private static void AddParameter(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
