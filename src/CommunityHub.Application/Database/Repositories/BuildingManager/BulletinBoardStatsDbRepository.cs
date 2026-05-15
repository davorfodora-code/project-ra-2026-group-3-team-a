using System.Data;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class BulletinBoardStatsDbRepository : IBulletinBoardStatsRepository
{
    // ---------------------------------------------------------------
    //  Zgrade kojima upravlja menadžer
    // ---------------------------------------------------------------

    public List<(long BuildingId, string Address)> GetManagedBuildings(long managerId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.id, a.street || ' ' || a.street_number AS address
            FROM buildings b
            JOIN addresses a ON a.id = b.address_id
            WHERE b.manager_id = @managerId
            ORDER BY a.street, a.street_number";
        AddParam(cmd, "@managerId", managerId);

        var result = new List<(long, string)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add((r.GetInt64(0), r.GetString(1)));
        return result;
    }

    // ---------------------------------------------------------------
    //  Svi oglasi za zgradu (aktivni + arhivirani)
    // ---------------------------------------------------------------

    public List<BulletinBoardAdDto> GetAllAds(long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ad.id,
                   u.name || ' ' || u.surname      AS tenant_name,
                   a.street || ' ' || a.street_number AS building_address,
                   ad.is_offering, ad.start_date, ad.end_date, ad.category, ad.status
            FROM advertisements ad
            JOIN bulletin_boards bb ON bb.id  = ad.bulletin_board_id
            JOIN buildings       b  ON b.id   = bb.building_id
            JOIN addresses       a  ON a.id   = b.address_id
            JOIN users           u  ON u.id   = ad.tenant_id
            WHERE b.id = @buildingId
            ORDER BY ad.start_date DESC";
        AddParam(cmd, "@buildingId", buildingId);

        var result = new List<BulletinBoardAdDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(new BulletinBoardAdDto
            {
                Id              = r.GetInt64(r.GetOrdinal("id")),
                TenantFullName  = r.GetString(r.GetOrdinal("tenant_name")),
                BuildingAddress = r.GetString(r.GetOrdinal("building_address")),
                IsOffering      = r.GetBoolean(r.GetOrdinal("is_offering")),
                StartDate       = r.GetDateTime(r.GetOrdinal("start_date")),
                EndDate         = r.GetDateTime(r.GetOrdinal("end_date")),
                Category        = r.GetString(r.GetOrdinal("category")),
                Status          = r.GetString(r.GetOrdinal("status")),
            });
        return result;
    }

    // ---------------------------------------------------------------
    //  Statistika: period + trenutno stanje + top pomagač
    // ---------------------------------------------------------------

    public BulletinBoardStatsDto GetStats(long buildingId, int year, int? month)
    {
        string monthFilter = month.HasValue
            ? "AND EXTRACT(MONTH FROM ad.start_date) = @month"
            : string.Empty;

        var (offeringCount, seekingCount) = GetPeriodCounts(buildingId, year, month, monthFilter);
        var categoryStats                 = GetCategoryStats(buildingId, year, month, monthFilter);
        var (activeCount, archivedCount)  = GetCurrentStateCounts(buildingId);
        var activeByCategory              = GetActiveByCategory(buildingId);
        var (topName, topSlots)           = GetTopHelper(buildingId);

        return new BulletinBoardStatsDto
        {
            PeriodOfferingCount = offeringCount,
            PeriodSeekingCount  = seekingCount,
            CategoryStats       = categoryStats,
            CurrentActiveCount  = activeCount,
            CurrentArchivedCount = archivedCount,
            ActiveByCategory    = activeByCategory,
            TopHelperName       = topName,
            TopHelperSlots      = topSlots,
        };
    }

    // ---------------------------------------------------------------
    //  Privatne metode
    // ---------------------------------------------------------------

    private static (int Offering, int Seeking) GetPeriodCounts(
        long buildingId, int year, int? month, string monthFilter)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT COUNT(*) FILTER (WHERE ad.is_offering = true)  AS offering_count,
                   COUNT(*) FILTER (WHERE ad.is_offering = false) AS seeking_count
            FROM advertisements ad
            JOIN bulletin_boards bb ON bb.id = ad.bulletin_board_id
            JOIN buildings       b  ON b.id  = bb.building_id
            WHERE b.id = @buildingId
              AND EXTRACT(YEAR FROM ad.start_date) = @year
              {monthFilter}";
        AddParam(cmd, "@buildingId", buildingId);
        AddParam(cmd, "@year",       year);
        if (month.HasValue) AddParam(cmd, "@month", month.Value);

        using IDataReader r = cmd.ExecuteReader();
        if (!r.Read()) return (0, 0);
        return (r.GetInt32(0), r.GetInt32(1));
    }

    private static List<CategoryStatsDto> GetCategoryStats(
        long buildingId, int year, int? month, string monthFilter)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT ad.category,
                   COUNT(*) FILTER (WHERE ad.is_offering = true)  AS offering_count,
                   COUNT(*) FILTER (WHERE ad.is_offering = false) AS seeking_count
            FROM advertisements ad
            JOIN bulletin_boards bb ON bb.id = ad.bulletin_board_id
            JOIN buildings       b  ON b.id  = bb.building_id
            WHERE b.id = @buildingId
              AND EXTRACT(YEAR FROM ad.start_date) = @year
              {monthFilter}
            GROUP BY ad.category
            ORDER BY ad.category";
        AddParam(cmd, "@buildingId", buildingId);
        AddParam(cmd, "@year",       year);
        if (month.HasValue) AddParam(cmd, "@month", month.Value);

        var result = new List<CategoryStatsDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(new CategoryStatsDto
            {
                Category     = r.GetString(0),
                OfferingCount = r.GetInt32(1),
                SeekingCount  = r.GetInt32(2),
            });
        return result;
    }

    private static (int Active, int Archived) GetCurrentStateCounts(long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FILTER (WHERE ad.status = 'Aktivan')   AS active_count,
                   COUNT(*) FILTER (WHERE ad.status = 'Arhiviran') AS archived_count
            FROM advertisements ad
            JOIN bulletin_boards bb ON bb.id = ad.bulletin_board_id
            JOIN buildings       b  ON b.id  = bb.building_id
            WHERE b.id = @buildingId";
        AddParam(cmd, "@buildingId", buildingId);

        using IDataReader r = cmd.ExecuteReader();
        if (!r.Read()) return (0, 0);
        return (r.GetInt32(0), r.GetInt32(1));
    }

    private static List<(string Category, int Count)> GetActiveByCategory(long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ad.category, COUNT(*) AS cnt
            FROM advertisements ad
            JOIN bulletin_boards bb ON bb.id = ad.bulletin_board_id
            JOIN buildings       b  ON b.id  = bb.building_id
            WHERE b.id = @buildingId AND ad.status = 'Aktivan'
            GROUP BY ad.category
            ORDER BY ad.category";
        AddParam(cmd, "@buildingId", buildingId);

        var result = new List<(string, int)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add((r.GetString(0), r.GetInt32(1)));
        return result;
    }

    private static (string? Name, int Slots) GetTopHelper(long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.name || ' ' || u.surname AS full_name, COUNT(s.id) AS slot_count
            FROM advertisement_slots s
            JOIN advertisements  ad ON ad.id  = s.advertisement_id
            JOIN bulletin_boards bb ON bb.id  = ad.bulletin_board_id
            JOIN buildings       b  ON b.id   = bb.building_id
            JOIN users           u  ON u.id   = ad.tenant_id
            WHERE b.id = @buildingId
              AND ad.is_offering   = true
              AND s.is_available   = false
            GROUP BY u.id, u.name, u.surname
            ORDER BY slot_count DESC
            LIMIT 1";
        AddParam(cmd, "@buildingId", buildingId);

        using IDataReader r = cmd.ExecuteReader();
        if (!r.Read()) return (null, 0);
        return (r.GetString(0), r.GetInt32(1));
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
