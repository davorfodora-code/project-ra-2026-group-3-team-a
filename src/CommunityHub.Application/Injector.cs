using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application;

public class Injector
{
    private static readonly Dictionary<Type, object> _implementations = new()
    {
        // BuildingManager
        { typeof(IBuildingRepository),              new BuildingDbRepository() },
        { typeof(IBuildingAccessRequestRepository), new BuildingAccessRequestDbRepository() },
        { typeof(ICommonRoomRepository),            new CommonRoomDbRepository() },

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
