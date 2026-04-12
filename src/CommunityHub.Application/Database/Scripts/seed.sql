-- ============================================================
--  CommunityHub — početni test podaci
--  Pokrenuti POSLE database.sql kroz pgAdmin
-- ============================================================


-- ============================================================
--  Korisnici
-- ============================================================
INSERT INTO users (id, username, password, name, surname, birthday, role, address) VALUES
(-1, 'marko',  'marko123',  'Marko',  'Marković',   '2000-05-15', 'BuildingManager',      'Bulevar oslobođenja 5, Novi Sad'),
(-2, 'ana',    'ana123',    'Ana',    'Anić',        '1998-08-22', 'BuildingManager',      'Kralja Petra 12, Beograd'),
(-3, 'petar',  'petar123',  'Petar',  'Petrović',   '2001-03-10', 'Tenant',               'Dunavska 15, Novi Sad'),
(-4, 'jovana', 'jovana123', 'Jovana', 'Jovanović',  '1999-11-30', 'Tenant',               'Zmaj Jovina 8, Novi Sad'),
(-5, 'stefan', 'stefan123', 'Stefan', 'Stefanović', '2002-01-25', 'DistrictCoordinator',  'Terazije 22, Beograd'),
(-6, 'milica', 'milica123', 'Milica', 'Milić',      '2000-07-08', 'DistrictCoordinator',  'Knez Mihailova 3, Beograd'),
(-7, 'nikola', 'nikola123', 'Nikola', 'Nikolić',    '1997-12-18', 'Citizen',              'Zemunska 10, Beograd'),
(-8, 'ivana',  'ivana123',  'Ivana',  'Ivanović',   '2001-09-05', 'Citizen',              'Gandijeva 25, Beograd');


-- ============================================================
--  Postovi (legacy primer)
-- ============================================================
INSERT INTO posts (id, title, content, created_at, user_id) VALUES
(-1, 'Moj prvi post', 'Ovo je moj prvi post na platformi!', '2024-01-15 10:30:00', -1),
(-2, 'Zanimljiv dan', 'Danas sam imao vrlo zanimljiv dan na fakultetu.', '2024-01-17 09:15:00', -1),
(-3, 'Programiranje', 'Učim C# i WPF, jako je interesantno!', '2024-01-20 14:30:00', -1),
(-4, 'Dobrodošli', 'Drago mi je što sam ovde.', '2024-01-16 14:20:00', -2),
(-5, 'Vikend planovi', 'Šta planirate za vikend?', '2024-01-19 16:00:00', -2),
(-6, 'Nova knjiga', 'Počela sam da čitam novu knjigu o programiranju.', '2024-01-22 11:00:00', -2),
(-7, 'Nova tema', 'Želim da podelim nešto interesantno o bazama podataka.', '2024-01-18 11:45:00', -3),
(-8, 'SQL je moćan', 'SQL je neverovatno moćan jezik za rad sa podacima.', '2024-01-21 10:00:00', -3),
(-9, 'Postgres', 'Postgres je odličan sistem za upravljanje bazama.', '2024-01-23 15:30:00', -3),
(-10, 'Učenje programiranja', 'Programiranje je fascinantno!', '2024-01-20 08:30:00', -4),
(-11, 'Moj projekat', 'Radim na zanimljivom projektu.', '2024-01-22 13:15:00', -4),
(-12, 'Pozdrav svima', 'Zdravo svima iz zajednice!', '2024-01-21 12:00:00', -5),
(-13, 'Sport', 'Volim da trčim ujutru pre posla.', '2024-01-23 07:00:00', -5),
(-14, 'Muzika', 'Slušam jazz muziku dok programiram.', '2024-01-24 18:30:00', -5),
(-15, 'Moje iskustvo', 'Delim svoje iskustvo sa bazama podataka.', '2024-01-22 15:30:00', -6),
(-16, 'Kafa', 'Najbolja kafa je ujutru!', '2024-01-24 08:00:00', -6),
(-17, 'Fudbal', 'Volim da gledam fudbal vikendom.', '2024-01-21 16:00:00', -7),
(-18, 'Putovanje', 'Planiram putovanje na more.', '2024-01-24 12:00:00', -7),
(-19, 'Film', 'Pogledala sam odličan film sinoć.', '2024-01-22 20:00:00', -8),
(-20, 'Trening', 'Redovno treniram u teretani.', '2024-01-25 06:30:00', -8);


-- ============================================================
--  Lokacije
-- ============================================================
INSERT INTO locations (id, city, country) VALUES
(-1, 'Beograd', 'Srbija'),
(-2, 'Novi Sad', 'Srbija');


-- ============================================================
--  Kvartovi (za koordinatora kvarta — funkcionalnosti 3.x i 4.x)
-- ============================================================
INSERT INTO districts (id, name, description, location_id, budget, coordinator_id) VALUES
(-1, 'Zemun',        'Kvart Zemun u Beogradu',        -1, 0, -5),
(-2, 'Novi Beograd', 'Kvart Novi Beograd u Beogradu', -1, 0, -5);

-- Ulice u kvartovima
INSERT INTO district_streets (id, district_id, street_name, start_number, end_number) VALUES
(-1, -1, 'Zemunska',      1, 50),
(-2, -1, 'Glavna',        1, 30),
(-3, -2, 'Gandijeva',     1, 80),
(-4, -2, 'Bulevar Zorana Đinđića', 1, 120);

-- Slike kvartova
INSERT INTO images (id, url, district_id) VALUES
(-1, 'images/zemun_1.jpg', -1),
(-2, 'images/novi_beograd_1.jpg', -2);

-- Zahtevi za pristup kvartovima (test podaci za 3.2)
INSERT INTO district_access_requests (id, user_id, district_id, creation_date, status, rejection_reason) VALUES
(-1, -7, -1, '2025-03-10 14:30:00', 'Pending', NULL),
(-2, -8, -1, '2025-03-11 09:00:00', 'Pending', NULL),
(-3, -8, -2, '2025-03-12 16:45:00', 'Approved', NULL),
(-4, -7, -2, '2025-03-08 11:20:00', 'Rejected', 'Adresa nije u opsegu ulica kvarta.');

-- Članstva u kvartovima (za odobrene zahteve)
INSERT INTO district_memberships (id, district_id, user_id, join_date) VALUES
(-1, -2, -8, '2025-03-12 16:45:00');
