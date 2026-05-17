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
(-8, 'ivana',  'ivana123',  'Ivana',  'Ivanović',   '2001-09-05', 'Citizen',              'Gandijeva 25, Beograd'),
(-9, 'boban',  'boban123',  'Boban',  'Bobanović',  '1999-04-10', 'Citizen',              'Zemunska 22, Beograd'),
(-10, 'jelena', 'jelena123', 'Jelena', 'Petrović',   '1996-02-11', 'Citizen',             'Gandijeva 14, Beograd'),
(-11, 'milan',  'milan123',  'Milan',  'Jovanović',  '1995-10-01', 'Citizen',             'Gandijeva 40, Beograd'),
(-14, 'teodora','teodora123','Teodora','Ristić',     '1999-01-07', 'Citizen',             'Gandijeva 62, Beograd'),
(-17, 'uros',   'uros123',   'Uroš',   'Živković',   '1994-09-28', 'Citizen',             'Bulevar Zorana Đinđića 110, Beograd'),
(-19, 'aleksa', 'aleksa123', 'Aleksa', 'Milošević',  '1996-08-08', 'Citizen',             'Gandijeva 49, Beograd'),
(-21, 'igor',   'igor123',   'Igor',   'Babić',      '1993-04-26', 'Citizen',             'Gandijeva 12, Beograd'),
(-22, 'nina',   'nina123',   'Nina',   'Lazić',      '2000-12-05', 'Citizen',             'Glavna 9, Beograd'),
(-23, 'filip',  'filip123',  'Filip',  'Tadić',      '1998-01-22', 'Citizen',             'Zemunska 28, Beograd');


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

-- Članstva u kvartovima (bogatiji podaci za 4.4)
INSERT INTO district_memberships (id, district_id, user_id, join_date, trust_level) VALUES
(-1,  -2, -8,  '2026-02-12 16:45:00', 'Nov'),
(-10, -2, -10, '2024-06-01 09:00:00', 'Nov'),
(-11, -2, -11, '2023-02-10 09:00:00', 'Nov'),
(-14, -2, -14, '2022-10-21 09:00:00', 'Nov'),
(-17, -2, -17, '2021-05-09 09:00:00', 'Nov'),
(-19, -2, -19, '2023-12-05 09:00:00', 'Nov'),
(-21, -2, -21, '2022-01-12 09:00:00', 'Nov'),
(-22, -1, -22, '2024-03-14 09:00:00', 'Nov'),
(-23, -1, -23, '2023-08-01 09:00:00', 'Nov');

-- Dogadjaji gradjana (4.3)
INSERT INTO district_events (id, district_id, organizer_id, name, description, event_date, start_time, duration_minutes, min_volunteers, status, created_at) VALUES
(-1, -1, -7, 'Ciscenje parka', 'Akcija ciscenja centralnog parka u kvartu.', '2026-06-20', '09:00', 180, 2, 'UPripremi', '2026-06-01 10:00:00'),
(-2, -2, -8, 'Uredjenje igralista', 'Farbanje klupa i sredjivanje igralista.', '2026-06-22', '10:00', 120, 2, 'Zakazan', '2026-06-02 12:00:00'),
(-10, -2, -10, 'Prolecno ciscenje bloka', 'Ciscenje oko zgrada i parkinga.', '2026-03-01', '09:00', 120, 3, 'Zavrsen', '2026-02-20 12:00:00'),
(-11, -2, -11, 'Sadnja drveca', 'Sadnja mladica u dvoristu kvartovske skole.', '2026-03-09', '11:00', 180, 4, 'Zavrsen', '2026-02-28 13:00:00'),
(-12, -2, -11, 'Uredjenje igralista 2', 'Popravka klupa i kanti za otpatke.', '2026-03-18', '10:00', 150, 3, 'Zavrsen', '2026-03-05 14:00:00'),
(-13, -2, -14, 'Akcija reciklaze', 'Prikupljanje i sortiranje reciklaznog otpada.', '2026-03-24', '09:30', 120, 4, 'Zavrsen', '2026-03-10 09:30:00'),
(-14, -2, -17, 'Bojenje parkovske ograde', 'Uredjenje ograde i zamena ostecenih delova.', '2026-03-27', '08:00', 180, 4, 'Zavrsen', '2026-03-12 08:15:00'),
(-15, -2, -19, 'Komsijski sajam knjiga', 'Razmena knjiga i uredjenje stajalista.', '2026-04-02', '12:00', 120, 2, 'Zavrsen', '2026-03-20 16:00:00'),
(-16, -2, -21, 'Ciscenje setalista', 'Sredjivanje setalista uz glavnu saobracajnicu.', '2026-04-08', '09:00', 120, 3, 'Zavrsen', '2026-03-28 10:00:00'),
(-17, -2, -17, 'Kosenje i uredjenje zelenila', 'Uredjenje travnjaka i zelenih povrsina.', '2026-04-15', '08:30', 180, 4, 'Zavrsen', '2026-04-01 10:00:00'),
(-18, -1, -22, 'Ciscenje zemunskog parka', 'Akcija ciscenja i uredjenja klupa.', '2026-03-22', '09:00', 120, 3, 'Zavrsen', '2026-03-10 12:00:00');

