using System;
using System.Collections.Generic;
using System.Globalization;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class CityObjectSlotDto
{
    private static readonly Dictionary<DayOfWeek, string> _dayNames = new()
    {
        { DayOfWeek.Monday,    "ponedeljak" },
        { DayOfWeek.Tuesday,   "utorak"     },
        { DayOfWeek.Wednesday, "sreda"      },
        { DayOfWeek.Thursday,  "četvrtak"   },
        { DayOfWeek.Friday,    "petak"      },
        { DayOfWeek.Saturday,  "subota"     },
        { DayOfWeek.Sunday,    "nedelja"    },
    };

    public DateTime StartTime { get; set; }
    public DateTime EndTime   { get; set; }

    public string DateDisplay => StartTime.ToString("d.M.yyyy", CultureInfo.InvariantCulture);
    public string DayDisplay  => _dayNames[StartTime.DayOfWeek];
    public string TimeDisplay => $"{StartTime:HH:mm}-{EndTime:HH:mm}h";
    public string FullDisplay => $"{DateDisplay}, {DayDisplay}, {TimeDisplay}";
}
