namespace CommunityHub.Application.Services.Tenant.Dto;

public class NotificationDto
{
    public long Id { get; init; }
    public string Type { get; init; } = "";
    public DateTime CreatedAt { get; init; }

    // Actor: booker name for reservation, new ad owner for matching
    public string ActorName { get; init; } = "";

    // Own ad info (always present)
    public string OwnAdCategory { get; init; } = "";
    public bool OwnAdIsOffering { get; init; }

    // Matching ad notification fields
    public long RelatedAdId { get; init; }
    public bool RelatedAdIsOffering { get; init; }
    public string RelatedAdDescription { get; init; } = "";
    public DateTime RelatedAdStart { get; init; }
    public DateTime RelatedAdEnd { get; init; }
    public long OwnAdId { get; init; }
    public DateTime OwnAdStart { get; init; }
    public DateTime OwnAdEnd { get; init; }

    public string TitleDisplay => Type == "Rezervacija termina"
        ? $"{ActorName} je rezervisao/la termin na Vašem oglasu \"{OwnAdTypeDisplay} – {OwnAdCategory}\""
        : $"Novi oglas za Vaš \"{OwnAdTypeDisplay} – {OwnAdCategory}\": {ActorName} {(RelatedAdIsOffering ? "nudi" : "traži")} pomoć";

    public string ActionText => Type == "Rezervacija termina"
        ? "Pregledaj zakazivanje"
        : "Pogledaj slobodne termine";

    public string TimeAgoDisplay
    {
        get
        {
            var e = DateTime.UtcNow - CreatedAt;
            if (e.TotalMinutes < 60) return $"Pre {Math.Max(1, (int)e.TotalMinutes)} min";
            if (e.TotalHours < 24) return $"Pre {(int)e.TotalHours} h";
            return $"Pre {(int)e.TotalDays} dana";
        }
    }

    private string OwnAdTypeDisplay => OwnAdIsOffering ? "Nudim pomoć" : "Tražim pomoć";
}