INSERT INTO district_event_required_items (id, event_id, item_name, assigned_volunteer_id) VALUES
(-1, -1, 'Rukavice', -7),
(-2, -1, 'Vrece za smece', NULL),
(-3, -2, 'Farba', -8),
(-4, -2, 'Cetke', -9),
(-10, -10, 'Vrece za smece', NULL),
(-11, -10, 'Rukavice', -10),
(-12, -11, 'Lopate', -14),
(-13, -11, 'Rukavice', -11),
(-14, -12, 'Farba', -19),
(-15, -13, 'Kese', -14),
(-16, -14, 'Cetke', -19),
(-17, -15, 'Stolovi', -21),
(-18, -16, 'Metle', -10),
(-19, -17, 'Trimer', -11),
(-20, -18, 'Rukavice', -23);

INSERT INTO district_event_applications (id, event_id, citizen_id, applied_at) VALUES
(-1, -1, -7, '2026-06-05 09:30:00'),
(-2, -2, -8, '2026-06-06 08:00:00'),
(-3, -2, -9, '2026-06-06 08:10:00'),
(-10, -10, -10, '2026-02-25 09:00:00'),
(-11, -10, -11, '2026-02-25 09:10:00'),
(-12, -11, -14, '2026-03-02 08:50:00'),
(-13, -11, -10, '2026-03-02 09:05:00'),
(-14, -12, -11, '2026-03-11 10:00:00'),
(-15, -13, -19, '2026-03-16 09:40:00'),
(-16, -14, -19, '2026-03-20 08:30:00'),
(-17, -15, -21, '2026-03-25 12:20:00'),
(-18, -16, -10, '2026-04-01 09:00:00'),
(-19, -17, -11, '2026-04-06 08:00:00'),
(-20, -18, -23, '2026-03-14 09:00:00');

INSERT INTO district_event_application_items (id, application_id, required_item_id) VALUES
(-1, -1, -1),
(-2, -2, -3),
(-3, -3, -4),
(-10, -10, -10),
(-11, -11, -11),
(-12, -12, -12),
(-13, -13, -13),
(-14, -14, -14),
(-15, -15, -15),
(-16, -16, -16),
(-17, -17, -17),
(-18, -18, -18),
(-19, -19, -19),
(-20, -20, -20);

