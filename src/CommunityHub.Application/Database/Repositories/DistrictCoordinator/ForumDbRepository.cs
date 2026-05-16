using System;
using System.Collections.Generic;
using System.Data;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class ForumDbRepository : IForumRepository
{
    public Forum Create(Forum forum)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO coordinator_forums (title, content, created_by_user_id, created_at, is_closed)
            VALUES (@title, @content, @authorId, @createdAt, FALSE)
            RETURNING id";

        AddParameter(cmd, "@title", forum.Title);
        AddParameter(cmd, "@content", forum.Content);
        AddParameter(cmd, "@authorId", forum.Author.Id);
        AddParameter(cmd, "@createdAt", forum.CreatedAt);

        long id = Convert.ToInt64(cmd.ExecuteScalar());
        return new Forum(id, forum.Title, forum.Content, forum.Author, forum.CreatedAt, false);
    }

    public List<Forum> GetVisibleForums(long currentUserId)
    {
        return Search(currentUserId, null);
    }

    public List<Forum> Search(long currentUserId, string? searchText)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT f.id, f.title, f.content, f.created_at, f.is_closed,
                   u.id AS author_id, u.name, u.surname,
                   COALESCE(c.comment_count, 0) AS comment_count
            FROM coordinator_forums f
            JOIN users u ON u.id = f.created_by_user_id
            LEFT JOIN (
                SELECT forum_id, COUNT(*) AS comment_count
                FROM coordinator_forum_comments
                GROUP BY forum_id
            ) c ON c.forum_id = f.id
            WHERE (f.is_closed IS NOT TRUE OR f.created_by_user_id = @currentUserId)";

        AddParameter(cmd, "@currentUserId", currentUserId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            cmd.CommandText += @"
              AND (f.title ILIKE @search OR f.content ILIKE @search)";
            AddParameter(cmd, "@search", "%" + searchText.Trim() + "%");
        }

        cmd.CommandText += " ORDER BY f.created_at DESC";

        List<Forum> forums = new List<Forum>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            forums.Add(MapForum(reader));
        }

        return forums;
    }

    public Forum? GetById(long forumId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT f.id, f.title, f.content, f.created_at, f.is_closed,
                   u.id AS author_id, u.name, u.surname,
                   COALESCE(c.comment_count, 0) AS comment_count
            FROM coordinator_forums f
            JOIN users u ON u.id = f.created_by_user_id
            LEFT JOIN (
                SELECT forum_id, COUNT(*) AS comment_count
                FROM coordinator_forum_comments
                GROUP BY forum_id
            ) c ON c.forum_id = f.id
            WHERE f.id = @forumId";

        AddParameter(cmd, "@forumId", forumId);

        using IDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? MapForum(reader) : null;
    }

    public void Close(long forumId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE coordinator_forums SET is_closed = TRUE WHERE id = @forumId";
        AddParameter(cmd, "@forumId", forumId);
        cmd.ExecuteNonQuery();
    }

    public ForumComment AddComment(ForumComment comment)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO coordinator_forum_comments (forum_id, author_id, content, created_at)
            VALUES (@forumId, @authorId, @content, @createdAt)
            RETURNING id";

        AddParameter(cmd, "@forumId", comment.ForumId);
        AddParameter(cmd, "@authorId", comment.Author.Id);
        AddParameter(cmd, "@content", comment.Content);
        AddParameter(cmd, "@createdAt", comment.CreatedAt);

        long id = Convert.ToInt64(cmd.ExecuteScalar());
        return new ForumComment(id, comment.ForumId, comment.Author, string.Empty,
            comment.Content, comment.CreatedAt, 0, 0);
    }

    public List<ForumComment> GetCommentsByForumId(long forumId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT fc.id, fc.forum_id, fc.content, fc.created_at,
                   u.id AS author_id, u.name, u.surname,
                   COALESCE(d.name, 'Nepoznat kvart') AS district_name,
                   COALESCE(likes.cnt, 0) AS like_count,
                   COALESCE(dislikes.cnt, 0) AS dislike_count
            FROM coordinator_forum_comments fc
            JOIN users u ON u.id = fc.author_id
            LEFT JOIN districts d ON d.coordinator_id = u.id
            LEFT JOIN (
                SELECT comment_id, COUNT(*) AS cnt
                FROM coordinator_forum_comment_reactions
                WHERE is_positive = TRUE
                GROUP BY comment_id
            ) likes ON likes.comment_id = fc.id
            LEFT JOIN (
                SELECT comment_id, COUNT(*) AS cnt
                FROM coordinator_forum_comment_reactions
                WHERE is_positive = FALSE
                GROUP BY comment_id
            ) dislikes ON dislikes.comment_id = fc.id
            WHERE fc.forum_id = @forumId
            ORDER BY fc.created_at ASC";

        AddParameter(cmd, "@forumId", forumId);

        List<ForumComment> comments = new List<ForumComment>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            comments.Add(MapComment(reader));
        }

        return comments;
    }

    public void SetReaction(long commentId, long userId, bool isPositive)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO coordinator_forum_comment_reactions (comment_id, user_id, is_positive)
            VALUES (@commentId, @userId, @isPositive)
            ON CONFLICT (comment_id, user_id)
            DO UPDATE SET is_positive = @isPositive";

        AddParameter(cmd, "@commentId", commentId);
        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@isPositive", isPositive);
        cmd.ExecuteNonQuery();
    }

    public void RemoveReaction(long commentId, long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM coordinator_forum_comment_reactions
            WHERE comment_id = @commentId AND user_id = @userId";

        AddParameter(cmd, "@commentId", commentId);
        AddParameter(cmd, "@userId", userId);
        cmd.ExecuteNonQuery();
    }

    public bool? GetUserReaction(long commentId, long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT is_positive
            FROM coordinator_forum_comment_reactions
            WHERE comment_id = @commentId AND user_id = @userId";

        AddParameter(cmd, "@commentId", commentId);
        AddParameter(cmd, "@userId", userId);

        object? result = cmd.ExecuteScalar();
        return result == null ? null : (bool)result;
    }

    private static Forum MapForum(IDataReader reader)
    {
        User author = new User(
            reader.GetInt64(reader.GetOrdinal("author_id")),
            "", "",
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            DateTime.MinValue, "DistrictCoordinator");

        return new Forum(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("title")),
            reader.GetString(reader.GetOrdinal("content")),
            author,
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetBoolean(reader.GetOrdinal("is_closed")),
            Convert.ToInt32(reader.GetValue(reader.GetOrdinal("comment_count"))));
    }

    private static ForumComment MapComment(IDataReader reader)
    {
        User author = new User(
            reader.GetInt64(reader.GetOrdinal("author_id")),
            "", "",
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            DateTime.MinValue, "DistrictCoordinator");

        return new ForumComment(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetInt64(reader.GetOrdinal("forum_id")),
            author,
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("content")),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            Convert.ToInt32(reader.GetValue(reader.GetOrdinal("like_count"))),
            Convert.ToInt32(reader.GetValue(reader.GetOrdinal("dislike_count"))));
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
