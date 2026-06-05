using System;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class ReviewDto
{
    public int       Rating    { get; set; }
    public string?   Comment   { get; set; }
    public DateTime  CreatedAt { get; set; }

    public string DateDisplay  => CreatedAt.ToString("d.M.yyyy");
    public string StarsDisplay => new string('★', Rating) + new string('☆', 5 - Rating);
    public string CommentDisplay => string.IsNullOrWhiteSpace(Comment) ? "—" : Comment;
}
