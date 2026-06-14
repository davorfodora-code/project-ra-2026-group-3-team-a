using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface ISchedulingRepository
{
    List<MyBookingGroupDto>      GetMyBookings(long tenantId);
    List<BookedOnMyPostDto>      GetBookedOnMyPosts(long tenantId);
    List<ReportAppointmentDto>   GetReportAppointments(long tenantId, DateTime from, DateTime to);
}
