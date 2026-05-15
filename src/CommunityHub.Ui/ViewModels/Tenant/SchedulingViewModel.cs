using System.Globalization;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class DayScheduleStat
{
    public DateTime Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public int OfferingCount { get; set; }
    public int SeekingCount { get; set; }
}

public class SchedulingViewModel : BaseViewModel
{
    private readonly SchedulingService _service;
    public long TenantId { get; }

    public List<MyBookingGroupDto> MyBookings { get; private set; } = [];
    public List<BookedOnMyPostDto> BookedOnMyPosts { get; private set; } = [];
    public DayScheduleStat[] WeekStats { get; private set; } = [];
    public string WeekLabel { get; private set; } = string.Empty;

    public SchedulingViewModel(long tenantId)
    {
        TenantId = tenantId;
        _service = new SchedulingService(Injector.Get<ISchedulingRepository>());
    }

    public void Load()
    {
        MyBookings     = _service.GetMyBookings(TenantId);
        BookedOnMyPosts = _service.GetBookedOnMyPosts(TenantId);
        ComputeWeekStats();
    }

    private void ComputeWeekStats()
    {
        DateTime today = DateTime.Today;
        // Find next Monday (0 = show current week if today is Monday)
        int daysToMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        DateTime monday = today.AddDays(daysToMonday == 0 ? 7 : daysToMonday);

        WeekLabel = $"{monday.ToString("MMM d", CultureInfo.InvariantCulture)} – " +
                    $"{monday.AddDays(6).ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";

        // Build a flat lookup: date → list of (slot, isOwnerOffering)
        var slotsByDay = MyBookings
            .SelectMany(g => g.Slots.Select(s => (Slot: s, g.IsOwnerOffering)))
            .Where(x => x.Slot.StartTime.Date >= monday && x.Slot.StartTime.Date < monday.AddDays(7))
            .GroupBy(x => x.Slot.StartTime.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        WeekStats = new DayScheduleStat[7];
        for (int i = 0; i < 7; i++)
        {
            DateTime day = monday.AddDays(i);
            var slotsOnDay = slotsByDay.TryGetValue(day, out var list) ? list : [];

            WeekStats[i] = new DayScheduleStat
            {
                Date          = day,
                DayLabel      = day.ToString("ddd", CultureInfo.InvariantCulture),
                // I am Offering help when the owner is Seeking (is_offering=false)
                OfferingCount = slotsOnDay.Count(x => !x.IsOwnerOffering),
                // I am Seeking help when the owner is Offering (is_offering=true)
                SeekingCount  = slotsOnDay.Count(x => x.IsOwnerOffering),
            };
        }
    }
}
