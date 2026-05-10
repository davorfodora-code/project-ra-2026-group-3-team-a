using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.BuildingManager;

namespace CommunityHub.Application.Services.BuildingManager;

public class CommonRoomService
{
    private readonly CommonRoomDbRepository _repository;
    private readonly BuildingDbRepository _buildingRepository;

    public CommonRoomService()
    {
        _repository = new CommonRoomDbRepository();
        _buildingRepository = new BuildingDbRepository();
    }

    public List<CommonRoomDto> GetByBuildingId(long buildingId)
    {
        return _repository.GetByBuildingId(buildingId)
            .Select(MapToDto)
            .ToList();
    }

    public CommonRoomDto Add(CreateCommonRoomDto dto)
    {
        Building? building = _buildingRepository.GetById(dto.BuildingId);
        if (building == null)
            throw new InvalidOperationException("Zgrada nije pronadjena.");

        CommonRoom room = new CommonRoom(building, dto.Name, dto.Description, dto.FloorNumber, dto.IsMultiDay);
        CommonRoom created = _repository.Add(room);
        return MapToDto(created);
    }

    private CommonRoomDto MapToDto(CommonRoom r) => new CommonRoomDto
    {
        Id = r.Id,
        BuildingId = r.Building.Id,
        Name = r.Name,
        Description = r.Description,
        FloorNumber = r.FloorNumber,
        IsMultiDay = r.IsMultiDay
    };
}
