using System;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class ForumDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string AuthorDistrictInfo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsClosed { get; set; }
    public int CommentCount { get; set; }
    public bool IsOwner { get; set; }

    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");

    public string ContentPreview =>
        Content.Length > 80 ? Content[..80] + "....." : Content;
}

public class CreateForumDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long CoordinatorId { get; set; }
}
