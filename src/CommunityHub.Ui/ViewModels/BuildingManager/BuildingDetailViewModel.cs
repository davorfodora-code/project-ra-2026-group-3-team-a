using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Glavni VM za stranicu detalja zgrade — orkestrira pod-VM-ove (slike, prijave problema)
// i drzi sopstveno stanje za tabove, stanare i prostorije.
public class BuildingDetailViewModel : BaseViewModel
{
    private readonly BuildingService _buildingService;
    private readonly BuildingMembershipsViewModel _membershipsVm;
    private readonly CommonRoomsViewModel         _commonRoomsVm;
    private int _activeTabIndex;
    private ObservableCollection<BuildingMembershipDto> _members = new();
    private ObservableCollection<CommonRoomDto>         _rooms   = new();

    public BuildingDetailViewModel(BuildingDto building, long managerId)
    {
        _buildingService = new BuildingService(Injector.Get<IBuildingRepository>());
        _membershipsVm   = new BuildingMembershipsViewModel(managerId);
        _commonRoomsVm   = new CommonRoomsViewModel();
        Building         = _buildingService.GetById(building.Id) ?? building;
        ImageGallery     = new BuildingImageGalleryViewModel(Building.ImagePaths);
        Issues           = new BuildingIssuesViewModel(Building.Id);
        LoadMembers();
        LoadRooms();
    }

    public BuildingDto                    Building     { get; }
    public BuildingImageGalleryViewModel  ImageGallery { get; }
    public BuildingIssuesViewModel        Issues       { get; }

    public string AddressText        => Building.AddressDisplay;
    public string LocationHeaderText => $"{Building.Neighborhood}, {Building.City}";
    public string LocationFullText   => $"{Building.City}, {Building.Country}";
    public string NumberOfFloorsText => Building.NumberOfFloors.ToString();
    public bool   HasFloors          => Building.Floors.Count > 0;

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (!SetProperty(ref _activeTabIndex, value)) return;
            OnPropertyChanged(nameof(IsInfoTabActive));
            OnPropertyChanged(nameof(IsMembersTabActive));
            OnPropertyChanged(nameof(IsRoomsTabActive));
            OnPropertyChanged(nameof(IsProblemsTabActive));
            OnPropertyChanged(nameof(IsFabVisible));
        }
    }
    public bool IsInfoTabActive     => _activeTabIndex == 0;
    public bool IsMembersTabActive  => _activeTabIndex == 1;
    public bool IsRoomsTabActive    => _activeTabIndex == 2;
    public bool IsProblemsTabActive => _activeTabIndex == 3;
    public bool IsFabVisible        => IsRoomsTabActive;

    public ObservableCollection<BuildingMembershipDto> Members
    {
        get => _members;
        private set
        {
            SetProperty(ref _members, value);
            OnPropertyChanged(nameof(HasMembers));
            OnPropertyChanged(nameof(HasNoMembers));
        }
    }
    public bool HasMembers   => _members.Count > 0;
    public bool HasNoMembers => _members.Count == 0;

    public void LoadMembers()
    {
        try
        {
            List<BuildingMembershipDto> list = _membershipsVm.GetMembershipsByBuildingId(Building.Id);
            Members = new ObservableCollection<BuildingMembershipDto>(list);
        }
        catch { Members = new ObservableCollection<BuildingMembershipDto>(); }
    }

    public ObservableCollection<CommonRoomDto> Rooms
    {
        get => _rooms;
        private set
        {
            SetProperty(ref _rooms, value);
            OnPropertyChanged(nameof(HasRooms));
            OnPropertyChanged(nameof(HasNoRooms));
        }
    }
    public bool HasRooms   => _rooms.Count > 0;
    public bool HasNoRooms => _rooms.Count == 0;

    public void LoadRooms()
    {
        try
        {
            List<CommonRoomDto> list = _commonRoomsVm.GetByBuildingId(Building.Id);
            Rooms = new ObservableCollection<CommonRoomDto>(list);
        }
        catch { Rooms = new ObservableCollection<CommonRoomDto>(); }
    }
}
