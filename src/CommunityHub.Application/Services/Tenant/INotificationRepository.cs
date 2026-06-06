using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface INotificationRepository
{
    List<NotificationDto> GetUnreadForTenant(long tenantId);
    void MarkAsRead(long notificationId);
}
