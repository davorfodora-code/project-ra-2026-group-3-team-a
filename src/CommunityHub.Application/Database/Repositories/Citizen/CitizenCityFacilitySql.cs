namespace CommunityHub.Application.Database.Repositories.Citizen;

internal static class CitizenCityFacilitySql
{
    public const string FacilitiesByDistrict = FacilitySelect + @"
            ORDER BY vote_count DESC, o.name";

    public const string FacilityById = FacilitySelect + @"
            WHERE o.id = @facilityId";

    public const string Vote = @"
            INSERT INTO city_object_votes (city_object_id, citizen_id, voted_at)
            VALUES (@facilityId, @citizenId, @votedAt)
            ON CONFLICT (city_object_id, citizen_id) DO NOTHING";

    public const string HasVote = @"
            SELECT 1
            FROM city_object_votes
            WHERE city_object_id = @facilityId AND citizen_id = @citizenId
            LIMIT 1";

    public const string FacilityBelongsToDistrict = @"
            SELECT 1
            FROM city_objects
            WHERE id = @facilityId
            LIMIT 1";

    public const string StatisticsBase = @"
            SELECT
                o.id AS facility_id,
                o.name AS facility_name,
                COUNT(r.id) AS visit_count
            FROM city_objects o
            JOIN city_object_reservations r ON r.city_object_id = o.id
            WHERE r.district_id = @districtId
              AND EXTRACT(YEAR FROM r.start_time) = @year";

    public const string StatisticsMonthFilter = " AND EXTRACT(MONTH FROM r.start_time) = @month";

    public const string StatisticsGroupAndOrder = @"
            GROUP BY o.id, o.name
            ORDER BY visit_count DESC, o.name";

    public const string ReservationHistory = @"
            SELECT
                o.id AS facility_id,
                o.name AS facility_name,
                r.start_time AS reserved_at,
                '' AS note
            FROM city_object_reservations r
            JOIN city_objects o ON o.id = r.city_object_id
            WHERE r.district_id = @districtId
              AND r.city_object_id = @facilityId
            ORDER BY r.start_time DESC";

    private const string FacilitySelect = @"
            SELECT
                o.id,
                @districtId::bigint AS district_id,
                o.name,
                o.type AS description,
                CONCAT(l.city, ', ', l.country) AS address,
                COALESCE(v.vote_count, 0) AS vote_count,
                CASE WHEN my_vote.id IS NULL THEN FALSE ELSE TRUE END AS has_my_vote,
                last_res.last_reserved_at
            FROM city_objects o
            JOIN locations l ON l.id = o.location_id
            LEFT JOIN (
                SELECT city_object_id, COUNT(*) AS vote_count
                FROM city_object_votes
                GROUP BY city_object_id
            ) v ON v.city_object_id = o.id
            LEFT JOIN city_object_votes my_vote
                ON my_vote.city_object_id = o.id AND my_vote.citizen_id = @citizenId
            LEFT JOIN (
                SELECT city_object_id, MAX(start_time) AS last_reserved_at
                FROM city_object_reservations
                WHERE district_id = @districtId
                GROUP BY city_object_id
            ) last_res ON last_res.city_object_id = o.id";
}
