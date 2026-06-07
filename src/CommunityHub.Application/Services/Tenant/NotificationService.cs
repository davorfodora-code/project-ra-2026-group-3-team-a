using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class NotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo) => _repo = repo;

    public List<NotificationDto> GetUnread(long tenantId) => _repo.GetUnreadForTenant(tenantId);

    public void MarkAsRead(long notificationId) => _repo.MarkAsRead(notificationId);
}
