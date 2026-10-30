// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 6 of 11 — PostLanguage: posts become single-language.
// Adds the Language column ("sv"/"en"), copies any English text into separate
// English posts (so nothing was lost), then removes the old *En columns.
// migrationBuilder.Sql(...) runs a hand-written SQL statement.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

// Stories and projects become single-language posts (sv or en).
// Existing posts keep their Swedish text as a Swedish post; any English text
// they had is copied into a separate English post, so nothing is lost.
[DbContext(typeof(AppDbContext))]
[Migration("20261012040000_PostLanguage")]
public partial class PostLanguage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Language",
            table: "Posts",
            type: "varchar(2)",
            maxLength: 2,
            nullable: false,
            defaultValue: "sv")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.Sql(@"
            INSERT INTO `Posts` (`Category`, `Language`, `Title`, `Summary`, `Body`, `CoverImageId`, `Url`, `PublishedDate`, `IsPublished`, `UpdatedAt`)
            SELECT `Category`, 'en',
                   COALESCE(NULLIF(TRIM(`TitleEn`), ''), `Title`),
                   COALESCE(NULLIF(TRIM(`SummaryEn`), ''), `Summary`),
                   `BodyEn`, `CoverImageId`, `Url`, `PublishedDate`, `IsPublished`, `UpdatedAt`
            FROM `Posts`
            WHERE NULLIF(TRIM(`TitleEn`), '') IS NOT NULL
               OR NULLIF(TRIM(`SummaryEn`), '') IS NOT NULL
               OR NULLIF(TRIM(`BodyEn`), '') IS NOT NULL;");

        migrationBuilder.DropColumn(name: "TitleEn", table: "Posts");
        migrationBuilder.DropColumn(name: "SummaryEn", table: "Posts");
        migrationBuilder.DropColumn(name: "BodyEn", table: "Posts");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TitleEn",
            table: "Posts",
            type: "varchar(200)",
            maxLength: 200,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "SummaryEn",
            table: "Posts",
            type: "varchar(500)",
            maxLength: 500,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "BodyEn",
            table: "Posts",
            type: "longtext",
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.DropColumn(name: "Language", table: "Posts");
    }
}
