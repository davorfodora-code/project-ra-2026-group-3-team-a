using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public class AssemblyService
{
    private readonly IAssemblyRepository  _repository;
    private readonly IBuildingRepository  _buildingRepository;

    public AssemblyService(IAssemblyRepository repository, IBuildingRepository buildingRepository)
    {
        _repository         = repository;
        _buildingRepository = buildingRepository;
    }

    public List<AssemblySummaryDto> GetByManagerId(long managerId)
    {
        return _repository.GetByManagerId(managerId)
            .Select(MapToSummary)
            .ToList();
    }

    public AssemblyDetailDto? GetDetail(long assemblyId)
    {
        TenantAssembly? assembly = _repository.GetById(assemblyId);
        if (assembly == null) return null;

        var attendances = _repository.GetAttendanceSummary(assemblyId, assembly.Building.Id);

        return new AssemblyDetailDto
        {
            Id              = assembly.Id,
            BuildingId      = assembly.Building.Id,
            BuildingAddress = AddressOf(assembly.Building),
            ScheduledAt     = assembly.ScheduledAt,
            Status          = assembly.Status,
            Topics          = assembly.Topics.Select(MapToTopicDto).ToList(),
            Attendances     = attendances.Select(a => new AttendanceItemDto
                              { ApartmentNumber = a.ApartmentNumber, IsConfirmed = a.IsConfirmed }).ToList()
        };
    }

    public AssemblySummaryDto Create(CreateAssemblyDto dto)
    {
        if (dto.ScheduledAt <= DateTime.Now)
            throw new InvalidOperationException("Datum i vreme skupštine moraju biti u budućnosti.");

        Building? building = _buildingRepository.GetById(dto.BuildingId);
        if (building == null)
            throw new InvalidOperationException("Zgrada nije pronađena.");
        if (_repository.HasTimeConflict(building.ManagerId, dto.ScheduledAt))
            throw new InvalidOperationException("Već postoji zakazana ili potvrđena skupština u tom terminu.");

        TenantAssembly assembly = new TenantAssembly(building, dto.ScheduledAt, "Scheduled");
        foreach (string text in dto.Topics)
            assembly.AddTopic(new AssemblyTopic(text, false));

        TenantAssembly created = _repository.Create(assembly);
        return MapToSummary(created);
    }

    private AssemblySummaryDto MapToSummary(TenantAssembly a) => new AssemblySummaryDto
    {
        Id              = a.Id,
        BuildingId      = a.Building.Id,
        BuildingAddress = AddressOf(a.Building),
        ScheduledAt     = a.ScheduledAt,
        Status          = a.Status,
        TopicCount      = a.Topics.Count
    };

    private static AssemblyTopicDto MapToTopicDto(AssemblyTopic t) => new AssemblyTopicDto
    {
        Id           = t.Id,
        Text         = t.Text,
        IsAdditional = t.IsAdditional
    };

    private static string AddressOf(Building b) =>
        $"{b.Address.Street} {b.Address.StreetNumber}";
}
