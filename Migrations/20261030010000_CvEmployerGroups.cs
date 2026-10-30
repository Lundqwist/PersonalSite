// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 11 of 11 — CvEmployerGroups: adds the Employer column to CvEntries
// and puts the LBS and NTI schools under "AcadeMedia" (2016–2024), so the CV
// shows one employer with its schools beneath. Also adds the motorcycle
// licence (A) to the driving licence line of the skills.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;
using PersonalSite.Models;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261030010000_CvEmployerGroups")]
public partial class CvEmployerGroups : Migration
{
    private const string Licences = "Driving licence: Car (manual and automatic)";
    private const string LicencesSv = "Körkort: Bil (manuell och automat)";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Employer",
            table: "CvEntries",
            type: "varchar(200)",
            maxLength: 200,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.Sql(@"
            UPDATE CvEntries SET Employer = 'AcadeMedia'
            WHERE Type = 'Workplace' AND Organization IN ('LBS Kreativa Gymnasiet', 'NTI Gymnasiet');");

        // Only where the motorcycle is not listed yet (a new database already has it, see CvSeedData)
        migrationBuilder.Sql($@"
            UPDATE PageContents
            SET Body   = IF(LOCATE('{CvSeedData.DrivingLicenceEn}', Body) > 0, Body,
                            REPLACE(Body, '{Licences}', '{CvSeedData.DrivingLicenceEn}')),
                BodySv = IF(LOCATE('{CvSeedData.DrivingLicenceSv}', BodySv) > 0, BodySv,
                            REPLACE(BodySv, '{LicencesSv}', '{CvSeedData.DrivingLicenceSv}')),
                UpdatedAt = UTC_TIMESTAMP(6)
            WHERE `Key` = '{PageContent.CvSkillsKey}';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Employer",
            table: "CvEntries");

        migrationBuilder.Sql($@"
            UPDATE PageContents
            SET Body   = REPLACE(Body,   '{CvSeedData.DrivingLicenceEn}', '{Licences}'),
                BodySv = REPLACE(BodySv, '{CvSeedData.DrivingLicenceSv}', '{LicencesSv}')
            WHERE `Key` = '{PageContent.CvSkillsKey}';");
    }
}
