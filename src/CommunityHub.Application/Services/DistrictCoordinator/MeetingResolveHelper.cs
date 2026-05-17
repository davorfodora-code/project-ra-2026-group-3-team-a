using System;
using System.Collections.Generic;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public static class MeetingResolveHelper
{
    public static void HandleResult(
        MeetingDto meeting,
        ResolveResult result,
        DistrictMeetingService service,
        Action<string, string, bool> notify,
        Action<MeetingDto, List<DateTime>, Action<DateTime?>> resolveTie)
    {
        if (!result.HasVotes)
        {
            notify(
                $"Sastanak '{meeting.MeetingTypeDisplay}' za kvart '{meeting.DistrictName}' " +
                "je automatski otkazan jer nema glasova.",
                "Sastanak otkazan", true);
            return;
        }

        if (result.IsTie)
        {
            notify(
                $"Glasanje za sastanak '{meeting.MeetingTypeDisplay}' (kvart: '{meeting.DistrictName}') " +
                "je izjednačeno. Molimo odaberite jedan od predloženih datuma.",
                "Izjednačen broj glasova", true);
            resolveTie(meeting, result.TiedDates, chosen =>
            {
                if (chosen.HasValue) service.ConfirmMeetingDate(meeting.Id, chosen.Value);
            });
            return;
        }

        notify(
            $"Sastanak '{meeting.MeetingTypeDisplay}' za kvart '{meeting.DistrictName}' " +
            $"je zakazan za {result.WinnerDate:dd.MM.yyyy.} u {meeting.MeetingTimeDisplay}.",
            "Sastanak zakazan", true);
    }
}
