-- Initial
DROP TABLE IF EXISTS project;
CREATE TABLE project(
    id UUID PRIMARY KEY NOT NULL,
    title VARCHAR NOT NULL,
    description VARCHAR NOT NULL,
    location VARCHAR NOT NULL,
    organizer JSON NOT NULL,
    co_organizers JSON NOT NULL,
    time_selection JSON NOT NULL
);

-- Add department
DROP TABLE IF EXISTS department;
CREATE TABLE department(
    id SERIAL PRIMARY KEY NOT NULL,
    name VARCHAR NOT NULL,
    color VARCHAR NOT NULL
);
INSERT INTO department (name, color) VALUES
    ('MB', '#183c7b'),
    ('ME', '#ad1410'),
    ('FS', '#009ec6'),
    ('GT', '#008040'),
    ('IEM', '#e78a00'),
    ('IEI', '#6b1c52');
ALTER TABLE project ADD COLUMN departments JSON;
UPDATE project SET departments=(SELECT json_agg(id) FROM department);
ALTER TABLE project ALTER COLUMN departments SET NOT NULL;

-- Add project group
ALTER TABLE project ADD COLUMN "group" VARCHAR;
UPDATE project SET "group"='';
ALTER TABLE project ALTER COLUMN "group" SET NOT NULL;

-- Add department long name
ALTER TABLE department ADD COLUMN "long_name" VARCHAR;
UPDATE department SET long_name='Maschinenbau - Anlagentechnik' WHERE name='MB';
UPDATE department SET long_name='Mechatronik' WHERE name='ME';
UPDATE department SET long_name='Fachschule Maschinenbau' WHERE name='FS';
UPDATE department SET long_name='Gebäudetechnik' WHERE name='GT';
UPDATE department SET long_name='Industrial Engineering and Management' WHERE name='IEM';
UPDATE department SET long_name='Industrial Engineering and Informatics' WHERE name='IEI';
ALTER TABLE department ALTER COLUMN long_name SET NOT NULL;

-- Allow empty project group
ALTER TABLE project ALTER COLUMN "group" DROP NOT NULL;

-- Add buildings
DROP TABLE IF EXISTS building;
CREATE TABLE building(
    id SERIAL PRIMARY KEY NOT NULL,
    name VARCHAR NOT NULL
);
INSERT INTO building (name) VALUES
    ('Theorie'),
    ('Labor'),
    ('Werkstätte');
ALTER TABLE project ADD COLUMN "building" INTEGER;
UPDATE project SET "building"=1;
ALTER TABLE project ALTER COLUMN "building" SET NOT NULL;

-- Remove time selection
ALTER TABLE project DROP COLUMN "time_selection";
