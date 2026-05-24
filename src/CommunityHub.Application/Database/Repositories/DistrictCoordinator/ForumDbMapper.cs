using System;
using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class ForumDbMapper
{
    internal static Forum MapForum(IDataReader reader)
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

    internal static ForumComment MapComment(IDataReader reader)
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
}
