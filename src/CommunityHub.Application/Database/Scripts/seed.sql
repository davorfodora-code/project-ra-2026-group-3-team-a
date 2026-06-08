-- ============================================================
--  CommunityHub — početni test podaci
--  Pokrenuti POSLE database.sql kroz pgAdmin
-- ============================================================

-- ============================================================
--  Uskladjavanje postojeće baze — 4.4 trust_level
--  (bezbedno ako kolona/constraint već postoje)
-- ============================================================
ALTER TABLE district_memberships
    ADD COLUMN IF NOT EXISTS trust_level VARCHAR(30) NOT NULL DEFAULT 'Nov';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'chk_trust_level'
    ) THEN
        ALTER TABLE district_memberships
            ADD CONSTRAINT chk_trust_level
            CHECK (trust_level IN ('Nov', 'Neaktivan', 'Aktivan', 'Istaknuti', 'OdPoverenja'));
    END IF;
END $$;


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
(-23, 'filip',  'filip123',  'Filip',  'Tadić',      '1998-01-22', 'Citizen',             'Zemunska 28, Beograd'),
(-12, 'dragan', 'dragan123', 'Dragan', 'Draganović', '1988-03-14', 'Tenant',               'Glavna 15, Zemun'),
(-13, 'maja',   'maja123',   'Maja',   'Majić',      '1991-07-22', 'Tenant',               'Glavna 15, Zemun');


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

-- Sastanci kvarta (4.5)
INSERT INTO district_meetings (id, district_id, meeting_type, meeting_time, date_range_start, date_range_end, status, scheduled_date) VALUES
(-1, -2, 'Dobrodoslica', '20:00', '2026-05-25', '2026-05-29', 'UPripremi', NULL),
(-2, -2, 'Motivacija',   '19:30', '2026-04-20', '2026-04-28', 'Zakazan',   '2026-04-24'),
(-3, -2, 'Motivacija',   '19:00', '2026-04-10', '2026-04-12', 'Otkazan',   NULL),
(-4, -2, 'Dobrodoslica', '18:00', '2026-05-19', '2026-05-21', 'UPripremi', NULL);

INSERT INTO district_meeting_votes (id, meeting_id, citizen_id, preferred_date) VALUES
(-1, -1, -8, '2026-05-27');

-- Sastanci kvarta Zemun (auto-uclanjenje preko Zemunske ulice)
INSERT INTO district_meetings (id, district_id, meeting_type, meeting_time, date_range_start, date_range_end, status, scheduled_date) VALUES
(-5, -1, 'Dobrodoslica', '20:00', '2026-05-25', '2026-05-29', 'UPripremi', NULL),
(-6, -1, 'Motivacija',   '19:30', '2026-04-20', '2026-04-28', 'Zakazan',   '2026-04-24'),
(-7, -1, 'Motivacija',   '19:00', '2026-04-10', '2026-04-12', 'Otkazan',   NULL),
(-8, -1, 'Dobrodoslica', '18:00', '2026-05-19', '2026-05-21', 'UPripremi', NULL);

