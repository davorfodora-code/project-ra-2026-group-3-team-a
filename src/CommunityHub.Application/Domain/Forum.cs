using System;

namespace CommunityHub.Application.Domain;

public class Forum
{
    public long Id { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public User Author { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsClosed { get; private set; }
    public int CommentCount { get; set; }

    public Forum(string title, string content, User author)
    {
        Title = title;
        Content = content;
        Author = author;
        CreatedAt = DateTime.Now;
        IsClosed = false;
    }

    public Forum(long id, string title, string content, User author, DateTime createdAt, bool isClosed, int commentCount = 0)
    {
        Id = id;
        Title = title;
        Content = content;
        Author = author;
        CreatedAt = createdAt;
        IsClosed = isClosed;
        CommentCount = commentCount;
    }

    public void Close() => IsClosed = true;
}
