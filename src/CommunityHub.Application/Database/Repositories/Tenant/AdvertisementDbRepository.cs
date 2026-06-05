using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public partial class AdvertisementDbRepository : IAdvertisementRepository
{
    public List<TenantBuildingDto> GetBuildingsByTenant(long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = AdvertisementSql.GetBuildingsByTenant;
        AddParameter(cmd, "@tenantId", tenantId);
        List<TenantBuildingDto> buildings = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            buildings.Add(new TenantBuildingDto
            {
                BuildingId  = reader.GetInt64(reader.GetOrdinal("building_id")),
                DisplayName = $"{reader.GetString(reader.GetOrdinal("street"))} {reader.GetString(reader.GetOrdinal("street_number"))}, {reader.GetString(reader.GetOrdinal("neighborhood"))}"
            });
        return buildings;
    }

    public long EnsureBulletinBoard(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = AdvertisementSql.EnsureBulletinBoard;
        AddParameter(cmd, "@buildingId", buildingId);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public long CreateAdvertisement(long bulletinBoardId, long tenantId, string description,
        bool isOffering, DateTime startDate, DateTime endDate, string category)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = AdvertisementSql.CreateAdvertisement;
        AddParameter(cmd, "@bulletinBoardId", bulletinBoardId);
        AddParameter(cmd, "@tenantId", tenantId);
        AddParameter(cmd, "@description", description);
        AddParameter(cmd, "@isOffering", isOffering);
        AddParameter(cmd, "@startDate", startDate);
        AddParameter(cmd, "@endDate", endDate);
        AddParameter(cmd, "@category", category);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public void CreateSlotsForAd(long adId, DateTime startDate, DateTime endDate)
    {
        var slots = new List<(DateTime Start, DateTime End)>();
        for (DateTime d = startDate.Date; d <= endDate.Date; d = d.AddDays(1))
            for (int h = 16; h < 20; h++)
                slots.Add((d.AddHours(h), d.AddHours(h + 1)));
        if (slots.Count == 0) return;
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        var values = new List<string>(slots.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            values.Add($"(@adId, @s{i}, @e{i}, TRUE)");
            AddParameter(cmd, $"@s{i}", slots[i].Start);
            AddParameter(cmd, $"@e{i}", slots[i].End);
        }
        AddParameter(cmd, "@adId", adId);
        cmd.CommandText = $"INSERT INTO advertisement_slots (advertisement_id, start_time, end_time, is_available) VALUES {string.Join(", ", values)}";
        cmd.ExecuteNonQuery();
    }

    public List<Advertisement> GetActiveByBulletinBoard(long bulletinBoardId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = AdvertisementSql.GetActiveByBulletinBoard;
        AddParameter(cmd, "@bulletinBoardId", bulletinBoardId);
        List<Advertisement> ads = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            ads.Add(MapAdvertisement(reader));
        return ads;
    }

    public List<Advertisement> GetMatchingAds(long bulletinBoardId, bool isOffering,
        DateTime startDate, DateTime endDate, string category, long excludeAdId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = AdvertisementSql.GetMatchingAds;
        AddParameter(cmd, "@bulletinBoardId", bulletinBoardId);
        AddParameter(cmd, "@category", category);
        AddParameter(cmd, "@oppositeOffering", !isOffering);
        AddParameter(cmd, "@startDate", startDate);
        AddParameter(cmd, "@endDate", endDate);
        AddParameter(cmd, "@excludeAdId", excludeAdId);
        List<Advertisement> ads = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            ads.Add(MapAdvertisement(reader));
        return ads;
    }

    public void ArchiveAd(long adId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE advertisements SET status = 'Arhiviran' WHERE id = @adId";
        AddParameter(cmd, "@adId", adId);
        cmd.ExecuteNonQuery();
    }

    public void ArchiveExpiredAds(long bulletinBoardId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = AdvertisementSql.ArchiveExpiredAds;
        AddParameter(cmd, "@bulletinBoardId", bulletinBoardId);
        AddParameter(cmd, "@today", DateTime.Today);
        cmd.ExecuteNonQuery();
    }

    private static Advertisement MapAdvertisement(IDataReader reader) =>
        new Advertisement(
            reader.GetInt64(reader.GetOrdinal("id")),
            new BulletinBoard(
                reader.GetInt64(reader.GetOrdinal("bulletin_board_id")),
                new Building(0, new Address(0, "", ""), "", new Location(0, "", ""), 0, 0)),
            new User(
                reader.GetInt64(reader.GetOrdinal("tenant_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role"))),
            reader.GetString(reader.GetOrdinal("description")),
            reader.GetBoolean(reader.GetOrdinal("is_offering")),
            reader.GetDateTime(reader.GetOrdinal("start_date")),
            reader.GetDateTime(reader.GetOrdinal("end_date")),
            reader.GetString(reader.GetOrdinal("category")),
            reader.GetString(reader.GetOrdinal("status")));

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