-- Potvrdjena volontiranja (4.4) za raznolikost nivoa poverenja
INSERT INTO district_event_volunteer_attendances (id, event_id, volunteer_id, attended, marked_at) VALUES
(-1, -10, -10, TRUE,  '2026-03-01 12:30:00'),
(-2, -10, -11, TRUE,  '2026-03-01 12:30:00'),
(-3, -11, -14, TRUE,  '2026-03-09 14:30:00'),
(-4, -11, -10, TRUE,  '2026-03-09 14:30:00'),
(-5, -12, -11, TRUE,  '2026-03-18 13:20:00'),
(-6, -13, -19, TRUE,  '2026-03-24 12:10:00'),
(-7, -14, -19, TRUE,  '2026-03-27 11:50:00'),
(-8, -15, -21, TRUE,  '2026-04-02 14:05:00'),
(-9, -16, -10, TRUE,  '2026-04-08 11:35:00'),
(-10, -17, -11, TRUE, '2026-04-15 13:10:00'),
(-11, -18, -23, TRUE, '2026-03-22 11:00:00'),
(-12, -14, -10, TRUE, '2026-03-27 11:55:00'),
(-13, -17, -10, TRUE, '2026-04-15 13:12:00'),
(-14, -11, -11, TRUE, '2026-03-09 14:32:00'),
(-15, -12, -19, TRUE, '2026-03-18 13:25:00'),
(-16, -15, -19, TRUE, '2026-04-02 14:06:00'),
(-17, -16, -19, TRUE, '2026-04-08 11:37:00'),
(-18, -13, -14, TRUE, '2026-03-24 12:20:00'),
(-19, -14, -14, TRUE, '2026-03-27 11:58:00'),
(-20, -15, -10, TRUE, '2026-04-02 14:10:00'),
(-21, -16, -14, TRUE, '2026-04-08 11:40:00'),
(-22, -17, -14, TRUE, '2026-04-15 13:15:00'),
(-23, -10, -21, TRUE, '2026-03-01 12:32:00'),
(-24, -12, -21, TRUE, '2026-03-18 13:30:00'),
(-25, -14, -21, TRUE, '2026-03-27 12:00:00'),
(-26, -15, -11, TRUE, '2026-04-02 14:15:00'),
(-27, -16, -11, TRUE, '2026-04-08 11:42:00'),
(-28, -17, -19, TRUE, '2026-04-15 13:18:00'),
(-29, -13, -10, TRUE, '2026-03-24 12:22:00'),
(-30, -14, -11, TRUE, '2026-03-27 12:03:00'),
(-31, -17, -21, FALSE,'2026-04-15 13:20:00'),
(-32, -18, -19, TRUE, '2026-04-02 14:20:00'),
(-33, -16, -21, TRUE, '2026-04-08 11:45:00'),
(-34, -18, -21, TRUE, '2026-04-15 13:23:00'),
(-35, -10, -19, TRUE, '2026-03-01 12:40:00'),
(-36, -11, -19, TRUE, '2026-03-09 14:40:00'),
(-37, -18, -10, TRUE, '2026-03-18 13:35:00'),
(-38, -18, -11, TRUE, '2026-03-24 12:25:00'),
(-39, -18, -14, TRUE, '2026-03-27 12:05:00'),
(-40, -18, -17, TRUE, '2026-04-02 14:25:00'),
(-41, -18, -8, TRUE, '2026-04-08 11:48:00'),
(-42, -18, -9, TRUE, '2026-04-15 13:27:00'),
(-43, -10, -14, TRUE, '2026-03-01 12:45:00'),
(-44, -11, -21, TRUE, '2026-03-09 14:45:00'),
(-45, -12, -22, TRUE, '2026-03-18 13:40:00'),
(-46, -13, -21, TRUE, '2026-03-24 12:30:00'),
(-47, -14, -22, TRUE, '2026-03-27 12:07:00'),
(-48, -15, -22, TRUE, '2026-04-02 14:30:00');


-- ============================================================
--  Adrese
-- ============================================================
INSERT INTO addresses (id, street, street_number) VALUES
(-1, 'Glavna', '15'),
(-2, 'Bulevar Mihajla Pupina', '22');


-- ============================================================
--  Zgrade
-- ============================================================
INSERT INTO buildings (id, address_id, neighborhood, location_id, number_of_floors, manager_id) VALUES
(-1, -1, 'Zemun',        -1, 3, -1),
(-2, -2, 'Novi Beograd', -1, 2, -2);


-- ============================================================
--  Spratovi i stanovi
-- ============================================================
-- Prva zgrada (3 sprata)
INSERT INTO floors (id, building_id, floor_number) VALUES
(-1, -1, 1),
(-2, -1, 2),
(-3, -1, 3);

-- Druga zgrada (2 sprata)
INSERT INTO floors (id, building_id, floor_number) VALUES
(-4, -2, 1),
(-5, -2, 2);

-- Stanovi za prvu zgradu
INSERT INTO apartments (id, floor_id, apartment_number) VALUES
(-1, -1, 1), (-2, -1, 2), (-3, -1, 3),
(-4, -2, 4), (-5, -2, 5), (-6, -2, 6),
(-7, -3, 7), (-8, -3, 8);

-- Stanovi za drugu zgradu
INSERT INTO apartments (id, floor_id, apartment_number) VALUES
(-9, -4, 1), (-10, -4, 2),
(-11, -5, 3), (-12, -5, 4);


-- ============================================================
--  Slike zgrada
-- ============================================================
INSERT INTO building_images (id, building_id, image_path) VALUES
(-1, -1, 'images/zgrada1_front.jpg'),
(-2, -1, 'images/zgrada1_side.jpg'),
(-3, -2, 'images/zgrada2_front.jpg');


