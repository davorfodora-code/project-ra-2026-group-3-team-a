-- ============================================================
--  CommunityHub — početni test podaci
--  Pokrenuti POSLE database.sql kroz pgAdmin
-- ============================================================


-- ============================================================
--  Korisnici
-- ============================================================
INSERT INTO users (id, username, password, name, surname, birthday) VALUES
(-1, 'marko', 'marko123', 'Marko', 'Marković', '2000-05-15'),
(-2, 'ana', 'ana123', 'Ana', 'Anić', '1998-08-22'),
(-3, 'petar', 'petar123', 'Petar', 'Petrović', '2001-03-10'),
(-4, 'jovana', 'jovana123', 'Jovana', 'Jovanović', '1999-11-30'),
(-5, 'stefan', 'stefan123', 'Stefan', 'Stefanović', '2002-01-25'),
(-6, 'milica', 'milica123', 'Milica', 'Milić', '2000-07-08'),
(-7, 'nikola', 'nikola123', 'Nikola', 'Nikolić', '1997-12-18'),
(-8, 'ivana', 'ivana123', 'Ivana', 'Ivanović', '2001-09-05');


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
--  Kvartovi (za sada dvostruka namena: i "naselje" iz
--  registracije zgrade i "kvart" za koordinatora)
-- ============================================================
INSERT INTO districts (id, name, description, location_id, budget, coordinator_id) VALUES
(-1, 'Zemun',        'Naselje Zemun u Beogradu',        -1, 0, -1),
(-2, 'Novi Beograd', 'Naselje Novi Beograd u Beogradu', -1, 0, -2);


-- ============================================================
--  Adrese
-- ============================================================
INSERT INTO addresses (id, street, street_number) VALUES
(-1, 'Glavna', '15'),
(-2, 'Bulevar Mihajla Pupina', '22');


-- ============================================================
--  Zgrade
-- ============================================================
INSERT INTO buildings (id, address_id, district_id, location_id, number_of_floors, manager_id) VALUES
(-1, -1, -1, -1, 3, -1),
(-2, -2, -2, -1, 2, -2);


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
