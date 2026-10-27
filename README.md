# lundqwist.se — Personal Site

The personal website of **Bjarne M. Lundqwist**: CV, stories, projects, gaming
and an About page, in English (the CV also in Swedish), with an admin area for
editing all content.

Built with **C# / ASP.NET Core 8 MVC**, **Entity Framework Core** and **MariaDB**
(running on a Synology DS220+).

---

## Copyright

**© 2026 Bjarne M. Lundqwist. All rights reserved.**

All content on this website is the property of Bjarne M. Lundqwist and is
protected by copyright. This includes, but is not limited to:

- all texts (CV, stories, projects, game pages and About page),
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
| Playfair Display, Inter (Google Fonts) | Typefaces, loaded from Google's servers (see `/Privacy`) | SIL Open Font License 1.1 |
| QuestPDF | Building the CV PDF | QuestPDF Community License |
| Lato (`Fonts/`) | Typeface in the CV PDF | SIL Open Font License 1.1 |

World of Warcraft, No Man's Sky, Terraria and Minecraft are trademarks of their
respective owners. The illustrations on the game pages are original artwork,
not official game art.

The full license texts are in each package and in `wwwroot/lib/*/LICENSE*`.

---

## What the site does

The pages, in menu order:

| Page | Address | Content |
|---|---|---|
| Home | `/` | Welcome text, portrait and links to all sections |
| Stories | `/Stories` | Texts and thoughts |
| Projects | `/Projects` | Side projects and experiments |
| Gaming | `/Gaming` | Only cards: one per game (each with its own page) and one for the Discord server |
| About | `/About` | About text, with a "View my CV" button |
| CV | `/CV` | Jobs, courses, education and board roles in English or Swedish, with a PDF download (`/CV/Pdf?lang=en` or `sv`); notes and courses taught appear when you hover over a card |
| Admin | `/Admin` | Log in to add and edit all of the above (menu link shown only when logged in) |
| Privacy & cookies | `/Privacy` | Which cookies the site uses, and why (linked from the footer and the cookie notice) |
| Showcase | `/showcase/…` | Your own files and small static websites, to link to from posts (see *The showcase folder*) |

**Layout.** Every page has the same frame: the menu bar with the Lundqwist logo
(its letters sit on the bar's bottom line, the q's tail hangs below it) and the
footer with contact details. On phones the menu becomes two rows: the logo,
then the links.

A single story, project or game page shows two cards: a title card (date — not
on game pages —, title and summary) and a text card. The About page shows its
text in the same kind of card.

**Languages.** The site is in English. Only the CV has an **English | Svenska**
switch; the choice is in the address (`/CV?lang=sv`), so no cookie is needed.

**Discord.** The Discord card on the Gaming page links to the invite in
`appsettings.json` → `Discord:InviteUrl`. While that is empty, the card shows
"Invite link coming soon".

