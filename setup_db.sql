-- Run this once on the Synology NAS (MariaDB 10 package), e.g. via SSH:
--   /usr/local/mariadb10/bin/mysql -u root -p < setup_db.sql
-- or paste it into phpMyAdmin's SQL tab.
--
-- Synology's root account can only log in from the NAS itself, so the app
-- uses a dedicated user that is allowed to connect from the local network.
-- Replace CHANGE_ME with a strong password and use the same one in
-- appsettings.json.

CREATE DATABASE IF NOT EXISTS personal_site
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- Allow connections from the LAN (192.168.50.x) and from the NAS itself
CREATE USER IF NOT EXISTS 'siteuser'@'192.168.50.%' IDENTIFIED BY 'CHANGE_ME';
CREATE USER IF NOT EXISTS 'siteuser'@'localhost'    IDENTIFIED BY 'CHANGE_ME';
GRANT ALL PRIVILEGES ON personal_site.* TO 'siteuser'@'192.168.50.%';
GRANT ALL PRIVILEGES ON personal_site.* TO 'siteuser'@'localhost';
FLUSH PRIVILEGES;

-- EF Core migrations will create the tables automatically on first run.
