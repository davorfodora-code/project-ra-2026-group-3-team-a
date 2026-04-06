namespace CommunityHub.Application.Domain;

public class AdvertisementSlot
{
    public long Id { get; private set; }
    public Advertisement Advertisement { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsAvailable { get; private set; }

    public AdvertisementSlot(long id, Advertisement advertisement, DateTime startTime, DateTime endTime, bool isAvailable)
    {
        Id = id;
        Advertisement = advertisement;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = isAvailable;
    }

    public AdvertisementSlot(Advertisement advertisement, DateTime startTime, DateTime endTime, bool isAvailable)
    {
        Id = 0;
        Advertisement = advertisement;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = isAvailable;
    }
}