-- Dogadjaji gradjana (4.3)
-- Statusi: UPripremi (prijava), Zakazan (spreman / buduci), Zavrsen (istorija + evidencija prisustva)
-- Demo za evidenciju: dogadjaj -19, organizator ivana (-8), bez unetog prisustva
INSERT INTO district_events (id, district_id, organizer_id, name, description, event_date, start_time, duration_minutes, min_volunteers, status, created_at) VALUES
(-1,  -1, -7,  'Ciscenje parka',              'Akcija ciscenja centralnog parka u kvartu.',                    '2026-06-20', '09:00', 180, 2, 'UPripremi', '2026-06-01 10:00:00'),
(-2,  -2, -8,  'Uredjenje igralista',         'Farbanje klupa i sredjivanje igralista.',                       '2026-06-22', '10:00', 120, 2, 'Zakazan',   '2026-06-02 12:00:00'),
(-3,  -2, -8,  'Letnje sredjivanje dvorista',  'Priprema zelenih povrsina oko stambenih blokova.',              '2026-06-28', '09:00', 150, 2, 'UPripremi', '2026-06-03 11:00:00'),
(-4,  -2, -10, 'Obelezavanje biciklisticke staze', 'Postavljanje oznaka i uredjenje staze.',                     '2026-07-10', '08:30', 120, 2, 'UPripremi', '2026-06-05 14:00:00'),
(-5,  -2, -11, 'Akcija zelenila oko skole',    'Uredjenje travnjaka i sadnja cvecja.',                          '2026-05-25', '10:00', 120, 2, 'UPripremi', '2026-05-10 09:00:00'),
(-11, -2, -11, 'Sadnja drveca',                 'Sadnja mladica u dvoristu kvartovske skole.',                    '2026-07-15', '11:00', 180, 3, 'UPripremi', '2026-06-08 13:00:00'),
(-12, -2, -14, 'Uredjenje igralista 2',         'Popravka klupa i kanti za otpatke.',                            '2026-07-22', '10:00', 150, 2, 'UPripremi', '2026-06-10 10:00:00'),
(-13, -2, -14, 'Akcija reciklaze',              'Prikupljanje i sortiranje reciklaznog otpada.',                 '2026-05-30', '09:30', 120, 2, 'Zakazan',   '2026-05-12 09:30:00'),
(-19, -2, -8,  'Evidencija prisustva (demo)',   'Zavrsen test dogadjaj — Ivana evidentira prisustvo volontera.',   '2026-05-10', '09:00', 120, 2, 'Zavrsen',   '2026-05-01 10:00:00'),
(-10, -2, -10, 'Prolecno ciscenje bloka',       'Ciscenje oko zgrada i parkinga.',                               '2026-03-01', '09:00', 120, 3, 'Zavrsen',   '2026-02-20 12:00:00'),
(-14, -2, -17, 'Bojenje parkovske ograde',      'Uredjenje ograde i zamena ostecenih delova.',                   '2026-03-27', '08:00', 180, 3, 'Zavrsen',   '2026-03-12 08:15:00'),
(-15, -2, -19, 'Komsijski sajam knjiga',        'Razmena knjiga i uredjenje stajalista.',                        '2026-04-02', '12:00', 120, 2, 'Zavrsen',   '2026-03-20 16:00:00'),
(-18, -1, -22, 'Ciscenje zemunskog parka',       'Akcija ciscenja i uredjenja klupa.',                            '2026-03-22', '09:00', 120, 2, 'Zavrsen',   '2026-03-10 12:00:00');

INSERT INTO district_event_required_items (id, event_id, item_name, assigned_volunteer_id) VALUES
(-1,  -1,  'Rukavice',         -7),
(-2,  -1,  'Vrece za smece',   NULL),
(-3,  -2,  'Farba',            -8),
(-4,  -2,  'Cetke',            -14),
(-21, -3,  'Lopate',           NULL),
(-22, -3,  'Kante',            NULL),
(-23, -4,  'Trake',            -11),
(-24, -4,  'Markeri',          NULL),
(-25, -5,  'Rukavice',         NULL),
(-26, -5,  'Vrece',            NULL),
(-12, -11, 'Lopate',           NULL),
(-13, -11, 'Rukavice',         NULL),
(-27, -12, 'Farba',            NULL),
(-28, -12, 'Sarafi',           NULL),
(-15, -13, 'Kese',             -14),
(-29, -13, 'Rukavice',         -19),
(-30, -19, 'Metle',            -10),
(-31, -19, 'Kese',             -11),
(-32, -19, 'Rukavice',         -14),
(-10, -10, 'Vrece za smece',   -10),
(-11, -10, 'Rukavice',         -11),
(-35, -10, 'Metle',            -14),
(-16, -14, 'Cetke',            -19),
(-33, -14, 'Farba',            -10),
(-36, -14, 'Rukavice',         -11),
(-17, -15, 'Stolovi',          -21),
(-34, -15, 'Knjige',           -19),
(-20, -18, 'Rukavice',         -23),
(-37, -18, 'Metle',            -22);

