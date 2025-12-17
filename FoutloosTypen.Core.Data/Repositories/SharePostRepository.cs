using FoutloosTypen.Core.Models;
using System.Collections.Generic;
using System.Data.Common;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class SharePostRepository : DatabaseConnection, FoutloosTypen.Core.Interfaces.Repositories.ISharePostRepository
    {
        public SharePostRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS SharePosts (
                        Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                        LessonId INT NOT NULL,
                        Text VARCHAR(280) NOT NULL,
                        ImagePath VARCHAR(260) NOT NULL,
                        CreatedAt VARCHAR(50) NOT NULL
                )");
        }

        public SharePost Add(SharePost post)
        {
            OpenConnection();
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO SharePosts(LessonId, Text, ImagePath, CreatedAt)
                                VALUES(@lessonId, @text, @imagePath, @createdAt);
                                SELECT LAST_INSERT_ID();";
            
            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@lessonId";
            p1.Value = post.LessonId;
            cmd.Parameters.Add(p1);
            
            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@text";
            p2.Value = post.Text;
            cmd.Parameters.Add(p2);
            
            var p3 = cmd.CreateParameter();
            p3.ParameterName = "@imagePath";
            p3.Value = post.ImagePath;
            cmd.Parameters.Add(p3);
            
            var p4 = cmd.CreateParameter();
            p4.ParameterName = "@createdAt";
            p4.Value = post.CreatedAt.ToString("o");
            cmd.Parameters.Add(p4);
            
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            post.Id = id;
            CloseConnection();
            return post;
        }

        public IEnumerable<SharePost> GetAll()
        {
            var list = new List<SharePost>();
            OpenConnection();
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT Id, LessonId, Text, ImagePath, CreatedAt FROM SharePosts";
            using DbDataReader reader = cmd.ExecuteReader();
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