**Cookies.** Visitors get no cookies. The only cookies are the admin login,
form protection and confirmation messages, which only affect the logged-in
admin. There is no tracking or analytics, so no consent is needed. A
small notice at the bottom of the page says so and links to `/Privacy`; clicking
OK hides it for good (remembered in the browser's localStorage, not a cookie).
If you add a cookie, update the table in `Views/Home/Privacy.cshtml`.

**Admin.** After logging in you can add, edit and delete CV entries, stories,
projects and game pages, and edit the About page. Text is written in
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
| Security: antiforgery, XSS, open redirects | `Program.cs`, `Services/MarkdownRenderer.cs` |
| Partial views (reused HTML pieces) | `Views/Shared/_CookieNotice.cshtml`, `_Footer.cshtml` |
| Serving an extra folder of files | The *showcase* block in `Program.cs` |
| CSS custom properties ("tokens") | The top of `wwwroot/css/site.css`: cards re-point the shared colors |

---

## Project structure

```
PersonalSite/
├── Program.cs                   Start-up: services, middleware, routes
├── appsettings.json             Settings, incl. the database connection string
├── setup_db.sql                 Creates the database and its user on the NAS
├── Models/                      Data classes (one per database table)
├── Data/
│   ├── AppDbContext.cs          EF Core: tables and relationships
│   ├── CvSeedData.cs            The original CV content (used once by migrations)
│   └── Bjarne_Lundqwist_CV.pdf  The CV document
├── Services/
│   ├── MarkdownRenderer.cs      Markdown → safe HTML
│   └── CvPdfBuilder.cs          The CV as a PDF
├── Fonts/                       Lato, for the CV PDF
├── Controllers/                 Public pages (Home + Privacy, CV + PDF, Stories/Projects/
│                                Gaming, About, image serving)
├── Views/                       Public HTML templates (Razor .cshtml)
│   └── Shared/                  Layout, footer, post list/details, language switch,
│                                cookie notice and other partials
├── Areas/Admin/                 Admin area (login required)
│   ├── Controllers/             Account, Dashboard, Cv, Posts, About, Media
│   ├── Models/                  Form models (login, change password, image list)
│   └── Views/                   Admin pages and the editor parts
├── Migrations/                  Database structure changes, applied at start-up
├── deploy/
│   ├── docker-compose.yml       The container definition used on the NAS
│   └── .env.example             Template for the secret .env file
├── deploy.ps1                   Publishes the site and copies it to the NAS
└── wwwroot/                     Files sent to the browser as they are
    ├── css/site.css             All styling
    ├── js/site.js               Copy button (footer) and cookie notice
    ├── js/admin-editor.js       Editor: toolbar, preview, image upload
    ├── images/                  Logo, portrait and page images
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
| Stories, projects, game pages | Admin → Stories / Projects / Gaming |
| Discord invite link | `Discord:InviteUrl` in `appsettings.json`, then deploy |
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

Files on the NAS, in `/volume1/docker/personalsite/` (`\\LUNARBASE\docker\personalsite`, or `Z:\personalsite` with the mapped drive):

| File / folder | Content |
|---|---|
| `docker-compose.yml` | Container definition (copy of `deploy/docker-compose.yml`) |
| `.env` | The database connection string — secret, only on the NAS |
| `app/` | The published site (replaced on every deploy) |
| `keys/` | Encryption keys for the login cookie (created automatically) |
| `showcase/` | Your own files and websites, served at `/showcase/` (never touched by deploys) |

> The site is **not** placed in the `web` share: Web Station publishes that
> folder as it is, so the site's settings would be downloadable. `web` only
> holds an `index.html` that sends http visitors to https.

### Updating the site
1. Run `.\deploy.ps1` in the project folder (publishes and copies to the NAS).
   It deletes each file before copying its new version, so the running site
   keeps working on its old copy until the restart. (Overwriting the files in
   place while the site runs makes it crash with empty "500" errors.)
2. DSM → Container Manager → Project → *personalsite* → Action → **Restart**.
3. Reload the page with **Ctrl+F5** to skip the browser's cache.

Database migrations run automatically when the site starts.

- **CSS, JavaScript or image changes only** (`wwwroot/`) usually show without a
  restart, because those files are read straight from disk.
- **Changed `docker-compose.yml`** (volumes, environment variables): a Restart
  is not enough. Stop the project, then **Build** it so the container is
  recreated with the new settings.

### The showcase folder
A folder on the NAS for files and small websites that you want to show or link
to, for example a game you made or a downloadable PDF:

```
\\LUNARBASE\docker\personalsite\showcase\
├── README.txt            short how-to
├── files\                downloads: PDFs, zips, images, builds …
│     cv.pdf            → https://lundqwist.se/showcase/files/cv.pdf
└── sites\                static websites, one folder each with an index.html
      my-game\          → https://lundqwist.se/showcase/sites/my-game/
```

- New files show up immediately; no deploy or restart needed.
- Link to them from a post: put the full address in the *Website address* /
  *Link* field, or in the text: `[Play the game](https://lundqwist.se/showcase/sites/my-game/)`.
- Use lowercase names without spaces, and relative links inside a site
  (`css/style.css`, not `/css/style.css`).
- Only static files work (HTML, CSS, JavaScript, images); no PHP or databases.
  Everything in the folder is public.
- **Sandbox.** HTML pages there are served with a `Content-Security-Policy: sandbox`
  header, so their scripts can never act as the logged-in admin. Side effect:
  they can't use cookies or localStorage. To allow that for code you fully
  trust, add `Showcase__Sandbox=false` to the environment in `docker-compose.yml`.
- How it works: `Program.cs` serves the folder (setting `Showcase:Path`), and
  `docker-compose.yml` mounts it read-only into the container at `/showcase`.
  `deploy.ps1` creates the folder if it is missing. When running locally
  `Showcase:Path` is not set, so `/showcase` doesn't exist there.

### Passwords
- Locally, the connection string is stored in .NET **user secrets**
  (`dotnet user-secrets list`), not in `appsettings.json`.
- On the NAS it is in `.env` (see `deploy/.env.example`).

### Notes
- **Never open the database port (3306) on the router.** The site connects to
  the database over the local network.
- **Image size limit.** MariaDB's default `max_allowed_packet` is 1 MB, so
  images are kept under 700 KB. Raising it on the NAS allows larger images.
  

---

© 2026 Bjarne M. Lundqwist. All rights reserved.
