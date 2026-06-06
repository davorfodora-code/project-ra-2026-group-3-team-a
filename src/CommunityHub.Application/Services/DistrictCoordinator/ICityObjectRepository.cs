using System;
using System.Collections.Generic;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface ICityObjectRepository
{
    List<CityObject> GetAll();
    Dictionary<long, int> GetVoteCounts();
    List<(DateTime StartTime, DateTime EndTime)> GetReservations(long cityObjectId);
    void CreateReservation(long cityObjectId, long districtId, DateTime startTime, DateTime endTime);
    void ResetVotes(long cityObjectId);
}
