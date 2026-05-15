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
(-9, 'boban',  'boban123',  'Boban',  'Bobanović',  '1999-04-10', 'Citizen',              'Zemunska 22, Beograd');


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

-- Dogadjaji gradjana (4.3)
INSERT INTO district_events (id, district_id, organizer_id, name, description, event_date, start_time, duration_minutes, min_volunteers, status, created_at) VALUES
(-1, -1, -7, 'Ciscenje parka', 'Akcija ciscenja centralnog parka u kvartu.', '2026-06-20', '09:00', 180, 2, 'UPripremi', '2026-06-01 10:00:00'),
(-2, -2, -8, 'Uredjenje igralista', 'Farbanje klupa i sredjivanje igralista.', '2026-06-22', '10:00', 120, 2, 'Zakazan', '2026-06-02 12:00:00');

INSERT INTO district_event_required_items (id, event_id, item_name, assigned_volunteer_id) VALUES
(-1, -1, 'Rukavice', -7),
(-2, -1, 'Vrece za smece', NULL),
(-3, -2, 'Farba', -8),
(-4, -2, 'Cetke', -9);

INSERT INTO district_event_applications (id, event_id, citizen_id, applied_at) VALUES
(-1, -1, -7, '2026-06-05 09:30:00'),
(-2, -2, -8, '2026-06-06 08:00:00'),
(-3, -2, -9, '2026-06-06 08:10:00');

INSERT INTO district_event_application_items (id, application_id, required_item_id) VALUES
(-1, -1, -1),
(-2, -2, -3),
(-3, -3, -4);


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
-- Maj 2026 — aktivni
(-1, -1, -3, 'Nudim pomoć pri selidbi, imam kombi i slobodne ruke.',    TRUE,  '2026-05-10', '2026-05-15', 'Selidba',           'Aktivan'),
(-2, -1, -4, 'Tražim nekoga ko može da mi pomogne sa selidbom.',        FALSE, '2026-05-12', '2026-05-18', 'Selidba',           'Aktivan'),
(-3, -1, -3, 'Mogu pozajmiti alat za sitne kućne popravke.',            TRUE,  '2026-05-01', '2026-05-31', 'Pozajmljivanje',    'Aktivan'),
(-4, -1, -4, 'Tražim nekoga za popravku veš mašine.',                   FALSE, '2026-05-05', '2026-05-20', 'Popravke uređaja',  'Aktivan'),
-- April 2026 — miks aktivnih i arhiviranih
(-5, -1, -3, 'Nudim pomoć pri čišćenju stana posle selidbe.',           TRUE,  '2026-04-01', '2026-04-10', 'Čišćenje',          'Arhiviran'),
(-6, -1, -4, 'Tražim pomoć sa čišćenjem tepiha.',                       FALSE, '2026-04-05', '2026-04-15', 'Čišćenje',          'Arhiviran'),
(-7, -1, -3, 'Nudim pomoć pri selidbi u aprilu.',                       TRUE,  '2026-04-10', '2026-04-25', 'Selidba',           'Arhiviran'),
(-8, -1, -4, 'Tražim nekoga za popravku bojlera.',                      FALSE, '2026-04-12', '2026-04-20', 'Popravke uređaja',  'Aktivan'),
(-9, -1, -3, 'Pozajmljujem merdevine i osnovni alat.',                  TRUE,  '2026-04-15', '2026-04-30', 'Pozajmljivanje',    'Aktivan');

-- Zauzeti termini za oglase gde Petar nudi pomoć (za top pomagača)
-- Oglas -1 (Petar nudi selidbu, 10-15.05): 5 zauzetih termina
INSERT INTO advertisement_slots (advertisement_id, start_time, end_time, is_available) VALUES
(-1, '2026-05-10 16:00:00', '2026-05-10 17:00:00', FALSE),
(-1, '2026-05-10 17:00:00', '2026-05-10 18:00:00', FALSE),
(-1, '2026-05-11 16:00:00', '2026-05-11 17:00:00', FALSE),
(-1, '2026-05-12 16:00:00', '2026-05-12 17:00:00', FALSE),
(-1, '2026-05-13 16:00:00', '2026-05-13 17:00:00', FALSE),
-- Oglas -3 (Petar pozajmljuje alat, ceo maj): još 3 zauzeta termina
(-3, '2026-05-02 16:00:00', '2026-05-02 17:00:00', FALSE),
(-3, '2026-05-08 17:00:00', '2026-05-08 18:00:00', FALSE),
(-3, '2026-05-14 16:00:00', '2026-05-14 17:00:00', FALSE);


-- ============================================================
--  Zajedničke prostorije (test podaci za funkcionalnosti 1.3 i 1.4)
-- ============================================================
INSERT INTO common_rooms (id, building_id, name, description, floor_number, is_multi_day) VALUES
(-1, -1, 'Sala za proslave', 'Prostorija za organizovanje proslava i priredbi.', 1, FALSE),
(-2, -1, 'Vešernica',        'Prostorija sa veš mašinama za stanare.',            0, TRUE),
(-3, -1, 'Ostava',           'Zajednička ostava za bicikle i sezónske stvari.',   0, FALSE);


-- ============================================================
--  Zahtevi za iznajmljivanje prostorija (test podaci za funkcionalnost 1.4)
-- ============================================================
-- Zahtev na čekanju: Petar želi Salu za proslave 15.05-17.05 (upravnik treba da izabere dan)
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-1, -3, -1, '2026-05-15 00:00:00', '2026-05-17 00:00:00', NULL, NULL, NULL, 'Pending');

-- Zahtev - promena termina: Jovana želi Vešernicu 20.05-25.05, upravnik predložio 21.05-26.05
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-2, -4, -2, '2026-05-20 00:00:00', '2026-05-25 00:00:00', '2026-05-21 00:00:00', '2026-05-26 00:00:00', NULL, 'ProposedCounter');

-- Odobreni zahtev: Petar je koristio Ostavu 06.05-08.05
INSERT INTO common_room_requests (id, tenant_id, common_room_id, requested_start_date, requested_end_date, proposed_start_date, proposed_end_date, approved_date, status) VALUES
(-3, -3, -3, '2026-05-05 00:00:00', '2026-05-08 00:00:00', '2026-05-06 00:00:00', '2026-05-08 00:00:00', '2026-05-06 08:00:00', 'Approved');
