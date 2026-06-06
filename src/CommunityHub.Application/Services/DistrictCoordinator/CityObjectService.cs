using System;
using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class CityObjectService
{
    private const int DayStartHour = 10;
    private const int DayEndHour   = 18;
    private const int MaxSlotsToReturn   = 6;
    private const int FallbackSearchDays = 60;

    private readonly ICityObjectRepository _repo;

    public CityObjectService(ICityObjectRepository repo) => _repo = repo;

    public List<CityObjectDto> GetAllWithVotes()
    {
        var objects    = _repo.GetAll();
        var voteCounts = _repo.GetVoteCounts();

        return objects.Select(o => new CityObjectDto
        {
            Id        = o.Id,
            Name      = o.Name,
            Type      = o.Type,
            City      = o.Location.City,
            Capacity  = o.Capacity,
            VoteCount = voteCounts.TryGetValue(o.Id, out int count) ? count : 0
        }).ToList();
    }

    public List<CityObjectSlotDto> FindAvailableSlots(
        long cityObjectId, int durationHours, DateTime rangeStart, DateTime rangeEnd)
    {
        var reservations = _repo.GetReservations(cityObjectId);

        var slotsInRange = GenerateSlots(rangeStart.Date, rangeEnd.Date, durationHours, reservations);
        if (slotsInRange.Count > 0)
            return slotsInRange;

        var fallbackEnd  = rangeEnd.Date.AddDays(FallbackSearchDays);
        return GenerateSlots(rangeEnd.Date.AddDays(1), fallbackEnd, durationHours, reservations);
    }

    public void Reserve(long cityObjectId, long districtId, DateTime startTime, DateTime endTime)
    {
        _repo.CreateReservation(cityObjectId, districtId, startTime, endTime);
        _repo.ResetVotes(cityObjectId);
    }

    private static List<CityObjectSlotDto> GenerateSlots(
        DateTime from, DateTime to, int durationHours,
        List<(DateTime StartTime, DateTime EndTime)> reservations)
    {
        var slots = new List<CityObjectSlotDto>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            for (int hour = DayStartHour; hour + durationHours <= DayEndHour; hour += durationHours)
            {
                var slotStart = date.AddHours(hour);
                var slotEnd   = slotStart.AddHours(durationHours);

                bool overlaps = reservations.Any(r => r.StartTime < slotEnd && r.EndTime > slotStart);
                if (!overlaps)
                    slots.Add(new CityObjectSlotDto { StartTime = slotStart, EndTime = slotEnd });

                if (slots.Count >= MaxSlotsToReturn) return slots;
            }
        }

        return slots;
    }
}
