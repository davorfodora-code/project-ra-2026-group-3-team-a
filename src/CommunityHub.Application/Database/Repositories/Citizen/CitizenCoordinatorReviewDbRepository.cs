using System.Data;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenCoordinatorReviewDbRepository : ICitizenCoordinatorReviewRepository
{
    public List<CoordinatorReviewDto> GetActiveReviewsByDistrict(long districtId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT
                r.id,
                r.district_id,
                r.coordinator_id,
                r.author_id,
                CONCAT(u.name, ' ', u.surname) AS author_full_name,
                r.rating,
                COALESCE(r.justification, '') AS justification,
                r.created_at,
                COALESCE(report_stats.report_count, 0) AS report_count,
                COALESCE(report_stats.trusted_report_count, 0) AS trusted_report_count,
                CASE WHEN r.author_id = @citizenId THEN TRUE ELSE FALSE END AS is_mine,
                CASE WHEN my_report.id IS NULL THEN FALSE ELSE TRUE END AS is_reported_by_me
            FROM district_coordinator_reviews r
            JOIN users u ON u.id = r.author_id
            LEFT JOIN district_coordinator_review_reports my_report
                ON my_report.review_id = r.id AND my_report.reporter_id = @citizenId
            LEFT JOIN (
                SELECT
                    rr.review_id,
                    COUNT(*) AS report_count,
                    COUNT(*) FILTER (
                        WHERE dm.trust_level IN ('Istaknuti', 'OdPoverenja')
                    ) AS trusted_report_count
                FROM district_coordinator_review_reports rr
                JOIN district_coordinator_reviews cr ON cr.id = rr.review_id
                LEFT JOIN district_memberships dm
                    ON dm.user_id = rr.reporter_id
                   AND dm.district_id = cr.district_id
                GROUP BY rr.review_id
            ) report_stats ON report_stats.review_id = r.id
            WHERE r.district_id = @districtId
              AND r.status = 'Active'
            ORDER BY r.created_at DESC";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@citizenId", citizenId);

        List<CoordinatorReviewDto> reviews = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            reviews.Add(MapReview(reader));
        }

        return reviews;
    }

    public CoordinatorReviewDto? GetReviewById(long reviewId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT
                r.id,
                r.district_id,
                r.coordinator_id,
                r.author_id,
                CONCAT(u.name, ' ', u.surname) AS author_full_name,
                r.rating,
                COALESCE(r.justification, '') AS justification,
                r.created_at,
                COALESCE(report_stats.report_count, 0) AS report_count,
                COALESCE(report_stats.trusted_report_count, 0) AS trusted_report_count,
                CASE WHEN r.author_id = @citizenId THEN TRUE ELSE FALSE END AS is_mine,
                CASE WHEN my_report.id IS NULL THEN FALSE ELSE TRUE END AS is_reported_by_me
            FROM district_coordinator_reviews r
            JOIN users u ON u.id = r.author_id
            LEFT JOIN district_coordinator_review_reports my_report
                ON my_report.review_id = r.id AND my_report.reporter_id = @citizenId
            LEFT JOIN (
                SELECT
                    rr.review_id,
                    COUNT(*) AS report_count,
                    COUNT(*) FILTER (
                        WHERE dm.trust_level IN ('Istaknuti', 'OdPoverenja')
                    ) AS trusted_report_count
                FROM district_coordinator_review_reports rr
                JOIN district_coordinator_reviews cr ON cr.id = rr.review_id
                LEFT JOIN district_memberships dm
                    ON dm.user_id = rr.reporter_id
                   AND dm.district_id = cr.district_id
                GROUP BY rr.review_id
            ) report_stats ON report_stats.review_id = r.id
            WHERE r.id = @reviewId
              AND r.status = 'Active'";

        AddParameter(cmd, "@reviewId", reviewId);
        AddParameter(cmd, "@citizenId", citizenId);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapReview(reader) : null;
    }

    public long Create(long districtId, long coordinatorId, long authorId, int rating, string? justification, DateTime createdAt)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO district_coordinator_reviews
                (district_id, coordinator_id, author_id, rating, justification, created_at, status)
            VALUES
                (@districtId, @coordinatorId, @authorId, @rating, @justification, @createdAt, 'Active')
            RETURNING id";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@coordinatorId", coordinatorId);
        AddParameter(cmd, "@authorId", authorId);
        AddParameter(cmd, "@rating", rating);
        AddParameter(cmd, "@justification", (object?)justification ?? DBNull.Value);
        AddParameter(cmd, "@createdAt", createdAt);

        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public long GetDistrictCoordinatorId(long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT coordinator_id
            FROM districts
            WHERE id = @districtId";

        AddParameter(cmd, "@districtId", districtId);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public bool HasReviewInMonth(long districtId, long authorId, int year, int month)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT 1
            FROM district_coordinator_reviews
            WHERE district_id = @districtId
              AND author_id = @authorId
              AND status = 'Active'
              AND EXTRACT(YEAR FROM created_at) = @year
              AND EXTRACT(MONTH FROM created_at) = @month
            LIMIT 1";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@authorId", authorId);
        AddParameter(cmd, "@year", year);
        AddParameter(cmd, "@month", month);

        return cmd.ExecuteScalar() != null;
    }

    public int CountRemovedReviewsByAuthor(long authorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT COUNT(*)
            FROM district_coordinator_reviews
            WHERE author_id = @authorId
              AND status = 'Removed'";

        AddParameter(cmd, "@authorId", authorId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool ReviewBelongsToDistrict(long reviewId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT 1
            FROM district_coordinator_reviews
            WHERE id = @reviewId
              AND district_id = @districtId
              AND status = 'Active'
            LIMIT 1";

        AddParameter(cmd, "@reviewId", reviewId);
        AddParameter(cmd, "@districtId", districtId);

        return cmd.ExecuteScalar() != null;
    }

    public bool HasReport(long reviewId, long reporterId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT 1
            FROM district_coordinator_review_reports
            WHERE review_id = @reviewId
              AND reporter_id = @reporterId
            LIMIT 1";

        AddParameter(cmd, "@reviewId", reviewId);
        AddParameter(cmd, "@reporterId", reporterId);

        return cmd.ExecuteScalar() != null;
    }

    public void AddReport(long reviewId, long reporterId, DateTime reportedAt)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO district_coordinator_review_reports
                (review_id, reporter_id, reported_at)
            VALUES
                (@reviewId, @reporterId, @reportedAt)
            ON CONFLICT (review_id, reporter_id) DO NOTHING";

        AddParameter(cmd, "@reviewId", reviewId);
        AddParameter(cmd, "@reporterId", reporterId);
        AddParameter(cmd, "@reportedAt", reportedAt);

        cmd.ExecuteNonQuery();
    }

    public int CountReports(long reviewId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT COUNT(*)
            FROM district_coordinator_review_reports
            WHERE review_id = @reviewId";

        AddParameter(cmd, "@reviewId", reviewId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int CountTrustedReports(long reviewId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            SELECT COUNT(*)
            FROM district_coordinator_review_reports rr
            JOIN district_coordinator_reviews r ON r.id = rr.review_id
            JOIN district_memberships dm
                ON dm.user_id = rr.reporter_id
               AND dm.district_id = r.district_id
            WHERE rr.review_id = @reviewId
              AND dm.trust_level IN ('Istaknuti', 'OdPoverenja')";

        AddParameter(cmd, "@reviewId", reviewId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void MarkRemoved(long reviewId, DateTime removedAt)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        IDbCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            UPDATE district_coordinator_reviews
            SET status = 'Removed',
                removed_at = @removedAt
            WHERE id = @reviewId";

        AddParameter(cmd, "@reviewId", reviewId);
        AddParameter(cmd, "@removedAt", removedAt);

        cmd.ExecuteNonQuery();
    }

    private static CoordinatorReviewDto MapReview(IDataReader reader)
    {
        return new CoordinatorReviewDto
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            DistrictId = reader.GetInt64(reader.GetOrdinal("district_id")),
            CoordinatorId = reader.GetInt64(reader.GetOrdinal("coordinator_id")),
            AuthorId = reader.GetInt64(reader.GetOrdinal("author_id")),
            AuthorFullName = reader.GetString(reader.GetOrdinal("author_full_name")).Trim(),
            Rating = reader.GetInt32(reader.GetOrdinal("rating")),
            Justification = reader.GetString(reader.GetOrdinal("justification")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            ReportCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("report_count"))),
            TrustedReportCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("trusted_report_count"))),
            IsMine = reader.GetBoolean(reader.GetOrdinal("is_mine")),
            IsReportedByMe = reader.GetBoolean(reader.GetOrdinal("is_reported_by_me"))
        };
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}