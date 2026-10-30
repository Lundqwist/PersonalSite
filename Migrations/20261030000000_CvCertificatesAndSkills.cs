// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 10 of 10 — CvCertificatesAndSkills: adds three certificates to the
// CV courses (AI in teaching, GDPR, English test), adds Unity, EF Core, Git,
// Linux, Docker and self-hosting to the skills, gives English its CEFR level,
// and turns the Swedish Louis Gibeck notes into bullets like the English ones.
// Edits the live rows in place (REPLACE / WHERE NOT EXISTS), so other edits
// made on /Admin/Cv are kept. On a new database the skills already have the new
// text (Data/CvSeedData.cs), so the REPLACEs find nothing to change.
// No table changes.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;
using PersonalSite.Models;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261030000000_CvCertificatesAndSkills")]
public partial class CvCertificatesAndSkills : Migration
{
    // (old text, new text) for the skills rows. A skills item ends at a comma
    // (see CvProfile.ParseSkills), so the new items contain no commas inside.
    private static readonly (string Old, string New)[] SkillsEn =
    {
        ("Programming: C#, .NET, ASP.NET Core, C++, HTML, CSS, JavaScript, SQL",
         "Programming: C#, .NET, ASP.NET Core, Entity Framework Core, C++, Unity, HTML, CSS, JavaScript, SQL, MariaDB/MySQL, Git"),
        ("IT administration, MS Exchange Server, Client support",
         "IT administration, MS Exchange Server, Client support, Linux, Docker, Self-hosting on Synology NAS and Raspberry Pi, WireGuard VPN, Pi-hole DNS"),
        ("Programming and web courses, Supporting students with NPF",
         "Programming and web courses, Supporting students with NPF, Generative AI in teaching"),
        ("English (fluent)", "English (CEFR C1)"),
    };

    private static readonly (string Old, string New)[] SkillsSv =
    {
        ("Programmering: C#, .NET, ASP.NET Core, C++, HTML, CSS, JavaScript, SQL",
         "Programmering: C#, .NET, ASP.NET Core, Entity Framework Core, C++, Unity, HTML, CSS, JavaScript, SQL, MariaDB/MySQL, Git"),
        ("IT-administration, MS Exchange Server, Klientsupport",
         "IT-administration, MS Exchange Server, Klientsupport, Linux, Docker, Egen drift på Synology NAS och Raspberry Pi, WireGuard VPN, Pi-hole DNS"),
        ("Programmerings- och webbkurser, Stöd för elever med NPF",
         "Programmerings- och webbkurser, Stöd för elever med NPF, Generativ AI i undervisningen"),
        ("Engelska (flytande)", "Engelska (CEFR C1)"),
    };

    // The new courses, newest first. Existing courses move down three places.
    private static readonly (string Title, string TitleEn, string Org, string OrgEn, string Date, string Comment, string CommentEn)[] Certificates =
    {
        ("Språktest i engelska, CEFR C1", "English language test, CEFR C1", "Eurotest", "Eurotest", "2026-08-21",
         "Ordförråd och grammatik C1, hörförståelse över C1, läsförståelse B2.",
         "Vocabulary and grammar C1, listening above C1, reading B2."),
        ("Generativ AI och lärande, 40 timmar", "Generative AI and Learning, 40 hours", "Luleå tekniska universitet", "Luleå University of Technology", "2024-01-15",
         "Användning av generativ AI för att stödja lärande i skola och högre utbildning: anpassning av lärande, bedömning och AI:s påverkan på undervisningen.",
         "Using generative AI to support learning in schools and higher education: adapting learning, assessment, and how AI is changing teaching."),
        ("GDPR-utbildning 2.0", "GDPR Training 2.0", "AcadeMedia", "AcadeMedia", "2023-01-16", null, null),
    };

    // Matched on its start, so line breaks saved as "\r\n" by the admin form still match
    private const string LouisGibeckOldStart = "Ansvarat för drift av koncernens nätverk";

