using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class SchedulingService
{
    private readonly ISchedulingRepository _repository;

    public SchedulingService(ISchedulingRepository repository)
    {
        _repository = repository;
    }

    public List<MyBookingGroupDto> GetMyBookings(long tenantId)
        => _repository.GetMyBookings(tenantId);

    public List<BookedOnMyPostDto> GetBookedOnMyPosts(long tenantId)
        => _repository.GetBookedOnMyPosts(tenantId);

    public List<ReportAppointmentDto> GetReportAppointments(long tenantId, DateTime from, DateTime to)
        => _repository.GetReportAppointments(tenantId, from, to);
}
