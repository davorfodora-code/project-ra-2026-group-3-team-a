using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal static class CitizenDistrictAccessRequestMapper
{
    public static DistrictAccessRequest MapAccessRequestRow(IDataReader reader)
    {
        User user = ReadUserFromAccessRequestRow(reader);
        Location location = ReadLocationFromAccessRequestRow(reader);
        District district = ReadDistrictFromAccessRequestRow(reader, location);

        DistrictAccessRequestStatus status = ReadAccessRequestStatus(reader);
        string? rejectionReason = ReadNullableString(reader, "rejection_reason");

        return new DistrictAccessRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            user,
            district,
            reader.GetDateTime(reader.GetOrdinal("creation_date")),
            status,
            rejectionReason);
    }

    private static User ReadUserFromAccessRequestRow(IDataReader reader)
    {
        string? address = ReadNullableString(reader, "address");

        return new User(
            reader.GetInt64(reader.GetOrdinal("user_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")),
            address);
    }

    private static Location ReadLocationFromAccessRequestRow(IDataReader reader)
    {
        return new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));
    }

    private static District ReadDistrictFromAccessRequestRow(IDataReader reader, Location location)
    {
        User coordinatorStub = new(0, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        return new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinatorStub);
    }

    private static DistrictAccessRequestStatus ReadAccessRequestStatus(IDataReader reader)
    {
        string statusStr = reader.GetString(reader.GetOrdinal("status"));
        return Enum.Parse<DistrictAccessRequestStatus>(statusStr);
    }

    private static string? ReadNullableString(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