    private const string LouisGibeckOld =
        "Ansvarat för drift av koncernens nätverk, mailsystem och persondatorer.\nPersonalutbildare samt ansvarig för inköp av datautrustning.";

    private const string LouisGibeckNew =
        "Ansvarade för IT-driften på koncernens internationella kontor.\n" +
        "• Drev nätverk, servrar, mailsystem (MS Exchange), arbetsstationer, bärbara datorer och mobiltelefoner\n" +
        "• Förvaltade servrar i Upplands Väsby, Bromma, Kuala Lumpur (Malaysia), Dallas (USA) och Rotterdam (Nederländerna)\n" +
        "• Utbildade personalen i allt från MS Office till backupsystem för servrar\n" +
        "• Ansvarig för IT-inköp";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($@"
            UPDATE PageContents
            SET Body = {Replaces("Body", SkillsEn, toNew: true)},
                BodySv = {Replaces("BodySv", SkillsSv, toNew: true)},
                UpdatedAt = UTC_TIMESTAMP(6)
            WHERE `Key` = '{PageContent.CvSkillsKey}';");

        // Move the existing courses down once, only if the certificates are not there yet
        migrationBuilder.Sql($@"
            UPDATE CvEntries SET SortOrder = SortOrder + {Certificates.Length}
            WHERE Type = 'Course'
              AND NOT EXISTS (SELECT 1 FROM (SELECT Title FROM CvEntries) c WHERE c.Title = '{Certificates[0].Title}');");

        for (var i = 0; i < Certificates.Length; i++)
        {
            var c = Certificates[i];
            migrationBuilder.Sql($@"
                INSERT INTO CvEntries (Type, Title, TitleEn, Organization, OrganizationEn, StartDate, DisplayDate, IsCurrent, Comment, CommentEn, SortOrder)
                SELECT 'Course', '{c.Title}', '{c.TitleEn}', '{c.Org}', '{c.OrgEn}', '{c.Date}', '{c.Date}', 0, {Quote(c.Comment)}, {Quote(c.CommentEn)}, {i + 1}
                FROM DUAL
                WHERE NOT EXISTS (SELECT 1 FROM CvEntries WHERE Title = '{c.Title}');");
        }

        migrationBuilder.Sql($@"
            UPDATE CvEntries SET Comment = '{Escape(LouisGibeckNew)}'
            WHERE Organization = 'Louis Gibeck AB' AND Comment LIKE '{LouisGibeckOldStart}%';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($@"
            UPDATE PageContents
            SET Body = {Replaces("Body", SkillsEn, toNew: false)},
                BodySv = {Replaces("BodySv", SkillsSv, toNew: false)}
            WHERE `Key` = '{PageContent.CvSkillsKey}';");

        foreach (var c in Certificates)
            migrationBuilder.Sql($"DELETE FROM CvEntries WHERE Title = '{c.Title}';");

        migrationBuilder.Sql($@"
            UPDATE CvEntries SET SortOrder = SortOrder - {Certificates.Length}
            WHERE Type = 'Course' AND SortOrder > {Certificates.Length};");

        migrationBuilder.Sql($@"
            UPDATE CvEntries SET Comment = '{Escape(LouisGibeckOld)}'
            WHERE Organization = 'Louis Gibeck AB' AND Comment = '{Escape(LouisGibeckNew)}';");
    }

    // REPLACE(REPLACE(column, 'old1', 'new1'), 'old2', 'new2') ... ; toNew: false swaps the direction
    private static string Replaces(string column, (string Old, string New)[] pairs, bool toNew) =>
        pairs.Aggregate(column, (sql, p) => toNew
            ? $"REPLACE({sql}, '{p.Old}', '{p.New}')"
            : $"REPLACE({sql}, '{p.New}', '{p.Old}')");

    // MySQL string literals: escape quotes and backslashes; "\n" becomes CHAR(10) via '\n'
    private static string Escape(string text) =>
        text.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");

    private static string Quote(string text) => text == null ? "NULL" : $"'{Escape(text)}'";
}
