using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public class AdvertisementDbRepository : IAdvertisementRepository
{
    public List<TenantBuildingDto> GetBuildingsByTenant(long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT b.id AS building_id, a.street, a.street_number, b.neighborhood
            FROM building_memberships bm
            JOIN buildings b ON b.id = bm.building_id
            JOIN addresses a ON a.id = b.address_id
            WHERE bm.tenant_id = @tenantId
            ORDER BY a.street, a.street_number";
        AddParameter(cmd, "@tenantId", tenantId);

        List<TenantBuildingDto> buildings = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            buildings.Add(new TenantBuildingDto
            {
                BuildingId = reader.GetInt64(reader.GetOrdinal("building_id")),
                DisplayName = $"{reader.GetString(reader.GetOrdinal("street"))} {reader.GetString(reader.GetOrdinal("street_number"))}, {reader.GetString(reader.GetOrdinal("neighborhood"))}"
            });
        }
        return buildings;
    }

    public long EnsureBulletinBoard(long buildingId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT id FROM bulletin_boards WHERE building_id = @buildingId LIMIT 1";
        AddParameter(selectCmd, "@buildingId", buildingId);

        object? existing = selectCmd.ExecuteScalar();
        if (existing != null && existing != DBNull.Value)
            return Convert.ToInt64(existing);

        IDbCommand insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO bulletin_boards (building_id) VALUES (@buildingId) RETURNING id";
        AddParameter(insertCmd, "@buildingId", buildingId);

        return Convert.ToInt64(insertCmd.ExecuteScalar());
    }

    public long CreateAdvertisement(long bulletinBoardId, long tenantId, string description,
        bool isOffering, DateTime startDate, DateTime endDate, string category)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO advertisements (bulletin_board_id, tenant_id, description, is_offering, start_date, end_date, category, status)
            VALUES (@bulletinBoardId, @tenantId, @description, @isOffering, @startDate, @endDate, @category, 'Aktivan')
            RETURNING id";

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
        using IDbConnection connection = PostgresConnection.CreateConnection();

        DateTime current = startDate.Date;
        while (current <= endDate.Date)
        {
            for (int hour = 16; hour < 20; hour++)
            {
                IDbCommand cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO advertisement_slots (advertisement_id, start_time, end_time, is_available)
                    VALUES (@adId, @startTime, @endTime, TRUE)";

                AddParameter(cmd, "@adId", adId);
                AddParameter(cmd, "@startTime", current.AddHours(hour));
                AddParameter(cmd, "@endTime", current.AddHours(hour + 1));
                cmd.ExecuteNonQuery();
            }
            current = current.AddDays(1);
        }
    }

    public List<Advertisement> GetActiveByBulletinBoard(long bulletinBoardId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT a.id, a.description, a.is_offering, a.start_date, a.end_date, a.category, a.status,
                   a.bulletin_board_id,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role
            FROM advertisements a
            JOIN users u ON u.id = a.tenant_id
            WHERE a.bulletin_board_id = @bulletinBoardId
              AND a.status = 'Aktivan'
            ORDER BY a.start_date";

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
        cmd.CommandText = @"
            SELECT a.id, a.description, a.is_offering, a.start_date, a.end_date, a.category, a.status,
                   a.bulletin_board_id,
                   u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role
            FROM advertisements a
            JOIN users u ON u.id = a.tenant_id
            WHERE a.bulletin_board_id = @bulletinBoardId
              AND a.category = @category
              AND a.is_offering = @oppositeOffering
              AND a.status = 'Aktivan'
              AND a.start_date <= @endDate
              AND a.end_date >= @startDate
              AND a.id != @excludeAdId";

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

    public List<SlotDto> GetAvailableSlots(long adId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, start_time, end_time
            FROM advertisement_slots
            WHERE advertisement_id = @adId AND is_available = TRUE
            ORDER BY start_time";
        AddParameter(cmd, "@adId", adId);

        List<SlotDto> slots = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            slots.Add(new SlotDto
            {
                Id       = reader.GetInt64(reader.GetOrdinal("id")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("start_time")),
                EndTime   = reader.GetDateTime(reader.GetOrdinal("end_time"))
            });
        }
        return slots;
    }

    public void ReserveSlot(long slotId, long requesterAdId, long tenantId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand updateCmd = connection.CreateCommand();
        updateCmd.CommandText = "UPDATE advertisement_slots SET is_available = FALSE WHERE id = @slotId";
        AddParameter(updateCmd, "@slotId", slotId);
        updateCmd.ExecuteNonQuery();

        IDbCommand insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO slot_reservations (slot_id, requester_ad_id, tenant_id)
            VALUES (@slotId, @requesterAdId, @tenantId)";
        AddParameter(insertCmd, "@slotId", slotId);
        AddParameter(insertCmd, "@requesterAdId", requesterAdId);
        AddParameter(insertCmd, "@tenantId", tenantId);
        insertCmd.ExecuteNonQuery();
    }

    public void ArchiveExpiredAds(long bulletinBoardId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE advertisements
            SET status = 'Arhiviran'
            WHERE bulletin_board_id = @bulletinBoardId
              AND status = 'Aktivan'
              AND end_date < @today";

        AddParameter(cmd, "@bulletinBoardId", bulletinBoardId);
        AddParameter(cmd, "@today", DateTime.Today);
        cmd.ExecuteNonQuery();
    }

    private static Advertisement MapAdvertisement(IDataReader reader)
    {
        User tenant = new User(
            reader.GetInt64(reader.GetOrdinal("tenant_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")));

        var placeholderAddress = new Address(0, string.Empty, string.Empty);
        var placeholderLocation = new Location(0, string.Empty, string.Empty);
        var placeholderBuilding = new Building(0, placeholderAddress, string.Empty, placeholderLocation, 0, 0);
        var bulletinBoard = new BulletinBoard(
            reader.GetInt64(reader.GetOrdinal("bulletin_board_id")),
            placeholderBuilding);

        return new Advertisement(
            reader.GetInt64(reader.GetOrdinal("id")),
            bulletinBoard,
            tenant,
            reader.GetString(reader.GetOrdinal("description")),
            reader.GetBoolean(reader.GetOrdinal("is_offering")),
            reader.GetDateTime(reader.GetOrdinal("start_date")),
            reader.GetDateTime(reader.GetOrdinal("end_date")),
            reader.GetString(reader.GetOrdinal("category")),
            reader.GetString(reader.GetOrdinal("status")));
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
