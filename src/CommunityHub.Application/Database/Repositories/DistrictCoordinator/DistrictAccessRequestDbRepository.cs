using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictAccessRequestDbRepository : IDistrictAccessRequestRepository
{
    private const string GetByCoordinatorQuery = @"
        SELECT r.id, r.creation_date, r.status, r.rejection_reason,
               u.id AS user_id, u.username, u.password, u.name, u.surname, u.birthday, u.role, u.address,
               d.id AS district_id, d.name AS district_name, d.description, d.budget,
               l.id AS location_id, l.city, l.country
        FROM district_access_requests r
        JOIN users u ON u.id = r.user_id
        JOIN districts d ON d.id = r.district_id
        JOIN locations l ON l.id = d.location_id
        WHERE d.coordinator_id = @coordinatorId
        ORDER BY r.creation_date DESC";

    public long Create(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO district_access_requests (user_id, district_id, creation_date, status)
            VALUES (@userId, @districtId, @creationDate, @status)
            RETURNING id";

        AddParameter(command, "@userId",       userId);
        AddParameter(command, "@districtId",   districtId);
        AddParameter(command, "@creationDate", DateTime.Now);
        AddParameter(command, "@status",       DistrictAccessRequestStatus.Pending.ToString());

        return Convert.ToInt64(command.ExecuteScalar());
    }

    public bool ExistsPending(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 1
            FROM district_access_requests
            WHERE user_id = @userId
              AND district_id = @districtId
              AND status = @status
            LIMIT 1";

        AddParameter(command, "@userId",     userId);
        AddParameter(command, "@districtId", districtId);
        AddParameter(command, "@status",     DistrictAccessRequestStatus.Pending.ToString());

        return command.ExecuteScalar() != null;
    }

    public List<DistrictAccessRequest> GetByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = GetByCoordinatorQuery;
        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<DistrictAccessRequest> requests = new List<DistrictAccessRequest>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            requests.Add(DistrictAccessRequestDbMapper.MapAccessRequest(reader, coordinatorId));
        return requests;
    }

    public void Approve(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();
        (long userId, long districtId) = DistrictApprovalHelper.GetRequestUserAndDistrict(connection, requestId);
        DistrictApprovalHelper.UpdateRequestStatus(connection, requestId, "Approved");
        DistrictApprovalHelper.InsertMembership(connection, districtId, userId);
    }

    public void Reject(long requestId, string? reason)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_access_requests
            SET status = 'Rejected',
                rejection_reason = @reason
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@reason", (object?)reason ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public List<DistrictMembership> GetMembershipsByDistrictId(long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT m.id, m.join_date, m.trust_level,
                   u.id AS user_id, u.username, u.password, u.name, u.surname, u.birthday, u.role, u.address
            FROM district_memberships m
            JOIN users u ON u.id = m.user_id
            WHERE m.district_id = @districtId
            ORDER BY m.join_date DESC";

        AddParameter(cmd, "@districtId", districtId);

        List<DistrictMembership> memberships = new List<DistrictMembership>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            memberships.Add(DistrictAccessRequestDbMapper.MapMembership(reader));
        return memberships;
    }

    public List<District> GetDistrictsByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT d.id, d.name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country
            FROM districts d
            JOIN locations l ON l.id = d.location_id
            WHERE d.coordinator_id = @coordinatorId
            ORDER BY d.name";

        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<District> districts = new List<District>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            districts.Add(DistrictAccessRequestDbMapper.MapDistrictBasic(reader, coordinatorId));
        return districts;
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
