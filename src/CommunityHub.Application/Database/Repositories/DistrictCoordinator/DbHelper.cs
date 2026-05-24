using System.Data;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DbHelper
{
    internal static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
