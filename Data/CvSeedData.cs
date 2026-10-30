using PersonalSite.Models;

namespace PersonalSite.Data;

// ════════════════════════════════════════════════════════════════════════════
// Data/CvSeedData.cs — the ORIGINAL CV content, used to fill the database once.
//
// The text comes from Data/Bjarne_Lundqwist_CV.pdf, in Swedish with English
// translations. The migrations RealCvData and CvTranslations copy these rows
// into the database the first time they run. Education entries were corrected
// against Ladok on 2026-10-27 (unfinished studies removed, credits added).
//
// IMPORTANT: after that, the database is the source of truth. Edit the CV on
// the admin page (/Admin/Cv) — changing this file does NOT change the live CV.
//
// Each section is sorted newest first via SortOrder.
// Courses are one per line ("\n" = line break) and shown as a bullet list.
// ════════════════════════════════════════════════════════════════════════════
public static class CvSeedData
{
    // Small helper so dates can be written as D(2022, 8) instead of
    // new DateOnly(2022, 8, 1). "int day = 1" is an optional parameter.
    private static DateOnly D(int year, int month, int day = 1) => new(year, month, day);

    // ── Profile summary and skills (migration CvProfile) ──────
    // Stored as PageContents rows, see Models/CvProfile.cs. Skills: one group
    // per line, "Label: item, item, item".
    public const string SummaryEn =
        "Teacher and developer with a background in IT operations, networks and game development. " +
        "Since 2014 I have taught programming, web development and game development at upper secondary " +
        "schools and a folk high school, and built and led vocational programmes such as Te4 and T4. " +
        "Before teaching I worked in network operations at Skanova (Telia Sonera) and as an IT administrator, " +
        "so I know what it takes to keep real systems running. My students have won awards at the Swedish " +
        "Game Awards, and I was nominated for Sweden's Teacher of the Year in 2019 and 2022. " +
        "I am currently studying full-stack C# .NET at Lexicon and C++ at YrkesAkademin.";

    public const string SummarySv =
        "Lärare och utvecklare med bakgrund inom IT-drift, nätverk och spelutveckling. " +
        "Sedan 2014 har jag undervisat i programmering, webbutveckling och spelutveckling på gymnasier " +
        "och folkhögskola, och byggt upp och lett yrkesutbildningar som Te4 och T4. " +
        "Innan jag blev lärare arbetade jag med nätdrift på Skanova (Telia Sonera) och som IT-administratör, " +
        "så jag vet vad som krävs för att hålla riktiga system igång. Mina elever har vunnit priser i " +
        "Swedish Game Awards, och jag nominerades till årets lärare i Sverige 2019 och 2022. " +
        "Just nu studerar jag fullstack C# .NET på Lexicon och C++ på YrkesAkademin.";

    public const string SkillsEn =
        "Programming: C#, .NET, ASP.NET Core, Entity Framework Core, C++, Unity, HTML, CSS, JavaScript, SQL, MariaDB/MySQL, Git\n" +
        "Game development: Game programming, Game design, 2D graphics, Animation, Digital creation\n" +
        "IT & networks: Network operations, Troubleshooting (copper and fibre), IT administration, MS Exchange Server, Client support, Linux, Docker, Self-hosting on Synology NAS and Raspberry Pi, WireGuard VPN, Pi-hole DNS\n" +
        "Teaching: Course and programme development, Programming and web courses, Supporting students with NPF, Generative AI in teaching\n" +
        "Leadership: Group leadership, Board work, Event organisation (650+ participants)\n" +
        "Languages: Swedish (native), English (CEFR C1)\n" +
        DrivingLicenceEn;

    public const string SkillsSv =
        "Programmering: C#, .NET, ASP.NET Core, Entity Framework Core, C++, Unity, HTML, CSS, JavaScript, SQL, MariaDB/MySQL, Git\n" +
        "Spelutveckling: Spelprogrammering, Speldesign, 2D-grafik, Animation, Digitalt skapande\n" +
        "IT & nätverk: Nätdrift, Felsökning (koppar och fiber), IT-administration, MS Exchange Server, Klientsupport, Linux, Docker, Egen drift på Synology NAS och Raspberry Pi, WireGuard VPN, Pi-hole DNS\n" +
        "Undervisning: Kurs- och utbildningsutveckling, Programmerings- och webbkurser, Stöd för elever med NPF, Generativ AI i undervisningen\n" +
        "Ledarskap: Gruppledarskap, Styrelsearbete, Eventarrangemang (650+ deltagare)\n" +
        "Språk: Svenska (modersmål), Engelska (CEFR C1)\n" +
        DrivingLicenceSv;

