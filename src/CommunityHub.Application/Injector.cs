using BmRepos = CommunityHub.Application.Database.Repositories.BuildingManager;
using TenantRepos = CommunityHub.Application.Database.Repositories.Tenant;
using BmServices = CommunityHub.Application.Services.BuildingManager;
using TenantServices = CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Application;

public class Injector
{
    private static readonly Dictionary<Type, object> _implementations = new()
    {
        // BuildingManager
        { typeof(BmServices.IBuildingRepository),              new BmRepos.BuildingDbRepository() },
        { typeof(BmServices.IBuildingAccessRequestRepository), new BmRepos.BuildingAccessRequestDbRepository() },
        { typeof(BmServices.ICommonRoomRepository),            new BmRepos.CommonRoomDbRepository() },

        // Tenant
        { typeof(TenantServices.IAdvertisementRepository),             new TenantRepos.AdvertisementDbRepository() },
        { typeof(TenantServices.IBuildingMembershipRepository),        new TenantRepos.BuildingMembershipDbRepository() },
        { typeof(TenantServices.IBuildingAccessRequestRepository),     new TenantRepos.BuildingAccessRequestDbRepository() },
        { typeof(TenantServices.ITenantBuildingSearchRepository),      new BmRepos.BuildingDbRepository() },
    };

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_implementations.TryGetValue(type, out object? implementation))
            return (T)implementation;

        throw new ArgumentException($"No implementation found for type {type}");
    }
}
