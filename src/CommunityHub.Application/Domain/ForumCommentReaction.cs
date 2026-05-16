namespace CommunityHub.Application.Domain;

public class ForumCommentReaction
{
    public long Id { get; private set; }
    public long CommentId { get; private set; }
    public long UserId { get; private set; }
    public bool IsPositive { get; private set; }

    public ForumCommentReaction(long commentId, long userId, bool isPositive)
    {
        CommentId = commentId;
        UserId = userId;
        IsPositive = isPositive;
    }

    public ForumCommentReaction(long id, long commentId, long userId, bool isPositive)
    {
        Id = id;
        CommentId = commentId;
        UserId = userId;
        IsPositive = isPositive;
    }
}
