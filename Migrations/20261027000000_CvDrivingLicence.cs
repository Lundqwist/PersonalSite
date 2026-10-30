// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 9 of 11 — CvDrivingLicence: adds a driving licence line to the end of
// the CV skills (PageContents row "CvSkills", text from Data/CvSeedData.cs).
// Appends instead of overwriting, so edits made on /Admin/Cv/Profile are kept.
// No table changes.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;
using PersonalSite.Models;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261027000000_CvDrivingLicence")]
public partial class CvDrivingLicence : Migration
{
    // CHAR(10) = the line break between skill groups. On a new database the
    // CvProfile migration already inserts the line (it uses the current seed
    // text), so it is only added where it is missing.
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($@"
            UPDATE PageContents
            SET Body   = IF(LOCATE('{CvSeedData.DrivingLicenceEn}', Body) > 0, Body,
                            CONCAT(Body, CHAR(10), '{CvSeedData.DrivingLicenceEn}')),
                BodySv = IF(LOCATE('{CvSeedData.DrivingLicenceSv}', BodySv) > 0, BodySv,
                            CONCAT(BodySv, CHAR(10), '{CvSeedData.DrivingLicenceSv}')),
                UpdatedAt = UTC_TIMESTAMP(6)
            WHERE `Key` = '{PageContent.CvSkillsKey}';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($@"
            UPDATE PageContents
            SET Body   = REPLACE(Body,   CONCAT(CHAR(10), '{CvSeedData.DrivingLicenceEn}'), ''),
                BodySv = REPLACE(BodySv, CONCAT(CHAR(10), '{CvSeedData.DrivingLicenceSv}'), '')
            WHERE `Key` = '{PageContent.CvSkillsKey}';");
    }
}
