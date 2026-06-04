using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Services;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.Citizen;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;
using TenantDb  = CommunityHub.Application.Database.Repositories.Tenant;
using TenantSvc = CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application;

public class Injector
{
    private static readonly Dictionary<Type, object> _implementations = new()
    {
        // BuildingManager
        { typeof(IBuildingRepository),              new BuildingDbRepository() },
        { typeof(IBuildingAccessRequestRepository), new BuildingAccessRequestDbRepository() },
        { typeof(ICommonRoomRepository),            new CommonRoomDbRepository() },
        { typeof(ICommonRoomRequestRepository),     new CommonRoomRequestDbRepository() },
        { typeof(IBulletinBoardStatsRepository),    new BulletinBoardStatsDbRepository() },
        { typeof(IAssemblyRepository),              new AssemblyDbRepository() },

        // Citizen
        { typeof(ICitizenDistrictRepository),                 new CitizenDistrictDbRepository() },
        { typeof(ICitizenDistrictAccessRequestRepository),    new CitizenDistrictAccessRequestDbRepository() },
        { typeof(ICitizenUserProfileRepository),              new CitizenUserProfileDbRepository() },
        { typeof(ICitizenDistrictMembershipRepository),       new CitizenDistrictMembershipDbRepository() },
        { typeof(ICitizenDistrictEventRepository),            new CitizenDistrictEventDbRepository() },
        { typeof(ICitizenDistrictEventApplicationRepository), new CitizenDistrictEventApplicationDbRepository() },
        { typeof(ICitizenTrustRecordRepository),              new CitizenTrustRecordDbRepository() },
        { typeof(ICitizenTrustRecordService),                 new CitizenTrustRecordService(
                                                                new CitizenTrustRecordDbRepository(),
                                                                new CitizenDistrictMembershipDbRepository()) },
        { typeof(ICitizenMeetingRepository),                  new CitizenMeetingDbRepository() },
        { typeof(ICitizenMeetingService),                     new CitizenMeetingService(
                                                                new CitizenMeetingDbRepository(),
                                                                new CitizenDistrictMembershipDbRepository()) },
        { typeof(ICitizenCityFacilityRepository),            new CitizenCityFacilityDbRepository()  },
        { typeof(ICitizenCityFacilityService),               new CitizenCityFacilityService(
                                                                new CitizenCityFacilityDbRepository(),
                                                                new CitizenDistrictMembershipDbRepository()) },

        // Tenant
        { typeof(TenantSvc.ITenantBuildingSearchRepository),         new BuildingDbRepository() },
        { typeof(TenantSvc.IBuildingAccessRequestRepository),        new TenantDb.BuildingAccessRequestDbRepository() },
        { typeof(TenantSvc.IBuildingMembershipRepository),           new TenantDb.BuildingMembershipDbRepository() },
        { typeof(TenantSvc.IAdvertisementRepository),                new TenantDb.AdvertisementDbRepository() },
        { typeof(TenantSvc.ISchedulingRepository),                   new TenantDb.SchedulingDbRepository() },
        { typeof(TenantSvc.ICommonRoomTenantRepository),             new TenantDb.CommonRoomTenantDbRepository() },
        { typeof(TenantSvc.ICommonRoomRequestTenantRepository),      new TenantDb.CommonRoomRequestTenantDbRepository() },

        // User
        { typeof(IUserRepository),                      new UserDbRepository() },

        // DistrictCoordinator
        { typeof(IDistrictRepository),                  new DistrictDbRepository() },
        { typeof(IDistrictAccessRequestRepository),     new DistrictAccessRequestDbRepository() },
        { typeof(IDistrictMeetingRepository),            new DistrictMeetingDbRepository() },
        { typeof(ICitizenTrustStatisticRepository),      new CitizenTrustStatisticDbRepository() },
        { typeof(IForumRepository), new ForumDbRepository() },
        { typeof(ICityObjectRepository), new CityObjectDbRepository() },
        { typeof(IDistrictMeetingRepository),           new DistrictMeetingDbRepository() },
        { typeof(ICitizenTrustStatisticRepository),     new CitizenTrustStatisticDbRepository() },
        { typeof(IForumRepository),                     new ForumDbRepository() },
    };

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_implementations.TryGetValue(type, out object? implementation))
            return (T)implementation;

        throw new ArgumentException($"No implementation found for type {type}");
    }
}
