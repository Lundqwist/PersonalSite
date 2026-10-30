// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 3 of 10 — CvTranslations: adds the English columns (TitleEn,
// OrganizationEn, LocationEn, CommentEn) and the course lists (Courses,
// CoursesEn) to the CV, then reloads all CV rows from Data/CvSeedData.cs.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261012010000_CvTranslations")]
public partial class CvTranslations : Migration
{
    private static readonly (string Name, string Type, int MaxLength)[] NewColumns =
    {
        ("TitleEn", "varchar(200)", 200),
        ("OrganizationEn", "varchar(200)", 200),
        ("LocationEn", "varchar(200)", 200),
        ("Courses", "varchar(1000)", 1000),
        ("CoursesEn", "varchar(1000)", 1000),
        ("CommentEn", "varchar(2000)", 2000),
    };

    private static readonly string[] Columns =
    {
        "Id", "Type", "Title", "TitleEn", "Organization", "OrganizationEn", "Location", "LocationEn",
        "StartDate", "EndDate", "DisplayDate", "IsCurrent", "Courses", "CoursesEn", "Comment", "CommentEn", "SortOrder"
    };

    private static readonly string[] ColumnTypes =
    {
        "int", "varchar(50)", "varchar(200)", "varchar(200)", "varchar(200)", "varchar(200)", "varchar(200)", "varchar(200)",
        "date", "date", "varchar(100)", "tinyint(1)", "varchar(1000)", "varchar(1000)", "varchar(2000)", "varchar(2000)", "int"
    };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var (name, type, maxLength) in NewColumns)
        {
            migrationBuilder.AddColumn<string>(
                name: name,
                table: "CvEntries",
                type: type,
                maxLength: maxLength,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        // Replace all CV rows with the translated data
        migrationBuilder.Sql("DELETE FROM `CvEntries`;");

        var entries = CvSeedData.Entries;
        var values = new object[entries.Length, Columns.Length];
        for (var i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            object[] row =
            {
                e.Id, e.Type.ToString(), e.Title, e.TitleEn, e.Organization, e.OrganizationEn, e.Location, e.LocationEn,
                e.StartDate, e.EndDate, e.DisplayDate, e.IsCurrent, e.Courses, e.CoursesEn, e.Comment, e.CommentEn, e.SortOrder
            };
            for (var j = 0; j < row.Length; j++)
                values[i, j] = row[j];
        }

        migrationBuilder.InsertData(
            table: "CvEntries",
            columns: Columns,
            columnTypes: ColumnTypes,
            values: values);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var (name, _, _) in NewColumns)
        {
            migrationBuilder.DropColumn(
                name: name,
                table: "CvEntries");
        }
    }
}
