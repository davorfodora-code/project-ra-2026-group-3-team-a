using System.Data;

namespace CommunityHub.Application.Database.Repositories.Tenant;

public partial class AdvertisementDbRepository
{
    public void CreateMatchingAdNotifications(long newAdId, long bulletinBoardId, bool newAdIsOffering,
        string category, DateTime startDate, DateTime endDate)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO notifications (tenant_id, type, related_ad_id, related_slot_id, is_read, created_at)
            SELECT a.tenant_id, 'Novi podudarni oglas', @newAdId, NULL, FALSE, @now
            FROM advertisements a
            WHERE a.bulletin_board_id = @bulletinBoardId
              AND a.category          = @category
              AND a.is_offering       = @oppositeOffering
              AND a.status            = 'Aktivan'
              AND a.end_date         >= @startDate
              AND a.start_date       <= @endDate
              AND a.id               != @newAdId";
        AddParameter(cmd, "@newAdId",          newAdId);
        AddParameter(cmd, "@bulletinBoardId",  bulletinBoardId);
        AddParameter(cmd, "@category",         category);
        AddParameter(cmd, "@oppositeOffering", !newAdIsOffering);
        AddParameter(cmd, "@startDate",        startDate);
        AddParameter(cmd, "@endDate",          endDate);
        AddParameter(cmd, "@now",              DateTime.UtcNow);
        cmd.ExecuteNonQuery();
    }
}
