using System;
using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class DistrictAccessRequestDbMapper
{
    internal static string? ReadNullableString(IDataReader reader, string column)
    {
        int ord = reader.GetOrdinal(column);
        return reader.IsDBNull(ord) ? null : reader.GetString(ord);
    }

    internal static User MapUserFromReader(IDataReader reader)
    {
        string? userAddress = ReadNullableString(reader, "address");
        return new User(
            reader.GetInt64(reader.GetOrdinal("user_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")),
            userAddress);
    }

    internal static District MapDistrictFromReader(IDataReader reader, long coordinatorId)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));
        User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");
        return new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);
    }

    internal static District MapDistrictBasic(IDataReader reader, long coordinatorId)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));
        User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");
        return new District(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);
    }

    internal static DistrictAccessRequest MapAccessRequest(IDataReader reader, long coordinatorId)
    {
        User user = MapUserFromReader(reader);
        District district = MapDistrictFromReader(reader, coordinatorId);
        DistrictAccessRequestStatus status = Enum.Parse<DistrictAccessRequestStatus>(
            reader.GetString(reader.GetOrdinal("status")));
        string? rejectionReason = ReadNullableString(reader, "rejection_reason");
        return new DistrictAccessRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            user, district,
            reader.GetDateTime(reader.GetOrdinal("creation_date")),
            status, rejectionReason);
    }

    internal static DistrictMembership MapMembership(IDataReader reader)
    {
        User user = MapUserFromReader(reader);
        int trustOrd = reader.GetOrdinal("trust_level");
        CitizenTrustLevel trustLevel = reader.IsDBNull(trustOrd)
            ? CitizenTrustLevel.Nov
            : Enum.Parse<CitizenTrustLevel>(reader.GetString(trustOrd));
        return new DistrictMembership(
            reader.GetInt64(reader.GetOrdinal("id")),
            null!,
            user,
            reader.GetDateTime(reader.GetOrdinal("join_date")),
            trustLevel);
    }
}
