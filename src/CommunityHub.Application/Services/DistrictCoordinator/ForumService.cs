using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class ForumService
{
    private readonly IForumRepository _repository;

    public ForumService(IForumRepository repository)
    {
        _repository = repository;
    }

    public ForumDto Create(CreateForumDto dto)
    {
        User author = new User(dto.CoordinatorId, "", "", "", "", System.DateTime.MinValue, "DistrictCoordinator");
        Forum forum = new Forum(dto.Title, dto.Content, author);
        Forum created = _repository.Create(forum);
        return MapForumToDto(created, dto.CoordinatorId);
    }

    public List<ForumDto> GetVisibleForums(long currentUserId)
    {
        return _repository.GetVisibleForums(currentUserId)
            .Select(f => MapForumToDto(f, currentUserId))
            .ToList();
    }

    public List<ForumDto> Search(long currentUserId, string? searchText)
    {
        return _repository.Search(currentUserId, searchText)
            .Select(f => MapForumToDto(f, currentUserId))
            .ToList();
    }

    public ForumDto? GetById(long forumId, long currentUserId)
    {
        Forum? forum = _repository.GetById(forumId);
        if (forum == null) return null;
        return MapForumToDto(forum, currentUserId);
    }

    public void CloseForum(long forumId)
    {
        _repository.Close(forumId);
    }

    public List<ForumCommentDto> GetComments(long forumId, long currentUserId, long forumAuthorId)
    {
        List<ForumComment> comments = _repository.GetCommentsByForumId(forumId);
        return comments.Select(c =>
        {
            bool? userReaction = _repository.GetUserReaction(c.Id, currentUserId);
            return MapCommentToDto(c, forumAuthorId, userReaction);
        }).ToList();
    }

    public void AddComment(long forumId, long authorId, string content)
    {
        User author = new User(authorId, "", "", "", "", System.DateTime.MinValue, "DistrictCoordinator");
        ForumComment comment = new ForumComment(forumId, author, content);
        _repository.AddComment(comment);
    }

    public void ToggleReaction(long commentId, long userId, bool isPositive)
    {
        bool? existing = _repository.GetUserReaction(commentId, userId);

        if (existing.HasValue && existing.Value == isPositive)
        {
            _repository.RemoveReaction(commentId, userId);
        }
        else
        {
            _repository.SetReaction(commentId, userId, isPositive);
        }
    }

    private static ForumDto MapForumToDto(Forum f, long currentUserId) => new ForumDto
    {
        Id = f.Id,
        Title = f.Title,
        Content = f.Content,
        AuthorId = f.Author.Id,
        AuthorFullName = $"{f.Author.Name} {f.Author.Surname}".Trim(),
        CreatedAt = f.CreatedAt,
        IsClosed = f.IsClosed,
        CommentCount = f.CommentCount,
        IsOwner = f.Author.Id == currentUserId
    };

    private static ForumCommentDto MapCommentToDto(ForumComment c, long forumAuthorId, bool? userReaction)
    {
        string name = c.Author.Name ?? "";
        string surname = c.Author.Surname ?? "";
        string initials = $"{(name.Length > 0 ? name[0] : '?')}{(surname.Length > 0 ? surname[0] : '?')}";

        return new ForumCommentDto
        {
            Id = c.Id,
            AuthorId = c.Author.Id,
            AuthorFullName = $"{name} {surname}".Trim(),
            AuthorInitials = initials.ToUpper(),
            AuthorDistrictInfo = $"{name} {surname}, {c.AuthorDistrictName}",
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            LikeCount = c.LikeCount,
            DislikeCount = c.DislikeCount,
            IsForumAuthor = c.Author.Id == forumAuthorId,
            CurrentUserReaction = userReaction
        };
    }
}
