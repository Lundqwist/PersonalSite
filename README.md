# lundqwist.se — Personal Site

The personal website of **Bjarne M. Lundqwist**: CV, stories, projects, portfolio
and an About page, in Swedish and English, with an admin area for editing all
content.

Built with **C# / ASP.NET Core 8 MVC**, **Entity Framework Core** and **MariaDB**
(running on a Synology DS220+).

---

## Copyright

**© 2026 Bjarne M. Lundqwist. All rights reserved.**

All content on this website is the property of Bjarne M. Lundqwist and is
protected by copyright. This includes, but is not limited to:

- all texts (CV, stories, projects, portfolio and About page),
- all photographs and illustrations, including the images in `wwwroot/images/`
  and every image uploaded through the admin pages,
- the CV document in `Data/` and the personal and contact information,
- the visual design of the site.

No part of this content may be copied, reproduced, republished, distributed,
modified or used for any other purpose, including training of AI models,
without prior written permission from the copyright holder.

The source code in this repository is likewise © Bjarne M. Lundqwist, all rights
reserved, unless otherwise stated below. Viewing the code for learning purposes
is welcome; reuse requires permission.

For permission requests, contact **bjarne@lundqwist.se**.

### Third-party components

The site uses the following open-source software, each under its own license.
Their copyright remains with their respective authors.

| Component | Used for | License |
|---|---|---|
| ASP.NET Core, Entity Framework Core (Microsoft) | Web framework, database access | MIT |
| Pomelo.EntityFrameworkCore.MySql | MySQL/MariaDB support for EF Core | MIT |
| Markdig | Markdown → HTML | BSD-2-Clause |
| HtmlSanitizer (with AngleSharp) | Cleaning HTML for safety | MIT |
| Bootstrap Icons 1.11.3 | Footer icons (email, LinkedIn, Discord, location) | MIT |
| Bootstrap, jQuery, jQuery Validation (`wwwroot/lib/`) | Included by the project template | MIT |
| Playfair Display, Inter (Google Fonts) | Typefaces | SIL Open Font License 1.1 |

The full license texts are in each package and in `wwwroot/lib/*/LICENSE*`.

---

## What the site does

| Page | Address | Content |
|---|---|---|
| Home | `/` | Welcome text, portrait and links to all sections |
| CV | `/CV` | Jobs, courses, education and board roles; notes and courses taught appear when you hover over a card |
| Stories | `/Stories` | Texts, each written in Swedish (SE) or English (EN) |
| Projects | `/Projects` | Side projects, each SE or EN |
| Portfolio | `/Portfolio` | Websites and work, with a "Visit site" link |
| About | `/About` | About text in English, with optional Swedish version |
| Admin | `/Admin` | Log in to add and edit all of the above |

**Languages.** Swedish is the default. The CV and About pages have a
*Svenska | English* switch; stories, projects and portfolio items are written in
one language each and show an **SE** or **EN** tag.

**Admin.** After logging in you can add, edit and delete CV entries, stories,
projects and portfolio items, and edit the About page. Text is written in
**Markdown** with a formatting toolbar and preview. Images are uploaded in the
editor; they are resized in the browser (max 1600 px), converted to WebP,
stripped of hidden metadata such as GPS position, and stored in the database.

---

## How it works — a guide for learning the code

Every source file has comments explaining what it does. A good reading order:

1. **`Program.cs`** — the starting point: services, login, language, routes.
2. **`Models/`** — the data: `CvEntry`, `Post`, `PageContent`, `MediaImage`, `AdminUser`.
3. **`Data/AppDbContext.cs`** — how the models map to database tables.
4. **`Controllers/`** — the public pages. Start with `HomeController`, then
   `CvController` and `PostCategoryController`.
5. **`Views/`** — the HTML templates (Razor). Start with `Views/Shared/_Layout.cshtml`,
   which explains the Razor syntax.
6. **`Areas/Admin/`** — the admin pages: controllers, view models and views.
7. **`wwwroot/js/admin-editor.js`** — the JavaScript for the editor.
8. **`wwwroot/css/site.css`** — the styling; the top of the file explains the CSS used.

### What happens when someone visits a page

```
Browser asks for /Stories
  │
  ▼
Program.cs middleware:  static files? → language → routing → login check
  │
  ▼
StoriesController.Index()            (Controllers/PostCategoryController.cs)
  │  asks the database for published stories (EF Core → SQL → MariaDB)
  ▼
View("PostList", posts)              (Views/Shared/PostList.cshtml)
  │  Razor turns the C# data into HTML, inside _Layout.cshtml
  ▼
HTML sent back to the browser, which loads site.css, site.js and images
```

### Key concepts

| Concept | Where to see it |
|---|---|
| MVC (Model–View–Controller) | `Models/`, `Views/`, `Controllers/` |
| Dependency injection | `Program.cs` (registration), controller constructors (use) |
| `async` / `await` | Every controller action that uses the database |
| LINQ queries | `_db.Posts.Where(...).OrderBy(...)` in the controllers |
| Migrations | `Migrations/` — the header of `InitialCreate` explains them |
| Razor views and tag helpers | `Views/Shared/_Layout.cshtml` |
| Areas | `Areas/Admin/` |
| Authentication (cookie login) | `Program.cs`, `Areas/Admin/Controllers/AccountController.cs` |
| Security: antiforgery, XSS, open redirects | `Program.cs`, `Services/MarkdownRenderer.cs`, `Controllers/LanguageController.cs` |

---

## Project structure

