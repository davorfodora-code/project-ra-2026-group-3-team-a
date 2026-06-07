namespace CommunityHub.Application.Database.Repositories.Citizen;

internal static class CitizenCityFacilitySql
{
    public const string FacilitiesByDistrict = FacilitySelect + @"
            WHERE f.district_id = @districtId
            ORDER BY vote_count DESC, f.name";

    public const string FacilityById = FacilitySelect + @"
            WHERE f.id = @facilityId";

    public const string Vote = @"
            INSERT INTO city_facility_interest_votes (facility_id, citizen_id, voted_at)
            VALUES (@facilityId, @citizenId, @votedAt)
            ON CONFLICT (facility_id, citizen_id) DO NOTHING";

    public const string HasVote = @"
            SELECT 1
            FROM city_facility_interest_votes
            WHERE facility_id = @facilityId AND citizen_id = @citizenId
            LIMIT 1";

    public const string FacilityBelongsToDistrict = @"
            SELECT 1
            FROM city_facilities
            WHERE id = @facilityId AND district_id = @districtId
            LIMIT 1";

    public const string StatisticsBase = @"
            SELECT
                f.id AS facility_id,
                f.name AS facility_name,
                COUNT(r.id) AS visit_count
            FROM city_facilities f
            JOIN city_facility_reservations r ON r.facility_id = f.id
            WHERE r.district_id = @districtId
              AND EXTRACT(YEAR FROM r.reserved_at) = @year";

    public const string StatisticsMonthFilter = " AND EXTRACT(MONTH FROM r.reserved_at) = @month";

    public const string StatisticsGroupAndOrder = @"
            GROUP BY f.id, f.name
            ORDER BY visit_count DESC, f.name";

    public const string ReservationHistory = @"
            SELECT
                f.id AS facility_id,
                f.name AS facility_name,
                r.reserved_at,
                COALESCE(r.note, '') AS note
            FROM city_facility_reservations r
            JOIN city_facilities f ON f.id = r.facility_id
            WHERE r.district_id = @districtId
              AND r.facility_id = @facilityId
            ORDER BY r.reserved_at DESC";

    private const string FacilitySelect = @"
            SELECT
                f.id,
                f.district_id,
                f.name,
                f.description,
                f.address,
                COALESCE(v.vote_count, 0) AS vote_count,
                CASE WHEN my_vote.id IS NULL THEN FALSE ELSE TRUE END AS has_my_vote,
                last_res.last_reserved_at
            FROM city_facilities f
            LEFT JOIN (
                SELECT facility_id, COUNT(*) AS vote_count
                FROM city_facility_interest_votes
                GROUP BY facility_id
            ) v ON v.facility_id = f.id
            LEFT JOIN city_facility_interest_votes my_vote
                ON my_vote.facility_id = f.id AND my_vote.citizen_id = @citizenId
            LEFT JOIN (
                SELECT facility_id, MAX(reserved_at) AS last_reserved_at
                FROM city_facility_reservations
                GROUP BY facility_id
            ) last_res ON last_res.facility_id = f.id";
}
