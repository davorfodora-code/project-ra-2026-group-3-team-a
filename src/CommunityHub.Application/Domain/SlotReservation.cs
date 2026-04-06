namespace CommunityHub.Application.Domain;

public class SlotReservation
{
    public long Id { get; private set; }
    public AdvertisementSlot Slot { get; private set; }
    public Advertisement RequesterAd { get; private set; }
    public User Tenant { get; private set; }

    public SlotReservation(long id, AdvertisementSlot slot, Advertisement requesterAd, User tenant)
    {
        Id = id;
        Slot = slot;
        RequesterAd = requesterAd;
        Tenant = tenant;
    }

    public SlotReservation(AdvertisementSlot slot, Advertisement requesterAd, User tenant)
    {
        Id = 0;
        Slot = slot;
        RequesterAd = requesterAd;
        Tenant = tenant;
    }
}
