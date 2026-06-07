namespace CommunityHub.Application.Database.Repositories.Tenant;

internal static class AdvertisementSql
{
    internal const string GetBuildingsByTenant = @"
        SELECT b.id AS building_id, a.street, a.street_number, b.neighborhood
        FROM building_memberships bm
        JOIN buildings b ON b.id = bm.building_id
        JOIN addresses a ON a.id = b.address_id
        WHERE bm.tenant_id = @tenantId
        ORDER BY a.street, a.street_number";

    internal const string EnsureBulletinBoard = @"
        INSERT INTO bulletin_boards (building_id) VALUES (@buildingId)
        ON CONFLICT (building_id) DO UPDATE SET building_id = EXCLUDED.building_id
        RETURNING id";

    internal const string CreateAdvertisement = @"
        INSERT INTO advertisements (bulletin_board_id, tenant_id, description, is_offering, start_date, end_date, category, status)
        VALUES (@bulletinBoardId, @tenantId, @description, @isOffering, @startDate, @endDate, @category, 'Aktivan')
        RETURNING id";

    internal const string GetActiveByBulletinBoard = @"
        SELECT a.id, a.description, a.is_offering, a.start_date, a.end_date, a.category, a.status,
               a.bulletin_board_id,
               u.id AS tenant_id, u.username, u.password, u.name, u.surname, u.birthday, u.role
        FROM advertisements a
        JOIN users u ON u.id = a.tenant_id
        WHERE a.bulletin_board_id = @bulletinBoardId
          AND a.status = 'Aktivan'
        ORDER BY a.start_date";

    internal const string GetMatchingAds = @"
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

    internal const string ArchiveExpiredAds = @"
        UPDATE advertisements
        SET status = 'Arhiviran'
        WHERE bulletin_board_id = @bulletinBoardId
          AND status = 'Aktivan'
          AND end_date < @today";
}
