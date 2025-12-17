using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class SharePostRepository : DatabaseConnection, FoutloosTypen.Core.Interfaces.Repositories.ISharePostRepository
    {
        public SharePostRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS SharePosts (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [LessonId] INTEGER NOT NULL,
                        [Text] NVARCHAR(280) NOT NULL,
                        [ImagePath] NVARCHAR(260) NOT NULL,
                        [CreatedAt] NVARCHAR(50) NOT NULL
                )");
        }

        public SharePost Add(SharePost post)
        {
            OpenConnection();
            using var cmd = new SqliteCommand(@"INSERT INTO SharePosts(LessonId, Text, ImagePath, CreatedAt)
                                                VALUES($lessonId, $text, $imagePath, $createdAt);
                                                SELECT last_insert_rowid();", Connection);
            cmd.Parameters.AddWithValue("$lessonId", post.LessonId);
            cmd.Parameters.AddWithValue("$text", post.Text);
            cmd.Parameters.AddWithValue("$imagePath", post.ImagePath);
            cmd.Parameters.AddWithValue("$createdAt", post.CreatedAt.ToString("o"));
            var id = (long)cmd.ExecuteScalar();
            post.Id = (int)id;
            CloseConnection();
            return post;
        }

        public IEnumerable<SharePost> GetAll()
        {
            var list = new List<SharePost>();
            OpenConnection();
            using var cmd = new SqliteCommand("SELECT Id, LessonId, Text, ImagePath, CreatedAt FROM SharePosts", Connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SharePost
                {
                    Id = reader.GetInt32(0),
                    LessonId = reader.GetInt32(1),
                    Text = reader.GetString(2),
                    ImagePath = reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind)
                });
            }
            CloseConnection();
            return list;
        }
    }
}
