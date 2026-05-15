using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public interface IBulletinBoardStatsRepository
{
    // Zgrade kojima upravlja ovaj menadžer (za dropdown selector)
    List<(long BuildingId, string Address)> GetManagedBuildings(long managerId);

    // Svi oglasi za zgradu (aktivni + arhivirani), bez termina
    List<BulletinBoardAdDto> GetAllAds(long buildingId);

    // Statistika za izabrani period + trenutno stanje + top pomagač
    BulletinBoardStatsDto GetStats(long buildingId, int year, int? month);
}
