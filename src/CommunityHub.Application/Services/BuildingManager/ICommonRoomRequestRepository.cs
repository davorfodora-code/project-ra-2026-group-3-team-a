using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.BuildingManager;

public interface ICommonRoomRequestRepository
{
    // Svi zahtevi za iznajmljivanje prostorija u zgradama kojima upravlja dati upravnik
    List<CommonRoomRequest> GetByManagerId(long managerId);

    // Postavlja predloženi termin i menja status na ProposedCounter
    void ProposeDate(long requestId, DateTime proposedStart, DateTime proposedEnd);

    // Vraća zauzete periode za datu prostoriju (Approved i ProposedCounter zahtevi)
    List<(DateTime Start, DateTime End)> GetBookedRangesForRoom(long roomId);
}
