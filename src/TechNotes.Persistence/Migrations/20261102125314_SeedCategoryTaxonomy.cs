using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNotes.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoryTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Categories"
                SET "Name" = '.NET / ASP.NET Core', "UpdatedAtUtc" = now()
                WHERE "Name" = 'dotnet';

                WITH parents("Name", "Description", "SortOrder") AS (
                    VALUES
                        ('Languages', 'Programming language references and fundamentals.', 10),
                        ('Databases', 'Database concepts, querying, and storage systems.', 20),
                        ('Frameworks & Libraries', 'Application frameworks, libraries, and data-access tooling.', 30)
                )
                INSERT INTO "Categories" (
                    "Id", "UserId", "Name", "Description", "ParentCategoryId",
                    "SortOrder", "CreatedAtUtc", "UpdatedAtUtc")
                SELECT
                    gen_random_uuid(), users."UserId", parents."Name", parents."Description",
                    NULL, parents."SortOrder", now(), NULL
                FROM (SELECT DISTINCT "UserId" FROM "Categories") AS users
                CROSS JOIN parents
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "Categories" AS existing
                    WHERE existing."UserId" = users."UserId"
                      AND existing."Name" = parents."Name"
                );

                WITH mapping("Name", "ParentName", "SortOrder") AS (
                    VALUES
                        ('C#', 'Languages', 10),
                        ('JavaScript', 'Languages', 20),
                        ('SQL', 'Databases', 10),
                        ('Angular', 'Frameworks & Libraries', 10),
                        ('React', 'Frameworks & Libraries', 20),
                        ('Redux', 'Frameworks & Libraries', 30),
                        ('TanStack Query', 'Frameworks & Libraries', 40),
                        ('ADO.NET', 'Frameworks & Libraries', 50),
                        ('EF Core', 'Frameworks & Libraries', 60),
                        ('.NET / ASP.NET Core', 'Frameworks & Libraries', 70)
                )
                UPDATE "Categories" AS child
                SET
                    "ParentCategoryId" = parent."Id",
                    "SortOrder" = mapping."SortOrder",
                    "UpdatedAtUtc" = now()
                FROM mapping
                JOIN "Categories" AS parent
                    ON parent."Name" = mapping."ParentName"
                WHERE child."Name" = mapping."Name"
                  AND child."UserId" = parent."UserId";

                WITH mapping("CategoryName", "TagName") AS (
                    VALUES
                        ('C#', 'C#'),
                        ('C#', '.NET'),
                        ('C#', 'Language'),
                        ('.NET / ASP.NET Core', 'C#'),
                        ('.NET / ASP.NET Core', '.NET'),
                        ('.NET / ASP.NET Core', 'ASP.NET Core'),
                        ('.NET / ASP.NET Core', 'Backend'),
                        ('ADO.NET', 'C#'),
                        ('ADO.NET', '.NET'),
                        ('ADO.NET', 'Data Access'),
                        ('EF Core', 'C#'),
                        ('EF Core', '.NET'),
                        ('EF Core', 'ORM'),
                        ('EF Core', 'Data Access'),
                        ('SQL', 'SQL'),
                        ('SQL', 'SQL Server'),
                        ('SQL', 'PostgreSQL'),
                        ('SQL', 'Database'),
                        ('JavaScript', 'JavaScript'),
                        ('JavaScript', 'Browser'),
                        ('JavaScript', 'Language'),
                        ('Angular', 'TypeScript'),
                        ('Angular', 'JavaScript'),
                        ('Angular', 'Frontend'),
                        ('Angular', 'Framework'),
                        ('React', 'JavaScript'),
                        ('React', 'TypeScript'),
                        ('React', 'Frontend'),
                        ('React', 'Library'),
                        ('Redux', 'JavaScript'),
                        ('Redux', 'TypeScript'),
                        ('Redux', 'React'),
                        ('Redux', 'State Management'),
                        ('TanStack Query', 'JavaScript'),
                        ('TanStack Query', 'TypeScript'),
                        ('TanStack Query', 'React'),
                        ('TanStack Query', 'Server State')
                )
                INSERT INTO "Tags" ("Id", "Name", "CreatedAtUtc", "UpdatedAtUtc")
                SELECT gen_random_uuid(), distinct_tags."TagName", now(), NULL
                FROM (SELECT DISTINCT "TagName" FROM mapping) AS distinct_tags
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "Tags" AS existing
                    WHERE lower(existing."Name") = lower(distinct_tags."TagName")
                );

                WITH mapping("CategoryName", "TagName") AS (
                    VALUES
                        ('C#', 'C#'),
                        ('C#', '.NET'),
                        ('C#', 'Language'),
                        ('.NET / ASP.NET Core', 'C#'),
                        ('.NET / ASP.NET Core', '.NET'),
                        ('.NET / ASP.NET Core', 'ASP.NET Core'),
                        ('.NET / ASP.NET Core', 'Backend'),
                        ('ADO.NET', 'C#'),
                        ('ADO.NET', '.NET'),
                        ('ADO.NET', 'Data Access'),
                        ('EF Core', 'C#'),
                        ('EF Core', '.NET'),
                        ('EF Core', 'ORM'),
                        ('EF Core', 'Data Access'),
                        ('SQL', 'SQL'),
                        ('SQL', 'SQL Server'),
                        ('SQL', 'PostgreSQL'),
                        ('SQL', 'Database'),
                        ('JavaScript', 'JavaScript'),
                        ('JavaScript', 'Browser'),
                        ('JavaScript', 'Language'),
                        ('Angular', 'TypeScript'),
                        ('Angular', 'JavaScript'),
                        ('Angular', 'Frontend'),
                        ('Angular', 'Framework'),
                        ('React', 'JavaScript'),
                        ('React', 'TypeScript'),
                        ('React', 'Frontend'),
                        ('React', 'Library'),
                        ('Redux', 'JavaScript'),
                        ('Redux', 'TypeScript'),
                        ('Redux', 'React'),
                        ('Redux', 'State Management'),
                        ('TanStack Query', 'JavaScript'),
                        ('TanStack Query', 'TypeScript'),
                        ('TanStack Query', 'React'),
                        ('TanStack Query', 'Server State')
                )
                INSERT INTO "NoteTags" ("Id", "NoteId", "TagId", "CreatedAtUtc", "UpdatedAtUtc")
                SELECT gen_random_uuid(), notes."Id", tags."Id", now(), NULL
                FROM mapping
                JOIN "Categories" AS categories
                    ON categories."Name" = mapping."CategoryName"
                JOIN "Notes" AS notes
                    ON notes."CategoryId" = categories."Id"
                JOIN "Tags" AS tags
                    ON lower(tags."Name") = lower(mapping."TagName")
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "NoteTags" AS existing
                    WHERE existing."NoteId" = notes."Id"
                      AND existing."TagId" = tags."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "NoteTags" AS note_tags
                USING "Tags" AS tags
                WHERE note_tags."TagId" = tags."Id"
                  AND lower(tags."Name") IN (
                      'c#', '.net', 'language', 'asp.net core', 'backend',
                      'data access', 'orm', 'sql', 'sql server', 'postgresql',
                      'database', 'javascript', 'browser', 'typescript', 'frontend',
                      'framework', 'library', 'react', 'state management', 'server state');

                DELETE FROM "Tags" AS tags
                WHERE lower(tags."Name") IN (
                    'c#', '.net', 'language', 'asp.net core', 'backend',
                    'data access', 'orm', 'sql', 'sql server', 'postgresql',
                    'database', 'javascript', 'browser', 'typescript', 'frontend',
                    'framework', 'library', 'react', 'state management', 'server state')
                  AND NOT EXISTS (
                      SELECT 1 FROM "NoteTags" AS note_tags WHERE note_tags."TagId" = tags."Id"
                  );

                UPDATE "Categories"
                SET "ParentCategoryId" = NULL, "SortOrder" = 0, "UpdatedAtUtc" = now();

                DELETE FROM "Categories" AS categories
                WHERE categories."Name" IN ('Languages', 'Databases', 'Frameworks & Libraries')
                  AND NOT EXISTS (
                      SELECT 1 FROM "Notes" AS notes WHERE notes."CategoryId" = categories."Id"
                  )
                  AND NOT EXISTS (
                      SELECT 1 FROM "Categories" AS children WHERE children."ParentCategoryId" = categories."Id"
                  );

                UPDATE "Categories"
                SET "Name" = 'dotnet', "UpdatedAtUtc" = now()
                WHERE "Name" = '.NET / ASP.NET Core';
                """);
        }
    }
}