INSERT INTO district_event_applications (id, event_id, citizen_id, applied_at) VALUES
(-1,  -1,  -7,  '2026-06-05 09:30:00'),
(-2,  -2,  -8,  '2026-06-06 08:00:00'),
(-3,  -2,  -14, '2026-06-06 08:10:00'),
(-4,  -4,  -11, '2026-06-07 10:00:00'),
(-5,  -13, -14, '2026-05-14 09:00:00'),
(-6,  -13, -19, '2026-05-14 09:15:00'),
(-30, -19, -10, '2026-05-05 08:00:00'),
(-31, -19, -11, '2026-05-05 08:10:00'),
(-32, -19, -14, '2026-05-05 08:20:00'),
(-10, -10, -10, '2026-02-25 09:00:00'),
(-11, -10, -11, '2026-02-25 09:10:00'),
(-12, -10, -14, '2026-02-25 09:20:00'),
(-16, -14, -19, '2026-03-20 08:30:00'),
(-33, -14, -10, '2026-03-20 08:40:00'),
(-34, -14, -11, '2026-03-20 08:50:00'),
(-17, -15, -21, '2026-03-25 12:20:00'),
(-35, -15, -19, '2026-03-25 12:30:00'),
(-20, -18, -23, '2026-03-14 09:00:00'),
(-36, -18, -22, '2026-03-14 09:10:00');

INSERT INTO district_event_application_items (id, application_id, required_item_id) VALUES
(-1,  -1,  -1),
(-2,  -2,  -3),
(-3,  -3,  -4),
(-4,  -4,  -23),
(-5,  -5,  -15),
(-6,  -6,  -29),
(-30, -30, -30),
(-31, -31, -31),
(-32, -32, -32),
(-10, -10, -10),
(-11, -11, -11),
(-12, -12, -35),
(-16, -16, -16),
(-33, -33, -33),
(-34, -34, -36),
(-17, -17, -17),
(-35, -35, -34),
(-20, -20, -20),
(-36, -36, -37);

-- Prisustvo samo na Zavrsenim dogadjajima; -19 namerno bez redova (test evidencije)
INSERT INTO district_event_volunteer_attendances (id, event_id, volunteer_id, attended, marked_at) VALUES
(-1,  -10, -10, TRUE,  '2026-03-01 12:30:00'),
(-2,  -10, -11, TRUE,  '2026-03-01 12:30:00'),
(-3,  -10, -14, TRUE,  '2026-03-01 12:35:00'),
(-7,  -14, -19, TRUE,  '2026-03-27 11:50:00'),
(-12, -14, -10, TRUE,  '2026-03-27 11:55:00'),
(-13, -14, -11, FALSE, '2026-03-27 11:58:00'),
(-8,  -15, -21, TRUE,  '2026-04-02 14:05:00'),
(-16, -15, -19, TRUE,  '2026-04-02 14:06:00'),
(-11, -18, -23, TRUE,  '2026-03-22 11:00:00'),
(-37, -18, -22, TRUE,  '2026-03-22 11:05:00');


-- ============================================================
--  Gradski objekti (3.5 i 4.6)
-- ============================================================
INSERT INTO city_objects (id, name, type, location_id, capacity) VALUES
(-1, 'Bioskop Zvezda',    'Bioskop',      -1, 200),
(-2, 'Gradski Bazen',     'Bazen',        -1, 150),
(-3, 'Luna Park',         'Luna park',    -1, 500),
(-4, 'Narodno Pozoriste', 'Pozoriste',    -1, 300),
(-5, 'Zooloski Vrt',      'Zooloski vrt', -1, 1000),
(-6, 'Aquapark',          'Aquapark',     -1, 600);

INSERT INTO city_object_votes (id, city_object_id, citizen_id, voted_at) VALUES
(-1, -1, -7,  '2026-05-01 10:00:00'),
(-2, -1, -8,  '2026-05-02 11:00:00'),
(-3, -2, -10, '2026-05-03 12:00:00'),
(-4, -2, -9,  '2026-05-04 09:00:00'),
(-5, -3, -11, '2026-05-05 14:00:00'),
(-6, -4, -22, '2026-05-06 16:00:00');

INSERT INTO city_object_reservations (id, city_object_id, district_id, start_time, end_time, reserved_at) VALUES
(-1, -1, -2, '2026-03-01 10:00:00', '2026-03-01 14:00:00', '2026-02-25 09:00:00'),
(-2, -2, -1, '2026-04-15 09:00:00', '2026-04-15 12:00:00', '2026-04-10 10:00:00');


