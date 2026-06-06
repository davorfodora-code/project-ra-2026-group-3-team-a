using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class CityObjectDbRepository : ICityObjectRepository
{
    public List<CityObject> GetAll()
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT co.id, co.name, co.type, co.capacity,
                   l.id AS location_id, l.city, l.country
            FROM city_objects co
            JOIN locations l ON l.id = co.location_id
            ORDER BY co.name";

        var list = new List<CityObject>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
        {
            var location = new Location(
                r.GetInt64(r.GetOrdinal("location_id")),
                r.GetString(r.GetOrdinal("city")),
                r.GetString(r.GetOrdinal("country")));

            list.Add(new CityObject(
                r.GetInt64(r.GetOrdinal("id")),
                r.GetString(r.GetOrdinal("name")),
                r.GetString(r.GetOrdinal("type")),
                location,
                r.GetInt32(r.GetOrdinal("capacity"))));
        }
        return list;
    }

    public Dictionary<long, int> GetVoteCounts()
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT city_object_id, COUNT(*) AS vote_count
            FROM city_object_votes
            GROUP BY city_object_id";

        var dict = new Dictionary<long, int>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            dict[Convert.ToInt64(r["city_object_id"])] = Convert.ToInt32(r["vote_count"]);
        return dict;
    }

    public List<(DateTime StartTime, DateTime EndTime)> GetReservations(long cityObjectId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT start_time, end_time
            FROM city_object_reservations
            WHERE city_object_id = @cityObjectId";
        AddParam(cmd, "@cityObjectId", cityObjectId);

        var list = new List<(DateTime, DateTime)>();
        using IDataReader r = cmd.ExecuteReader();
        while (r.Read())
            list.Add((r.GetDateTime(0), r.GetDateTime(1)));
        return list;
    }

    public void CreateReservation(long cityObjectId, long districtId, DateTime startTime, DateTime endTime)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO city_object_reservations
                (city_object_id, district_id, start_time, end_time, reserved_at)
            VALUES
                (@cityObjectId, @districtId, @startTime, @endTime, @reservedAt)";
        AddParam(cmd, "@cityObjectId", cityObjectId);
        AddParam(cmd, "@districtId",   districtId);
        AddParam(cmd, "@startTime",    startTime);
        AddParam(cmd, "@endTime",      endTime);
        AddParam(cmd, "@reservedAt",   DateTime.Now);
        cmd.ExecuteNonQuery();
    }

    public void ResetVotes(long cityObjectId)
    {
        using IDbConnection conn = PostgresConnection.CreateConnection();
        IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM city_object_votes WHERE city_object_id = @cityObjectId";
        AddParam(cmd, "@cityObjectId", cityObjectId);
        cmd.ExecuteNonQuery();
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value;
        cmd.Parameters.Add(p);
    }
}