    // Added to the live skills text by the migration CvDrivingLicence.
    public const string DrivingLicenceEn = "Driving licence: Car (manual and automatic)";
    public const string DrivingLicenceSv = "Körkort: Bil (manuell och automat)";

    // Names used several times, written once.
    private const string JU = "Jönköping University";
    private const string CampusGotland = "Uppsala Universitet - Campus Gotland";
    private const string CampusGotlandEn = "Uppsala University - Campus Gotland";

    // An array of CvEntry objects. "new() { ... }" creates a CvEntry and sets
    // its properties ("object initializer"); the type is known from the array.
    public static readonly CvEntry[] Entries =
    {
        // ── Workplaces ─────────────────────────────────────────
        new() { Id = 1, Type = CvEntryType.Workplace, SortOrder = 1,
            Title = "Lärare", TitleEn = "Teacher",
            Organization = "LBS Kreativa Gymnasiet", Location = "Växjö",
            StartDate = D(2022, 8), EndDate = D(2024, 6),
            Courses = "Programmering 1-2\nDigitalt Skapande 1-2\nAnimation 1-2",
            CoursesEn = "Programming 1-2\nDigital Creation 1-2\nAnimation 1-2",
            Comment = "Extra lektioner inom Matematik 1c - 2c, Fysik 1-2, Animation.\n\nNominerad och inbjuden till Galan för årets bästa lärare (i Sverige) 2022.",
            CommentEn = "Extra lessons in Mathematics 1c - 2c, Physics 1-2, Animation.\n\nNominated and invited to the gala for Sweden's Teacher of the Year 2022." },
        new() { Id = 2, Type = CvEntryType.Workplace, SortOrder = 2,
            Title = "Lärare", TitleEn = "Teacher",
            Organization = "NTI Gymnasiet", Location = "Luleå",
            StartDate = D(2021, 1), EndDate = D(2022, 6),
            Courses = "Te4 (Gymnasieingenjörsprogrammet) – ansvarig\nProgrammering 1-2\nTillämpad programmering\nWebbserverprogrammering\nWebbutveckling 1",
            CoursesEn = "Te4 (Upper Secondary Engineering Programme) – in charge\nProgramming 1-2\nApplied Programming\nWeb Server Programming\nWeb Development 1",
            Comment = "Utvecklat och genomfört Te4 (påbyggnadsutbildning till Teknikprogrammet). Mjukvaruutvecklingsutbildning med inriktning datorspel.",
            CommentEn = "Developed and ran Te4 (a continuation year of the Technology Programme). A software development programme focused on computer games." },
        new() { Id = 3, Type = CvEntryType.Workplace, SortOrder = 3,
            Title = "Lärare", TitleEn = "Teacher",
            Organization = "LBS Kreativa Gymnasiet", Location = "Helsingborg",
            StartDate = D(2016, 8), EndDate = D(2020, 6),
            Courses = "Programmering 1-2\nCAD 1\nDigitalt Skapande 1-2\nAnimation 1-2\nFysik 1-2\nGymnasiearbete",
            CoursesEn = "Programming 1-2\nCAD 1\nDigital Creation 1-2\nAnimation 1-2\nPhysics 1-2\nDiploma Project",
            Comment = "Extra lektioner inom programmering 1-2, Matematik 1B, 3C, Fysik 1 samt Historia och Kemi 1.\n\nMina elever vann priser i spelutveckling 2 år i rad. Bästa teamet vann LBS Game Awards i 2D-grafik samt vann i Swedish Game Awards.\nNominerad till årets bästa lärare (i Sverige) 2019.",
            CommentEn = "Extra lessons in Programming 1-2, Mathematics 1B, 3C, Physics 1, as well as History and Chemistry 1.\n\nMy students won game development awards two years in a row. The best team won LBS Game Awards for 2D graphics and also won at the Swedish Game Awards.\nNominated for Sweden's Teacher of the Year 2019." },
        new() { Id = 4, Type = CvEntryType.Workplace, SortOrder = 4,
            Title = "Lärare", TitleEn = "Teacher",
            Organization = "Erikdahlbergsgymnasiet", Location = "Jönköping",
            StartDate = D(2016, 2), EndDate = D(2016, 6),
            Courses = "Webbutveckling 1-2\nWebbserverprogrammering 1\nDatorteknik 1a\nProgrammering 1",
            CoursesEn = "Web Development 1-2\nWeb Server Programming 1\nComputer Technology 1a\nProgramming 1",
            Comment = "Genomfört extra lektioner inom programmering.\n\nByggt upp och planerat T4-utbildningen till år 2016-2017 samt gjort reklam nog för att fylla utbildningen med studenter.",
            CommentEn = "Gave extra lessons in programming.\n\nBuilt and planned the T4 programme for 2016-2017, and promoted it enough to fill it with students." },
        new() { Id = 5, Type = CvEntryType.Workplace, SortOrder = 5,
            Title = "Lärare", TitleEn = "Teacher",
            Organization = "Ädelfors Folkhögskola", OrganizationEn = "Ädelfors Folk High School", Location = "Vetlanda",
            StartDate = D(2014, 6), EndDate = D(2016, 2),
            Courses = "Spelprogrammering (program)",
            CoursesEn = "Game Programming (programme)",
            Comment = "Ansvarig för planering och utförande av programmering för spelutveckling.\n\nSamtliga deltagare på utbildningen hade någon form av NPF (Neuropsykologisk Funktionsnedsättning).",
            CommentEn = "Responsible for planning and teaching programming for game development.\n\nAll participants in the programme had some form of neurodevelopmental disorder (NPF)." },
        new() { Id = 6, Type = CvEntryType.Workplace, SortOrder = 6,
            Title = "Nätdriftsingenjör", TitleEn = "Network Operations Engineer",
            Organization = "Skanova - Telia Sonera", Location = "Jönköping",
            StartDate = D(2011, 6), EndDate = D(2011, 8),
            Comment = "Sommarvikariat.", CommentEn = "Summer position." },
        new() { Id = 7, Type = CvEntryType.Workplace, SortOrder = 7,
            Title = "Nätdriftsingenjör", TitleEn = "Network Operations Engineer",
            Organization = "Skanova - Telia Sonera", Location = "Jönköping",
            StartDate = D(2010, 6), EndDate = D(2010, 8),
            Comment = "Sommarvikariat.", CommentEn = "Summer position." },
        new() { Id = 8, Type = CvEntryType.Workplace, SortOrder = 8,
            Title = "Säljare", TitleEn = "Sales Representative",
            Organization = "Atlas Telemarketing",
            Location = "Maspalomas, Gran Canaria, Spanien", LocationEn = "Maspalomas, Gran Canaria, Spain",
            StartDate = D(2009, 11), EndDate = D(2010, 2) },
        new() { Id = 9, Type = CvEntryType.Workplace, SortOrder = 9,
            Title = "Nätdriftsingenjör", TitleEn = "Network Operations Engineer",
            Organization = "Skanova - Telia Sonera", Location = "Jönköping",
            StartDate = D(2007, 3), EndDate = D(2008, 12),
            Comment = "Vikariat.\n\nÖvervakning, felsökning och åtgärdande av problem med Sveriges internetförbindelser via koppar och fiber. Jobbet var ofta utfört ensamt på helger och nätter där man är ansvarig för kommunikation med kunder från hela världen.\n\nSäkerhetsklassad för speciell nätsäkerhet SNS (militär, polis...).",
            CommentEn = "Temporary position.\n\nMonitoring, troubleshooting and fixing problems with Sweden's internet connections over copper and fibre. The work was often done alone on weekends and nights, with responsibility for communicating with customers from all over the world.\n\nSecurity cleared for special network security SNS (military, police...)." },
        new() { Id = 10, Type = CvEntryType.Workplace, SortOrder = 10,
            Title = "Valförrättare/Rösträknare", TitleEn = "Election Officer / Vote Counter",
            Organization = "Jönköpings kommun", OrganizationEn = "Jönköping Municipality", Location = "Jönköping",
            StartDate = D(2002, 9, 15), EndDate = D(2004, 9, 13),
            DisplayDate = "2002-09-15, 2003-09-14, 2004-09-13" },
        new() { Id = 11, Type = CvEntryType.Workplace, SortOrder = 11,
            Title = "Lärare", TitleEn = "Teacher",
            Organization = "Nässjö kommun (Brinellgymnasiet)", OrganizationEn = "Nässjö Municipality (Brinellgymnasiet)", Location = "Nässjö",
            StartDate = D(2002, 8), EndDate = D(2003, 6),
            Courses = "Datorkommunikation\nNätverk A+B",
            CoursesEn = "Computer Communication\nNetworking A+B" },
        new() { Id = 12, Type = CvEntryType.Workplace, SortOrder = 12,
            Title = "IT-admin/IT-samordnare", TitleEn = "IT Administrator / IT Coordinator",
            Organization = "Louis Gibeck AB", Location = "Stockholm",
            StartDate = D(1997, 11), EndDate = D(1999, 4),
            Comment = "Ansvarat för drift av koncernens nätverk, mailsystem och persondatorer.\nPersonalutbildare samt ansvarig för inköp av datautrustning.",
            CommentEn = "Responsible for operating the group's network, email system and personal computers.\nStaff trainer and responsible for purchasing computer equipment." },
        new() { Id = 13, Type = CvEntryType.Workplace, SortOrder = 13,
            Title = "Manager / Säljande konsult / Tekniker", TitleEn = "Manager / Sales Consultant / Technician",
            Organization = "Skymar Datacenter AB", Location = "Jönköping",
            StartDate = D(1995, 10), EndDate = D(1997, 8),
            Comment = "Försäljning, service, reparationer, installation av datorer och nätverk. Började som tekniker och avslutade som manager och säljande konsult.",
            CommentEn = "Sales, service, repairs, and installation of computers and networks. Started as a technician and finished as manager and sales consultant." },
        new() { Id = 14, Type = CvEntryType.Workplace, SortOrder = 14,
            Title = "Optikerassistent", TitleEn = "Optician's Assistant",
            Organization = "Direkt Optik", Location = "Jönköping",
            StartDate = D(1995, 6), EndDate = D(1995, 9),
            Comment = "Vikariat.\n\nAssisterade optikern med försäljning, konsultation och utlämnande av glasögon i butik.",
            CommentEn = "Temporary position.\n\nAssisted the optician with sales, consultations and handing out glasses in the store." },
        new() { Id = 15, Type = CvEntryType.Workplace, SortOrder = 15,
            Title = "Gruppbefäl", TitleEn = "Squad Leader",
            Organization = "Militärtjänst, IB12", OrganizationEn = "Military Service, IB12", Location = "Eksjö",
            StartDate = D(1994, 7), EndDate = D(1995, 5),
            Comment = "Mönstrade som gruppbefäl utbildad i ledarskap och gruppdynamik.",
            CommentEn = "Served as squad leader, trained in leadership and group dynamics." },
        new() { Id = 16, Type = CvEntryType.Workplace, SortOrder = 16,
            Title = "ADB", TitleEn = "Data Processing",
            Organization = "Landstinget, APRIL-projektet", OrganizationEn = "County Council, APRIL Project", Location = "Jönköping",
            StartDate = D(1992, 1), EndDate = D(1994, 12),
            DisplayDate = "1992 – 1994",
            Comment = "Inmatning av enkäter till databaser som sedan användes som stöd i APRIL-projektet.\nExtra hos IT-avdelningen med klientsupport på länssjukhuset samt programmering i Pascal för att hantera utskrifter med streckkoder för patientjournaler.",
            CommentEn = "Entered survey data into databases that were used to support the APRIL project.\nAlso worked with the IT department on client support at the county hospital, and programmed in Pascal to handle printing barcodes for patient records." },

        // ── Schools ────────────────────────────────────────────
        new() { Id = 17, Type = CvEntryType.School, SortOrder = 1,
            Title = "C++ för utvecklare", TitleEn = "C++ for Developers",
            Organization = "YA YrkesAkademin",
            StartDate = D(2026, 10), IsCurrent = true },
        new() { Id = 18, Type = CvEntryType.School, SortOrder = 2,
            Title = "Fullstackutbildning C# .NET", TitleEn = "Full-Stack Development C# .NET",
            Organization = "Lexicon", Location = "Malmö",
            StartDate = D(2026, 8), IsCurrent = true },
        new() { Id = 19, Type = CvEntryType.School, SortOrder = 3,
            Title = "Kockutbildning", TitleEn = "Chef Training",
            Organization = "Arena Utbildning",
            StartDate = D(2025, 12), EndDate = D(2026, 4),
            Courses = "Service och bemötande\nLivsmedelshygien\nMatlagning 1",
            CoursesEn = "Customer Service\nFood Hygiene\nCooking 1" },
        new() { Id = 21, Type = CvEntryType.School, SortOrder = 5,
            Title = "Speldesign - Programmering, 92,5 hp", TitleEn = "Game Design - Programming, 92.5 credits",
            Organization = CampusGotland, OrganizationEn = CampusGotlandEn, Location = "Visby",
            StartDate = D(2010, 8), EndDate = D(2013, 6),
            Courses = "Introduktion till spelutveckling, 15 hp (VG)\nStort spelprojekt, 15 hp\nSpelprogrammering I, 7,5 hp\nMatematik för programmering och spelutveckling, 7,5 hp (VG)\nAvancerad speldesign, 7,5 hp\nLevel Design, 7,5 hp\nIntroduktion till spelanalys och design, 7,5 hp\nProduktionsmetodik för spel, 7,5 hp\nTheme Park, 7,5 hp\nAtt kommunicera i tal och skrift, 7,5 hp",
            CoursesEn = "Introduction to Game Development, 15 credits (with distinction)\nLarge Game Project, 15 credits\nGame Programming I, 7.5 credits\nMathematics for Programming and Game Development, 7.5 credits (with distinction)\nAdvanced Game Design, 7.5 credits\nLevel Design, 7.5 credits\nIntroduction to Game Analysis and Design, 7.5 credits\nProduction Methods for Games, 7.5 credits\nTheme Park, 7.5 credits\nCommunicating in Speech and Writing, 7.5 credits",
            Comment = "92,5 hp avklarade inom speldesign och programmering, ingen examen (examensarbetet ej genomfört).\n\nArbetade extra som elevassistent.",
            CommentEn = "Completed 92.5 credits in game design and programming; no degree (thesis not completed).\n\nWorked extra as a student assistant." },
        new() { Id = 22, Type = CvEntryType.School, SortOrder = 6,
            Title = "Data/IT Systemutveckling – Högskoleexamen 120 hp", TitleEn = "Computer Science / IT Systems Development – Higher Education Diploma, 120 credits",
            Organization = JU, Location = "Jönköping",
            StartDate = D(2003, 8), EndDate = D(2006, 6),
            Comment = "Högskoleexamen (120 hp) utfärdad 2013-12-13, baserad på studier i datateknik och IT-systemutveckling: bl.a. programmering, objektorienterad analys och design, datastrukturer och algoritmer, operativsystem, datanät och datasäkerhet.",
            CommentEn = "Higher Education Diploma (Högskoleexamen, 120 credits) awarded 2013-12-13, based on studies in computer engineering and IT systems development, including programming, object-oriented analysis and design, data structures and algorithms, operating systems, computer networks and IT security." },
        new() { Id = 24, Type = CvEntryType.School, SortOrder = 8,
            Title = "Tekniskt basår", TitleEn = "Technical Foundation Year",
            Organization = JU, Location = "Jönköping",
            StartDate = D(2000, 8), EndDate = D(2001, 6) },
        new() { Id = 25, Type = CvEntryType.School, SortOrder = 9,
            Title = "Högskolebehörighet", TitleEn = "University Entrance Qualification",
            Organization = "Komvux", OrganizationEn = "Komvux (Municipal Adult Education)",
            StartDate = D(1999, 8), EndDate = D(2000, 6) },
        new() { Id = 26, Type = CvEntryType.School, SortOrder = 10,
            Title = "Teknisk linje - inriktning Kemi", TitleEn = "Technology Programme - Chemistry",
            Organization = "Erikdahlbergsgymnasiet", Location = "Jönköping",
            StartDate = D(1990, 8), EndDate = D(1994, 6) },

        // ── Courses ────────────────────────────────────────────
        new() { Id = 27, Type = CvEntryType.Course, SortOrder = 1,
            Title = "Båtförarutbildning (Yachtmaster diploma)", TitleEn = "Boat Handling Training (Yachtmaster diploma)",
            StartDate = D(2024, 6), EndDate = D(2025, 8) },
        new() { Id = 43, Type = CvEntryType.Course, SortOrder = 2,
            Title = "Strukturerad programmering med C++, 7,5 hp", TitleEn = "Structured Programming with C++, 7.5 credits",
            Organization = "Linnéuniversitetet", OrganizationEn = "Linnaeus University",
            StartDate = D(2011, 11, 14), DisplayDate = "2011-11-14" },
        new() { Id = 29, Type = CvEntryType.Course, SortOrder = 3,
            Title = "MC-körkort A (Tung Motorcykel)", TitleEn = "Motorcycle Licence A (Heavy Motorcycle)",
            StartDate = D(2009, 1), EndDate = D(2009, 11) },
        new() { Id = 30, Type = CvEntryType.Course, SortOrder = 4,
            Title = "Kurser inom Mindmapping och support", TitleEn = "Courses in Mind Mapping and Support",
            StartDate = D(2006, 6), EndDate = D(2007, 3) },
        new() { Id = 31, Type = CvEntryType.Course, SortOrder = 5,
            Title = "PYRO - Specialkurs handhavande bomber 4”-8”", TitleEn = "PYRO - Special course in handling 4”-8” shells",
            StartDate = D(1999, 10, 16), DisplayDate = "1999-10-16" },
        new() { Id = 32, Type = CvEntryType.Course, SortOrder = 6,
            Title = "SCALA - Systemunderhåll", TitleEn = "SCALA - System Maintenance",
            StartDate = D(1998, 12, 10), DisplayDate = "1998-12-10" },
        new() { Id = 33, Type = CvEntryType.Course, SortOrder = 7,
            Title = "Administering MS Exchange Server 5,5", TitleEn = "Administering MS Exchange Server 5.5",
            StartDate = D(1998, 2, 24), EndDate = D(1998, 2, 26), DisplayDate = "1998-02-24 – 1998-02-26" },
        new() { Id = 34, Type = CvEntryType.Course, SortOrder = 8,
            Title = "AutoCAD Release 13, 14", Organization = "Autodesk",
            StartDate = D(1997, 1), DisplayDate = "1997",
            Comment = "Autodesk Authorized dealer." },
        new() { Id = 35, Type = CvEntryType.Course, SortOrder = 9,
            Title = "GARP - Order, lager, fakturering, kundreskontra, leverantörsreskontra och redovisning",
            TitleEn = "GARP - Orders, inventory, invoicing, accounts receivable, accounts payable and accounting",
            StartDate = D(1996, 2, 7), DisplayDate = "1996-02-07" },
        new() { Id = 36, Type = CvEntryType.Course, SortOrder = 10,
            Title = "PADI - Open Water Diver - Scuba diving",
            StartDate = D(1994, 11, 13), DisplayDate = "1994-11-13" },

        // ── Boards & committees ────────────────────────────────
        new() { Id = 37, Type = CvEntryType.Board, SortOrder = 1,
            Title = "Vice ordförande", TitleEn = "Vice Chair",
            Organization = "Gotlands studentkår Rindi", OrganizationEn = "Rindi Student Union, Gotland", Location = "Visby",
            StartDate = D(2012, 6), EndDate = D(2013, 6),
            Comment = "Ansvarig för kårhuset, IT, utskott, föreningar och ämnesföreningar på studentkåren.\nAnsvarig för uthyrning av lokaler för publika event såsom Almedalsveckan.\nArrangör 2 år i rad för ett av Sveriges största event för studenter, med ett deltagarantal på ca 650 personer (Absolut Gotland).",
            CommentEn = "Responsible for the student union building, IT, committees, associations and subject societies.\nResponsible for renting out venues for public events such as Almedalen Week.\nOrganiser two years in a row of one of Sweden's largest student events, with about 650 participants (Absolut Gotland)." },
        new() { Id = 40, Type = CvEntryType.Board, SortOrder = 2,
            Title = "Utbildnings- och forskningsnämnden", TitleEn = "Board of Education and Research",
            Organization = CampusGotland, OrganizationEn = CampusGotlandEn, Location = "Visby",
            StartDate = D(2010, 8), EndDate = D(2013, 6) },
        new() { Id = 41, Type = CvEntryType.Board, SortOrder = 3,
            Title = "Utbildningskommittén", TitleEn = "Education Committee",
            Organization = CampusGotland, OrganizationEn = CampusGotlandEn, Location = "Visby",
            StartDate = D(2010, 8), EndDate = D(2013, 6) },
        new() { Id = 42, Type = CvEntryType.Board, SortOrder = 4,
            Title = "Branschrådet", TitleEn = "Industry Advisory Board",
            Organization = JU, Location = "Jönköping",
            StartDate = D(2003, 8), EndDate = D(2006, 6) },
        new() { Id = 38, Type = CvEntryType.Board, SortOrder = 5,
            Title = "Ordförande, Norrlands Nation", TitleEn = "Chair, Norrlands Nation",
            Organization = JU, Location = "Jönköping",
            StartDate = D(2001, 8), EndDate = D(2002, 6) },
        new() { Id = 39, Type = CvEntryType.Board, SortOrder = 6,
            Title = "Ordförande, Qult (studentförening)", TitleEn = "Chair, Qult (student association)",
            Organization = JU, Location = "Jönköping",
            StartDate = D(2001, 8), EndDate = D(2002, 6) },
    };
}
