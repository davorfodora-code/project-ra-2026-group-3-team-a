using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal static class CitizenDistrictEventMapper
{
    public static DistrictEvent MapEventShell(IDataReader reader)
    {
        Location location = MapLocation(reader);
        District district = MapDistrict(reader, location);
        User organizer = MapOrganizer(reader);
        TimeSpan startTime = ReadStartTime(reader);
        int durationMinutes = reader.GetInt32(reader.GetOrdinal("duration_minutes"));
        DistrictEventStatus status = Enum.Parse<DistrictEventStatus>(reader.GetString(reader.GetOrdinal("status")));

        return new DistrictEvent(
            reader.GetInt64(reader.GetOrdinal("id")),
            district,
            organizer,
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("description")),
            reader.GetDateTime(reader.GetOrdinal("event_date")),
            startTime,
            TimeSpan.FromMinutes(durationMinutes),
            reader.GetInt32(reader.GetOrdinal("min_volunteers")),
            status,
            new List<DistrictEventRequiredItem>(),
            new List<DistrictEventVolunteerApplication>(),
            new List<DistrictEventVolunteerAttendance>());
    }

    private static Location MapLocation(IDataReader reader)
    {
        return new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));
    }

    private static District MapDistrict(IDataReader reader, Location location)
    {
        User coordinatorStub = new(0, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");
        return new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("district_description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("district_budget")),
            coordinatorStub);
    }

    private static User MapOrganizer(IDataReader reader)
    {
        return new User(
            reader.GetInt64(reader.GetOrdinal("organizer_id")),
            reader.GetString(reader.GetOrdinal("organizer_username")),
            reader.GetString(reader.GetOrdinal("organizer_password")),
            reader.GetString(reader.GetOrdinal("organizer_name")),
            reader.GetString(reader.GetOrdinal("organizer_surname")),
            reader.GetDateTime(reader.GetOrdinal("organizer_birthday")),
            reader.GetString(reader.GetOrdinal("organizer_role")),
            ReadNullableString(reader, "organizer_address"));
    }

    private static TimeSpan ReadStartTime(IDataReader reader)
    {
        object rawTime = reader.GetValue(reader.GetOrdinal("start_time"));
        return rawTime switch
        {
            TimeOnly timeOnly => timeOnly.ToTimeSpan(),
            TimeSpan timeSpan => timeSpan,
            _ => throw new InvalidCastException($"Unexpected type for start_time: {rawTime.GetType()}")
        };
    }

    public static User MapUser(IDataReader reader, string idAlias, string addressAlias)
    {
        return new User(
            reader.GetInt64(reader.GetOrdinal(idAlias)),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")),
            ReadNullableString(reader, addressAlias));
    }

    public static string? ReadNullableString(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static long? ReadNullableInt64(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }
}
