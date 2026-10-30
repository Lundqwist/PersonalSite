// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 2 of 11 — RealCvData: replaces the template's sample CV with the
// real CV (from Data/CvSeedData.cs), adds the DisplayDate column and makes
// Organization optional. (See InitialCreate for what migrations are.)
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261012000000_RealCvData")]
public partial class RealCvData : Migration
{
    private static readonly string[] Columns =
        { "Id", "Type", "Title", "Organization", "Location", "StartDate", "EndDate", "DisplayDate", "IsCurrent", "Comment", "SortOrder" };

    private static readonly string[] ColumnTypes =
        { "int", "varchar(50)", "varchar(200)", "varchar(200)", "varchar(200)", "date", "date", "varchar(100)", "tinyint(1)", "varchar(2000)", "int" };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DisplayDate",
            table: "CvEntries",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
            name: "Organization",
            table: "CvEntries",
            type: "varchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(200)",
            oldMaxLength: 200)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        // Remove the template sample entries
        migrationBuilder.DeleteData(
            table: "CvEntries",
            keyColumn: "Id",
            keyColumnType: "int",
            keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        // Insert the real CV entries
        var entries = CvSeedData.Entries;
        var values = new object[entries.Length, Columns.Length];
        for (var i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            object[] row = { e.Id, e.Type.ToString(), e.Title, e.Organization, e.Location, e.StartDate, e.EndDate, e.DisplayDate, e.IsCurrent, e.Comment, e.SortOrder };
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
        migrationBuilder.DeleteData(
            table: "CvEntries",
            keyColumn: "Id",
            keyColumnType: "int",
            keyValues: CvSeedData.Entries.Select(e => (object)e.Id).ToArray());

        migrationBuilder.DropColumn(
            name: "DisplayDate",
            table: "CvEntries");

        migrationBuilder.AlterColumn<string>(
            name: "Organization",
            table: "CvEntries",
            type: "varchar(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "varchar(200)",
            oldMaxLength: 200,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
    }
}