-- ============================================================
--  Recenzije koordinatora kvarta (4.7)
-- ============================================================
INSERT INTO district_coordinator_reviews
(id, district_id, coordinator_id, author_id, rating, justification, created_at, status, removed_at)
VALUES
(-1, -2, -5, -8, 5, 'Koordinator je veoma azuran i dobro organizuje aktivnosti.', '2026-05-04 10:00:00', 'Active', NULL),
(-2, -2, -5, -10, 4, 'Komunikacija je dobra, sastanci su korisni.', '2026-05-08 12:00:00', 'Active', NULL),
(-3, -2, -5, -11, 2, 'Nisam zadovoljan jer odgovori na zahteve kasne.', '2026-05-10 09:30:00', 'Active', NULL),
(-4, -1, -5, -22, 5, 'Sve pohvale za organizaciju u Zemunu.', '2026-05-11 15:00:00', 'Active', NULL);

INSERT INTO district_coordinator_review_reports
(id, review_id, reporter_id, reported_at)
VALUES
(-1, -3, -14, '2026-05-12 11:00:00'),
(-2, -3, -17, '2026-05-12 12:00:00');


-- ============================================================
--  Forumi koordinatora kvartova (3.4)
-- ============================================================
INSERT INTO coordinator_forums (id, title, content, created_by_user_id, created_at, is_closed) VALUES
(-1, 'Problemi sa komunalnim preduzecima',
     'Zelim da pokrenemo diskusiju o nacinu saradnje sa JKP-om u nasim kvartovima. Kakva su vasa iskustva?',
     -5, '2026-04-10 10:00:00', FALSE),
(-2, 'Predlog za zajednicki budzet kvartova',
     'Predlazem da koordinatori zajedno razmotre model zajednickog finansiranja vecih projekata.',
     -6, '2026-04-15 14:00:00', FALSE);

INSERT INTO coordinator_forum_comments (id, forum_id, author_id, content, created_at) VALUES
(-1, -1, -6, 'Potpuno se slazem, imali smo slicnih problema u Zemunu proslog meseca.', '2026-04-11 08:30:00'),
(-2, -1, -5, 'Predlazem da zakazemo zajednicki sastanak sa predstavnicima JKP-a.', '2026-04-12 11:00:00');

INSERT INTO coordinator_forum_comment_reactions (id, comment_id, user_id, is_positive) VALUES
(-1, -1, -5, TRUE),
(-2, -2, -6, TRUE);


-- ============================================================
--  Kategorije budzeta, donacije i rashodi (3.7 i 4.8)
-- ============================================================
INSERT INTO budget_categories (id, name) VALUES
(-1, 'Uredjenje ulica'),
(-2, 'Uredjenje parkova'),
(-3, 'Rasveta'),
(-4, 'Cistoca'),
(-5, 'Zelenilo'),
(-6, 'Hitne popravke');

INSERT INTO district_donations (id, district_id, category_id, citizen_id, amount, donated_at) VALUES
(-1, -2, -2, -8,  12000.00, '2026-05-03 10:15:00'),
(-2, -2, -3, -10,  8000.00, '2026-05-06 18:30:00'),
(-3, -2, -4, -11,  5000.00, '2026-05-08 09:45:00'),
(-4, -2, -5, -14,  6500.00, '2026-05-11 13:20:00'),
(-5, -1, -6, -22,  9000.00, '2026-05-09 12:00:00');

INSERT INTO district_expenses (id, district_id, category_id, amount, description, expense_date) VALUES
(-1, -2, -2, 3500.00, 'Popravka dve klupe u parku kod skole.', '2026-05-14'),
(-2, -2, -3, 2200.00, 'Zamena sijalica kod pesacke staze.',    '2026-05-16'),
(-3, -2, -4, 1800.00, 'Vanredno odnosenje kabastog otpada.',   '2026-05-18'),
(-4, -1, -6, 2500.00, 'Sanacija ostecenog poklopca sahta.',    '2026-05-15');

UPDATE districts
SET budget = COALESCE(don.total, 0) - COALESCE(exp.total, 0)
FROM
    (SELECT district_id, SUM(amount) AS total FROM district_donations GROUP BY district_id) don
    FULL OUTER JOIN
    (SELECT district_id, SUM(amount) AS total FROM district_expenses GROUP BY district_id) exp
    USING (district_id)
