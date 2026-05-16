using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Database.Repositories.Citizen;
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

        // Citizen
        { typeof(ICitizenDistrictRepository),               new CitizenDistrictDbRepository() },
        { typeof(ICitizenDistrictAccessRequestRepository),  new CitizenDistrictAccessRequestDbRepository() },
        { typeof(ICitizenUserProfileRepository),            new CitizenUserProfileDbRepository()  },
        { typeof(ICitizenDistrictMembershipRepository),     new CitizenDistrictMembershipDbRepository() },
        { typeof(ICitizenDistrictEventRepository),          new CitizenDistrictEventDbRepository() },
        { typeof(ICitizenDistrictEventApplicationRepository), new CitizenDistrictEventApplicationDbRepository() },

        // Tenant
        { typeof(TenantSvc.ITenantBuildingSearchRepository),  new BuildingDbRepository() },
        { typeof(TenantSvc.IBuildingAccessRequestRepository), new TenantDb.BuildingAccessRequestDbRepository() },
        { typeof(TenantSvc.IBuildingMembershipRepository),    new TenantDb.BuildingMembershipDbRepository() },
        { typeof(TenantSvc.IAdvertisementRepository),                new TenantDb.AdvertisementDbRepository() },
        { typeof(TenantSvc.ISchedulingRepository),                new TenantDb.SchedulingDbRepository() },
        { typeof(TenantSvc.ICommonRoomTenantRepository),          new TenantDb.CommonRoomTenantDbRepository() },
        { typeof(TenantSvc.ICommonRoomRequestTenantRepository),   new TenantDb.CommonRoomRequestTenantDbRepository() },

        // DistrictCoordinator
        { typeof(IDistrictRepository),                  new DistrictDbRepository() },
        { typeof(IDistrictAccessRequestRepository),     new DistrictAccessRequestDbRepository() },
        { typeof(IDistrictMeetingRepository),            new DistrictMeetingDbRepository() },
        { typeof(ICitizenTrustStatisticRepository),      new CitizenTrustStatisticDbRepository() },
    };

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_implementations.TryGetValue(type, out object? implementation))
            return (T)implementation;

        throw new ArgumentException($"No implementation found for type {type}");
    }
}
