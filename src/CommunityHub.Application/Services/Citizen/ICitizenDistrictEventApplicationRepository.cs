namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDistrictEventApplicationRepository
{
    bool ExistsByEventAndCitizen(long eventId, long citizenId);
    long CreateApplication(long eventId, long citizenId, DateTime appliedAt, IEnumerable<long>? selectedItemIds);
    void AddItemsToExistingApplication(long eventId, long citizenId, IEnumerable<long>? selectedItemIds);
    void UpsertAttendance(long eventId, long volunteerId, bool attended, DateTime markedAt);
}
