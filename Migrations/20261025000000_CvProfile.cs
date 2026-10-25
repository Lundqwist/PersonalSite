// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 8 of 8 — CvProfile: adds the CV's profile summary and skills as two
// PageContents rows (text from Data/CvSeedData.cs). No table changes.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;
using PersonalSite.Models;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261025000000_CvProfile")]
public partial class CvProfileText : Migration
{
    private static readonly string[] Columns = { "Key", "Body", "BodySv", "UpdatedAt" };
    private static readonly string[] ColumnTypes = { "varchar(50)", "longtext", "longtext", "datetime(6)" };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var now = DateTime.UtcNow;
        migrationBuilder.InsertData(
            table: "PageContents",
            columns: Columns,
            columnTypes: ColumnTypes,
            values: new object[,]
            {
                { PageContent.CvSummaryKey, CvSeedData.SummaryEn, CvSeedData.SummarySv, now },
                { PageContent.CvSkillsKey,  CvSeedData.SkillsEn,  CvSeedData.SkillsSv,  now },
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "PageContents",
            keyColumn: "Key",
            keyColumnType: "varchar(50)",
            keyValues: new object[] { PageContent.CvSummaryKey, PageContent.CvSkillsKey });
    }
}
