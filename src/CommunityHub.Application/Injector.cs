using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application;

public class Injector
{
    private static readonly Dictionary<Type, object> _implementations = new()
    {
        // BuildingManager
        { typeof(IBuildingRepository),              new BuildingDbRepository() },
        { typeof(IBuildingAccessRequestRepository), new BuildingAccessRequestDbRepository() },
        { typeof(ICommonRoomRepository),            new CommonRoomDbRepository() },

        // Citizen
        { typeof(ICitizenDistrictRepository),               new CitizenDistrictDbRepository() },
        { typeof(ICitizenDistrictAccessRequestRepository),  new CitizenDistrictAccessRequestDbRepository() },
        { typeof(ICitizenUserProfileRepository),            new CitizenUserProfileDbRepository()  },
        { typeof(ICitizenDistrictMembershipRepository),     new CitizenDistrictMembershipDbRepository() },
        { typeof(ICitizenDistrictEventRepository),          new CitizenDistrictEventDbRepository() },
        { typeof(ICitizenDistrictEventApplicationRepository), new CitizenDistrictEventApplicationDbRepository() },
            
        { typeof(ICitizenDistrictEventService),             new CitizenDistrictEventService() },
        { typeof(ICitizenAccessRequestService),             new CitizenAccessRequestService() },
        { typeof(ICitizenDistrictService),                  new CitizenDistrictService() },
        { typeof(ICitizenMembershipService),                new CitizenMembershipService() },
    };

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_implementations.TryGetValue(type, out object? implementation))
            return (T)implementation;

        throw new ArgumentException($"No implementation found for type {type}");
    }
}
