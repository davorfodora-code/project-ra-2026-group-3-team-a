using System.Data;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Database.Repositories.BuildingManager;

public class BulletinBoardStatsDbRepository : IBulletinBoardStatsRepository
{
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
        BulletinBoardStatsQueries.AddParam(cmd, "@managerId", managerId);
        var result = new List<(long, string)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add((r.GetInt64(0), r.GetString(1)));
        return result;
    }

    public List<BulletinBoardAdDto> GetAllAds(long buildingId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ad.id,
                   u.name || ' ' || u.surname        AS tenant_name,
                   a.street || ' ' || a.street_number AS building_address,
                   ad.is_offering, ad.start_date, ad.end_date, ad.category, ad.status
            FROM advertisements ad
            JOIN bulletin_boards bb ON bb.id = ad.bulletin_board_id
            JOIN buildings       b  ON b.id  = bb.building_id
            JOIN addresses       a  ON a.id  = b.address_id
            JOIN users           u  ON u.id  = ad.tenant_id
            WHERE b.id = @buildingId
            ORDER BY ad.start_date DESC";
        BulletinBoardStatsQueries.AddParam(cmd, "@buildingId", buildingId);
        var result = new List<BulletinBoardAdDto>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            result.Add(BulletinBoardStatsQueries.ReadAd(r));
        return result;
    }

    public BulletinBoardStatsDto GetStats(long buildingId, int year, int? month)
    {
        string monthFilter = month.HasValue
            ? "AND EXTRACT(MONTH FROM ad.start_date) = @month"
            : string.Empty;
        var (offeringCount, seekingCount) = BulletinBoardStatsQueries.GetPeriodCounts(buildingId, year, month, monthFilter);
        var categoryStats                 = BulletinBoardStatsQueries.GetCategoryStats(buildingId, year, month, monthFilter);
        var (activeCount, archivedCount)  = BulletinBoardStatsQueries.GetCurrentStateCounts(buildingId);
        var activeByCategory              = BulletinBoardStatsQueries.GetActiveByCategory(buildingId);
        var (topName, topSlots)           = BulletinBoardStatsQueries.GetTopHelper(buildingId);
        return new BulletinBoardStatsDto
        {
            PeriodOfferingCount  = offeringCount,
            PeriodSeekingCount   = seekingCount,
            CategoryStats        = categoryStats,
            CurrentActiveCount   = activeCount,
            CurrentArchivedCount = archivedCount,
            ActiveByCategory     = activeByCategory,
            TopHelperName        = topName,
            TopHelperSlots       = topSlots,
        };
    }
}
