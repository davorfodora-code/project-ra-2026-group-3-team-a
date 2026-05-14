using System.Data;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class BuildingMembershipDbRepository : IBuildingMembershipRepository
{
    public bool IsApartmentOccupied(long buildingId, int apartmentNumber)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM building_memberships
            WHERE building_id = @buildingId
              AND apartment_number = @apartmentNumber
            LIMIT 1";

        AddParameter(cmd, "@buildingId", buildingId);
        AddParameter(cmd, "@apartmentNumber", apartmentNumber);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