WHERE districts.id = COALESCE(don.district_id, exp.district_id);


-- ============================================================
--  Pokloni grada (3.8)
-- ============================================================
INSERT INTO city_gifts (id, name, amount, deadline, awarded, winner_district_id) VALUES
(-1, 'Poklon za kontejnere',        120000.00, '2026-08-01 00:00:00', FALSE, NULL),
(-2, 'Poklon za uredjenje parkova',  80000.00, '2026-05-01 00:00:00', FALSE, NULL),
(-3, 'Poklon za rasvetu',            60000.00, '2026-03-01 00:00:00', TRUE,  -1);

INSERT INTO city_gift_registrations (id, gift_id, district_id, coordinator_id, registered_at) VALUES
(-1, -1, -1, -5, '2026-07-10 10:00:00'),
(-2, -1, -2, -5, '2026-07-11 11:00:00'),
(-3, -3, -1, -5, '2026-02-10 09:00:00');


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
(-4, -2, -3, 1, 2, '2026-04-10 10:00:00'),
(-5, -1, -12, 1, 2, '2026-01-15 09:00:00'),
(-6, -1, -13, 2, 5, '2026-01-20 11:00:00');


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


-- ============================================================
--  Obaveštenja — test podaci za 2.6
-- ============================================================
-- Jovana (-4) ima aktivan oglas koji traži selidbu (budući datum)
INSERT INTO advertisements (id, bulletin_board_id, tenant_id, description, is_offering, start_date, end_date, category, status) VALUES
(-7, -1, -4, 'Tražim pomoć oko selidbe krajem juna.', FALSE, '2026-06-20', '2026-06-30', 'Selidba', 'Aktivan');

INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-701, -7, '2026-06-20 16:00:00', '2026-06-20 17:00:00', TRUE),
(-702, -7, '2026-06-21 16:00:00', '2026-06-21 17:00:00', TRUE);

-- Petar (-3) kreira oglas koji nudi selidbu (budući datum) — podudara se sa Jovaninim
INSERT INTO advertisements (id, bulletin_board_id, tenant_id, description, is_offering, start_date, end_date, category, status) VALUES
(-8, -1, -3, 'Nudim pomoć pri selidbi, imam kombi dostupan krajem juna.', TRUE, '2026-06-18', '2026-07-02', 'Selidba', 'Aktivan');

INSERT INTO advertisement_slots (id, advertisement_id, start_time, end_time, is_available) VALUES
(-801, -8, '2026-06-20 16:00:00', '2026-06-20 17:00:00', TRUE),
(-802, -8, '2026-06-20 17:00:00', '2026-06-20 18:00:00', TRUE),
(-803, -8, '2026-06-21 16:00:00', '2026-06-21 17:00:00', TRUE);

-- Notifikacija za Jovanu (-4): Petrov oglas (-8) se podudara sa njenim oglasom (-7)
INSERT INTO notifications (tenant_id, type, related_ad_id, related_slot_id, is_read, created_at) VALUES
(-4, 'Novi podudarni oglas', -8, NULL, FALSE, '2026-06-06 08:00:00');

-- Notifikacija za Petara (-3): Jovana je rezervisala termin na njegovom oglasu (već postoji rezervacija iz seed podataka)
INSERT INTO notifications (tenant_id, type, related_ad_id, related_slot_id, is_read, created_at) VALUES
(-3, 'Rezervacija termina', -1, -1, FALSE, '2026-06-01 10:00:00');
--  Skupstine stanara (test podaci za 1.6 i 1.7)
--  Zgrada -1 (Glavna 15, Zemun) — stanovi: 1-8
-- ============================================================

-- Skupstina 1: predstojeća, 3 teme, 2 predloga stanara, delimicno prisustvo
INSERT INTO tenant_assemblies (id, building_id, scheduled_at, status) VALUES
(-1, -1, '2026-07-15 19:00:00', 'Scheduled');

INSERT INTO assembly_topics (id, assembly_id, text, is_additional) VALUES
(-1, -1, 'Curenje vode na 3. spratu', false),
(-2, -1, 'Ciscenje zajednickih prostorija', false),
(-3, -1, 'Raspodela troskova za lift', false);

