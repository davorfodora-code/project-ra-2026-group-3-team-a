using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Application;

public class Injector
{
    private static readonly Dictionary<Type, Type> _bindings = new()
    {
        // BuildingManager
        { typeof(IBuildingRepository),              typeof(BuildingDbRepository) },
        { typeof(IBuildingAccessRequestRepository), typeof(BuildingAccessRequestDbRepository) },
        { typeof(ICommonRoomRepository),            typeof(CommonRoomDbRepository) },
    };

    public static T Get<T>()
    {
        Type interfaceType = typeof(T);
        if (!_bindings.TryGetValue(interfaceType, out Type? implType))
            throw new InvalidOperationException($"Nije registrovan binding za {interfaceType.Name}.");

        return (T)Activator.CreateInstance(implType)!;
    }
}
