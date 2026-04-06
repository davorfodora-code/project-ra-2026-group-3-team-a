namespace CommunityHub.Application.Domain;

public class Notification
{
    public long Id { get; private set; }
    public User Tenant { get; private set; }
    public string Type { get; private set; }
    public Advertisement? RelatedAd { get; private set; }
    public SlotReservation? RelatedSlot { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Notification(long id, User tenant, string type, Advertisement? relatedAd, SlotReservation? relatedSlot, bool isRead, DateTime createdAt)
    {
        Id = id;
        Tenant = tenant;
        Type = type;
        RelatedAd = relatedAd;
        RelatedSlot = relatedSlot;
        IsRead = isRead;
        CreatedAt = createdAt;
    }

    public Notification(User tenant, string type, Advertisement? relatedAd, SlotReservation? relatedSlot, bool isRead, DateTime createdAt)
    {
        Id = 0;
        Tenant = tenant;
        Type = type;
        RelatedAd = relatedAd;
        RelatedSlot = relatedSlot;
        IsRead = isRead;
        CreatedAt = createdAt;
    }
}
