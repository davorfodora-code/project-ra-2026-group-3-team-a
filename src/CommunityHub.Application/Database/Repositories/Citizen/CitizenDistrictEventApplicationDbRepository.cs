using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictEventApplicationDbRepository : ICitizenDistrictEventApplicationRepository
{
    private readonly CitizenDistrictEventApplicationQueryDbRepository _queryRepository = new();
    private readonly CitizenDistrictEventApplicationCommandDbRepository _commandRepository = new();
    private readonly CitizenDistrictEventAttendanceCommandDbRepository _attendanceRepository = new();

    public bool ExistsByEventAndCitizen(long eventId, long citizenId) => _queryRepository.ExistsByEventAndCitizen(eventId, citizenId);

    public long CreateApplication(long eventId, long citizenId, DateTime appliedAt, IEnumerable<long>? selectedItemIds)
        => _commandRepository.CreateApplication(eventId, citizenId, appliedAt, selectedItemIds);

    public void AddItemsToExistingApplication(long eventId, long citizenId, IEnumerable<long>? selectedItemIds)
        => _commandRepository.AddItemsToExistingApplication(eventId, citizenId, selectedItemIds);

    public void UpsertAttendance(long eventId, long volunteerId, bool attended, DateTime markedAt)
        => _attendanceRepository.UpsertAttendance(eventId, volunteerId, attended, markedAt);
}