-- ============================================================
--  Zahtevi za pristup zgradi (test podaci za funkcionalnost 1.2)
-- ============================================================
INSERT INTO building_access_requests (id, tenant_id, building_id, apartment_number, created_at, status, rejection_reason) VALUES
(-1, -3, -1, 1, '2026-04-01 10:00:00', 'Pending', NULL),
(-2, -4, -1, 4, '2026-04-02 14:30:00', 'Pending', NULL),
(-3, -3, -2, 2, '2026-04-03 09:00:00', 'Pending', NULL),
(-4, -7, -1, 7, '2026-03-20 11:00:00', 'Accepted', NULL),
(-5, -8, -1, 2, '2026-03-15 16:00:00', 'Rejected', 'Stan je već zauzet.');


-- ============================================================
--  Članstva u zgradama (za prethodno odobren zahtev -4)
-- ============================================================
INSERT INTO building_memberships (id, building_id, tenant_id, floor_number, apartment_number, joined_at) VALUES
(-1, -1, -7, 3, 7, '2026-03-20 11:00:00'),
(-2, -1, -3, 1, 1, '2026-04-05 10:00:00'),
(-3, -1, -4, 2, 4, '2026-04-06 14:30:00'),
(-4, -2, -3, 1, 2, '2026-04-10 10:00:00');


-- ============================================================
--  Oglasna tabla za pomoć (test podaci za funkcionalnost 2.3)
-- ============================================================
INSERT INTO bulletin_boards (id, building_id) VALUES
(-1, -1),
(-2, -2);

INSERT INTO advertisements (id, bulletin_board_id, tenant_id, description, is_offering, start_date, end_date, category, status) VALUES
(-1, -1, -3, 'Nudim pomoć pri selidbi, imam kombi i slobodne ruke.', TRUE,  '2026-05-10', '2026-05-15', 'Selidba', 'Aktivan'),
(-2, -1, -4, 'Tražim nekoga ko može da mi pomogne sa selidbom.', FALSE, '2026-05-12', '2026-05-18', 'Selidba', 'Aktivan'),
(-3, -1, -3, 'Mogu da pomognem sa čišćenjem, imam opremu.', TRUE,  '2026-05-15', '2026-05-25', 'Čišćenje', 'Aktivan'),
(-4, -1, -4, 'Trebam čišćenje stana pre selidbe.', FALSE, '2026-05-17', '2026-05-25', 'Čišćenje', 'Aktivan'),
(-5, -1, -3, 'Trebam nekoga ko bi mi pomogao sa čišćenjem.', FALSE, '2026-04-05', '2026-04-12', 'Čišćenje', 'Arhiviran'),
(-6, -1, -4, 'Nudim pomoć sa čišćenjem, imam sopstvenu opremu.', TRUE,  '2026-04-03', '2026-04-15', 'Čišćenje', 'Aktivan');

-- ============================================================
--  Termini i rezervacije (test podaci za 2.4 Zakazivanje)
-- ============================================================
-- Termini za oglas -1 (Petar nudi Selidba, 10-15 maj)
INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-101, -1, '2026-05-12 16:00:00', '2026-05-12 17:00:00', FALSE),
(-102, -1, '2026-05-12 17:00:00', '2026-05-12 18:00:00', TRUE),
(-103, -1, '2026-05-13 16:00:00', '2026-05-13 17:00:00', TRUE),
(-104, -1, '2026-05-13 17:00:00', '2026-05-13 18:00:00', FALSE),
(-105, -1, '2026-05-14 16:00:00', '2026-05-14 17:00:00', TRUE),
(-106, -1, '2026-05-14 17:00:00', '2026-05-14 18:00:00', TRUE);

-- Termini za oglas -2 (Jovana traži Selidba, 12-18 maj)
INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-201, -2, '2026-05-12 16:00:00', '2026-05-12 17:00:00', TRUE),
(-202, -2, '2026-05-13 16:00:00', '2026-05-13 17:00:00', TRUE),
(-203, -2, '2026-05-14 16:00:00', '2026-05-14 17:00:00', TRUE);

-- Termini za oglas -3 (Petar nudi Čišćenje, 15-25 maj)
INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-301, -3, '2026-05-19 16:00:00', '2026-05-19 17:00:00', FALSE),
(-302, -3, '2026-05-19 17:00:00', '2026-05-19 18:00:00', TRUE),
(-303, -3, '2026-05-20 16:00:00', '2026-05-20 17:00:00', TRUE);

