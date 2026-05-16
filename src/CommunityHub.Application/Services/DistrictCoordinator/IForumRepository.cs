using System.Collections.Generic;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface IForumRepository
{
    Forum Create(Forum forum);
    List<Forum> GetVisibleForums(long currentUserId);
    List<Forum> Search(long currentUserId, string? searchText);
    Forum? GetById(long forumId);
    void Close(long forumId);

    ForumComment AddComment(ForumComment comment);
    List<ForumComment> GetCommentsByForumId(long forumId);

    void SetReaction(long commentId, long userId, bool isPositive);
    void RemoveReaction(long commentId, long userId);
    bool? GetUserReaction(long commentId, long userId);
}
