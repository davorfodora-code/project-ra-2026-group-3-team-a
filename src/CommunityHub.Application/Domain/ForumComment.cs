using System;

namespace CommunityHub.Application.Domain;

public class ForumComment
{
    public long Id { get; private set; }
    public long ForumId { get; private set; }
    public User Author { get; private set; }
    public string AuthorDistrictName { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }

    public ForumComment(long forumId, User author, string content)
    {
        ForumId = forumId;
        Author = author;
        Content = content;
        CreatedAt = DateTime.Now;
        AuthorDistrictName = string.Empty;
    }

    public ForumComment(long id, long forumId, User author, string authorDistrictName,
        string content, DateTime createdAt, int likeCount, int dislikeCount)
    {
        Id = id;
        ForumId = forumId;
        Author = author;
        AuthorDistrictName = authorDistrictName;
        Content = content;
        CreatedAt = createdAt;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
    }
}