```
PersonalSite/
├── Program.cs                   Start-up: services, middleware, routes
├── Lang.cs                      Helper for Swedish/English texts
├── appsettings.json             Settings, incl. the database connection string
├── setup_db.sql                 Creates the database and its user on the NAS
├── Models/                      Data classes (one per database table)
├── Data/
│   ├── AppDbContext.cs          EF Core: tables and relationships
│   ├── CvSeedData.cs            The original CV content (used once by migrations)
│   └── Bjarne_Lundqwist_CV.pdf  The CV document
├── Services/
│   └── MarkdownRenderer.cs      Markdown → safe HTML
├── Controllers/                 Public pages (Home, CV, Stories/Projects/Portfolio,
│                                About, Language switch, image serving)
├── Views/                       Public HTML templates (Razor .cshtml)
│   └── Shared/                  Layout, footer, post list/details, partials
├── Areas/Admin/                 Admin area (login required)
│   ├── Controllers/             Account, Dashboard, Cv, Posts, About, Media
│   ├── Models/                  Form models (login, change password, image list)
│   └── Views/                   Admin pages and the editor parts
├── Migrations/                  Database structure changes, applied at start-up
└── wwwroot/                     Files sent to the browser as they are
    ├── css/site.css             All styling
    ├── js/site.js               Copy button (footer)
    ├── js/admin-editor.js       Editor: toolbar, preview, image upload
    ├── images/                  Portrait and page images
    └── lib/                     Third-party libraries from the template
```

---

## Setup

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- MariaDB 10.6+ or MySQL 8+ (here: the MariaDB 10 package on a Synology DS220+)

### 2. Database
Edit the password in `setup_db.sql`, then run it on the NAS, either in
phpMyAdmin's SQL tab or over SSH:

```bash
/usr/local/mariadb10/bin/mysql -u root -p < setup_db.sql
```

It creates the database `personal_site` and the user `siteuser`, allowed to
connect from the local network (192.168.50.x) and from the NAS itself.

### 3. Connection string
The connection string (with the password) is kept out of the code. On your
own computer it is stored in .NET **user secrets**:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=192.168.50.2;Port=3306;Database=personal_site;User=siteuser;Password=...;"
```

User secrets are stored in your Windows user profile, not in the project
folder, and are only used when the site runs in Development mode
(`dotnet run`). On the NAS the connection string comes from the `.env` file
(see *Publishing the site*).

### 4. Run
```bash
dotnet run
```
Open http://localhost:5000. On start-up the site automatically applies any
pending database migrations, so the tables are created on the first run.

### 5. Admin
On a new installation, go to **/Admin** and log in with username `lunarAdmin`
and the first-login password `ChangeMe-OnFirstLogin!`.
**Change the password right away** with *Change password* in the admin
header. Only a salted hash of the password is stored.

---

## Editing content

Everything is edited in the admin area — no code changes needed:

| What | Where |
|---|---|
| CV entries | Admin → CV |
| Stories, projects, portfolio | Admin → Stories / Projects / Portfolio |
| About page | Admin → About |
| Images | The *Images* section of each editor |

`Data/CvSeedData.cs` only held the original CV content; changing it does not
change the live CV.

### Formatting (Markdown)

| Type | Result |
|---|---|
| `**bold**` | **bold** |
| `*italic*` | *italic* |
| `## Heading` | a heading |
| `- item` | bullet list |
| `1. item` | numbered list |
| `> quote` | quote |
| `[text](https://…)` | link |
| `![description](/media/12)` | image (use *Insert* in the image library) |

---

## Changing the database structure

The database structure is defined by the classes in `Models/` and by the
migrations in `Migrations/`. To add a column or table:

1. Change the model class (e.g. add a property to `Post`).
2. Create a migration:
   ```bash
   dotnet tool install --global dotnet-ef   # once
   dotnet ef migrations add DescribeTheChange
   ```
3. Run the site; the migration is applied at start-up.

Never edit a migration that has already been applied to the live database —
add a new one instead.

---

## Publishing the site (Synology DS220+)

The site runs in **Container Manager** (Docker) on the NAS, using Microsoft's
official .NET 8 runtime image. DSM's **reverse proxy** receives visitors on
https://lundqwist.se and passes them to the site.

```
Visitor ──https──► DSM reverse proxy (lundqwist.se:443, Let's Encrypt)
                        │ http
                        ▼
                  container "personalsite"  (localhost:8090)
                        │
                        ▼
                  MariaDB 10 package (192.168.50.2:3306)
```

Files on the NAS, in `/volume1/docker/personalsite/` (`\\192.168.50.2\docker\personalsite`):

| File / folder | Content |
|---|---|
| `docker-compose.yml` | Container definition (copy of `deploy/docker-compose.yml`) |
| `.env` | The database connection string — secret, only on the NAS |
| `app/` | The published site |
| `keys/` | Encryption keys for the login cookie (created automatically) |

> The site is **not** placed in the `web` share: Web Station publishes that
> folder as it is, so the site's settings would be downloadable. `web` only
> holds an `index.html` that sends http visitors to https.

### Updating the site
1. Run `.\deploy.ps1` in the project folder (publishes and copies to the NAS).
2. DSM → Container Manager → Project → *personalsite* → Action → **Restart**.

Database migrations run automatically when the site starts.

### Passwords
- Locally, the connection string is stored in .NET **user secrets**
  (`dotnet user-secrets list`), not in `appsettings.json`.
- On the NAS it is in `.env` (see `deploy/.env.example`).

### Notes
- **Never open the database port (3306) on the router.** The site connects to
  the database over the local network.
- **Image size limit.** MariaDB's default `max_allowed_packet` is 1 MB, so
  images are kept under 700 KB. Raising it on the NAS allows larger images.
- The old project in `docker/lundqwist` also uses port 3306 for its own
  MariaDB container; don't start it while the MariaDB package is running.

---

© 2026 Bjarne M. Lundqwist. All rights reserved.
