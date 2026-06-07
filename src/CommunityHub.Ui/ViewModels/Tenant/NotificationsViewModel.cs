using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class NotificationsViewModel : BaseViewModel
{
    private readonly NotificationService _service;
    public long TenantId { get; }
    public ObservableCollection<NotificationDto> Notifications { get; } = new();
    public int UnreadCount => Notifications.Count;

    public NotificationsViewModel(long tenantId)
    {
        TenantId = tenantId;
        _service = new NotificationService(Injector.Get<INotificationRepository>());
    }

    public void Load()
    {
        Notifications.Clear();
        foreach (var n in _service.GetUnread(TenantId))
            Notifications.Add(n);
        OnPropertyChanged(nameof(UnreadCount));
    }

    public void MarkAsRead(long notificationId) => _service.MarkAsRead(notificationId);
}
