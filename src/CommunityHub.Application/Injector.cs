using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application;

public class Injector
{
    private static readonly Dictionary<Type, object> _implementations = new()
    {
        // BuildingManager
        { typeof(IBuildingRepository),              new BuildingDbRepository() },
        { typeof(IBuildingAccessRequestRepository), new BuildingAccessRequestDbRepository() },
        { typeof(ICommonRoomRepository),            new CommonRoomDbRepository() },
    };

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_implementations.TryGetValue(type, out object? implementation))
            return (T)implementation;

        throw new ArgumentException($"No implementation found for type {type}");
    }
}
