using System;
using FoutloosTypen.Core.Data;

namespace FoutloosTypen.Core.Data.Repositories
{
    internal class PracticeMaterialRepository : DatabaseConnection
    {
        public PracticeMaterialRepository()
        {
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS PracticeMaterials (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Sentences NVARCHAR(300) NOT NULL,
                    AssignmentId INTEGER NOT NULL
                );
            ");
        }
    }
}
