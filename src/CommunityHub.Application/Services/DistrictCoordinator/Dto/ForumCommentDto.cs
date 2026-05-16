using System;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class ForumCommentDto
{
    public long Id { get; set; }
    public long AuthorId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string AuthorInitials { get; set; } = string.Empty;
    public string AuthorDistrictInfo { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public bool IsForumAuthor { get; set; }
    public bool? CurrentUserReaction { get; set; } 

    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
}