INSERT INTO assembly_topic_suggestions (id, assembly_id, tenant_id, topic) VALUES
(-1, -1, -3, 'Sigurnosna kamera na ulazu'),
(-2, -1, -4, 'Problem sa parkingom');

INSERT INTO assembly_attendances (id, assembly_id, apartment_number, tenant_id) VALUES
(-1, -1, 1, -3),
(-2, -1, 4, -4);

-- Skupstina 2: prosla, Confirmed, 5 tema
INSERT INTO tenant_assemblies (id, building_id, scheduled_at, status) VALUES
(-2, -1, '2026-04-24 18:00:00', 'Confirmed');

INSERT INTO assembly_topics (id, assembly_id, text, is_additional) VALUES
(-4, -2, 'Godisnji plan odrzavanja', false),
(-5, -2, 'Zamena interfona', false),
(-6, -2, 'Finansijski izvestaj', false),
(-7, -2, 'Popravka krova', false),
(-8, -2, 'Nabavka kanti za reciklazu', true);

-- Prisustvo na prošloj potvrđenoj skupštini -2 (5/8 stanova = kvorum)
INSERT INTO assembly_attendances (id, assembly_id, apartment_number, tenant_id) VALUES
(-3, -2, 1,  -3),
(-4, -2, 4,  -4),
(-5, -2, 7,  -7),
(-6, -2, 2,  -12),
(-7, -2, 5,  -13);

-- Skupstina 3: otkazana, 4 teme
INSERT INTO tenant_assemblies (id, building_id, scheduled_at, status) VALUES
(-3, -1, '2026-03-27 19:30:00', 'Cancelled');

-- Skupstina 4: predstojeća, zgrada -2 (Bulevar Mihajla Pupina 22), Scheduled
INSERT INTO tenant_assemblies (id, building_id, scheduled_at, status) VALUES
(-4, -2, '2026-08-10 18:00:00', 'Scheduled');

INSERT INTO assembly_topics (id, assembly_id, text, is_additional) VALUES
(-9,  -3, 'Redovni pregled liftova', false),
(-10, -3, 'Zimsko ciscenje', false),
(-11, -3, 'Uredjenje dvorista', false),
(-12, -3, 'Zamena sijalica na hodniku', false);

INSERT INTO assembly_topics (id, assembly_id, text, is_additional) VALUES
(-13, -4, 'Ugradnja interfona', false),
(-14, -4, 'Uredjenje ulaza zgrade', false);

-- Skupstina 5: predstojeća, zgrada -1, bez prisustva — za testiranje prijave prisustva
INSERT INTO tenant_assemblies (id, building_id, scheduled_at, status) VALUES
(-5, -1, '2026-09-20 18:00:00', 'Scheduled');

INSERT INTO assembly_topics (id, assembly_id, text, is_additional) VALUES
(-15, -5, 'Sanacija fasade zgrade', false),
(-16, -5, 'Nabavka kanti za odvojeno odlaganje otpada', false);


-- ============================================================
--  Prijave problema (test podaci za 2.7)
-- ============================================================
INSERT INTO issue_reports (id, tenant_id, building_id, description, priority, reported_at, status) VALUES
(-1, -3, -1, 'Curenje vode na 3. spratu, mokri zidovi u hodniku.',         'Urgent',    '2026-04-14 09:00:00', 'Unresolved'),
(-2, -4, -1, 'Lift se zaglavljuje dva puta dnevno.',                        'Urgent',    '2026-04-20 11:30:00', 'Unresolved'),
(-3, -3, -1, 'Pokvarena sijalica na 3. spratu u hodniku.',                  'SolveSoon', '2026-03-20 14:00:00', 'Unresolved'),
(-4, -4, -1, 'Ostecena ograda na parkingu — opasnost za decu.',             'SolveSoon', '2026-04-08 10:00:00', 'PotentiallyResolved'),
(-5, -3, -1, 'Buka iz podruma kasno naveče.',                               'CanWait',   '2026-03-28 20:00:00', 'PotentiallyResolved'),
(-6, -4, -1, 'Klupa u dvorištu je slomljena.',                              'CanWait',   '2026-04-01 08:00:00', 'Unresolved');