-- Termini za oglas -4 (Jovana traži Čišćenje, 17-25 maj)
INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-401, -4, '2026-05-19 16:00:00', '2026-05-19 17:00:00', FALSE),
(-402, -4, '2026-05-20 16:00:00', '2026-05-20 17:00:00', TRUE);

-- Termini za oglas -5 (Petar traži Čišćenje, 5-12 apr, arhiviran)
INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-501, -5, '2026-04-07 16:00:00', '2026-04-07 17:00:00', FALSE);

-- Termini za oglas -6 (Jovana nudi Čišćenje, 3-15 apr)
INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-601, -6, '2026-04-07 16:00:00', '2026-04-07 17:00:00', TRUE),
(-602, -6, '2026-04-08 16:00:00', '2026-04-08 17:00:00', TRUE);

-- Rezervacije:
-- Jovana (-4) rezerviše termine iz Petrovog Selidba oglasa (-1) preko svog oglasa (-2)
INSERT INTO slot_reservations (id, slot_id, requester_ad_id, tenant_id) VALUES
(-1, -101, -2, -4),
(-2, -104, -2, -4);

-- Jovana (-4) rezerviše termin iz Petrovog Čišćenje oglasa (-3) preko svog oglasa (-4)
INSERT INTO slot_reservations (id, slot_id, requester_ad_id, tenant_id) VALUES
(-3, -301, -4, -4);

-- Petar (-3) rezerviše termin iz Jovaninog Čišćenje oglasa (-4) preko svog oglasa (-3)
INSERT INTO slot_reservations (id, slot_id, requester_ad_id, tenant_id) VALUES
(-4, -401, -3, -3);

-- Jovana (-4) rezerviše termin iz Petrovog arhiviranog Čišćenje oglasa (-5) preko svog oglasa (-6)
INSERT INTO slot_reservations (id, slot_id, requester_ad_id, tenant_id) VALUES
(-5, -501, -6, -4);


-- ============================================================
--  Zajedničke prostorije (test podaci za funkcionalnosti 1.3, 1.4, 2.5)
-- ============================================================
INSERT INTO common_rooms (id, building_id, name, description, floor_number, is_multi_day) VALUES
(-1, -1, 'Sala za proslave',  'Prostorija za organizovanje proslava i priredbi, kapaciteta 30 osoba.',  1, FALSE),
(-2, -1, 'Vešernica',         'Prostorija sa četiri veš mašine i sušilicama za stanare.',               0, TRUE),
(-3, -1, 'Ostava',            'Zajednička ostava za bicikle i sezonske stvari.',                        0, FALSE),
(-4, -1, 'Sala za sastanke',  'Opremljena sala sa projektorom i stolom za 10 osoba.',                   2, FALSE);


-- ============================================================
--  Zahtevi za iznajmljivanje prostorija (test podaci za 2.5)
-- Pokriva sve statuse: Pending, ProposedCounter, Approved, Rejected, Cancelled
-- ============================================================

-- PETAR (-3): Na čekanju — želi Salu za proslave 28.05
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-1, -3, -1, '2026-05-28 00:00:00', '2026-05-28 00:00:00', NULL, NULL, NULL, 'Pending');

-- JOVANA (-4): Na čekanju - promena termina — želi Vešernicu 20-25.05, upravnik predložio 22-27.05
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-2, -4, -2, '2026-05-20 00:00:00', '2026-05-25 00:00:00', '2026-05-22 00:00:00', '2026-05-27 00:00:00', NULL, 'ProposedCounter');

-- PETAR (-3): Odobreno — koristio Ostavu 06.05 (jednodnevno, approved_date = tačan dan)
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-3, -3, -3, '2026-05-05 00:00:00', '2026-05-07 00:00:00', '2026-05-06 00:00:00', '2026-05-06 00:00:00', '2026-05-06 08:00:00', 'Approved');

-- JOVANA (-4): Odbijeno — pokušala Salu za proslave 10.05, termin zauzet
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-4, -4, -1, '2026-05-10 00:00:00', '2026-05-10 00:00:00', NULL, NULL, NULL, 'Rejected');

-- PETAR (-3): Otkazano — sam otkazao zahtev za Vešernicu 01-03.05
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-5, -3, -2, '2026-05-01 00:00:00', '2026-05-03 00:00:00', NULL, NULL, NULL, 'Cancelled');

-- JOVANA (-4): Na čekanju — želi Salu za sastanke 30.05
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-6, -4, -4, '2026-05-30 00:00:00', '2026-05-30 00:00:00', NULL, NULL, NULL, 'Pending');
