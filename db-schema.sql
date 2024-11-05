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

-- Allow multiple groups
ALTER TABLE project ADD COLUMN "groups" JSONB;
UPDATE project SET groups = json_array("group");
ALTER TABLE project DROP COLUMN "group";

-- Combine groups and department to project type
ALTER TABLE project ADD COLUMN "type" JSONB;
UPDATE project SET type = json_object('name': 'general-info') WHERE json_array_length(departments) = 0;
UPDATE project SET type = json_object('name': 'department-independent') WHERE json_array_length(departments) = (SELECT COUNT(*) FROM department);
UPDATE project SET type = json_object('name': 'department-specific', 'selected_values': departments) WHERE type IS NULL;
ALTER TABLE project ALTER COLUMN "type" SET NOT NULL;
ALTER TABLE project DROP COLUMN "departments";
ALTER TABLE project DROP COLUMN "groups";

CREATE TABLE project_type(
    id VARCHAR PRIMARY KEY NOT NULL,
    "order" INT NOT NULL,
    title VARCHAR NOT NULL,
    selection_data JSONB
);
INSERT INTO project_type (id, "order", title, selection_data) VALUES
    ('general-info', 1, 'Allgemeine Infos und Anmeldung', json_object('type': 'simple', 'color': '#000000AA')),
    ('department-independent', 2, 'Abteilungsübergreifend', json_object('type': 'simple', 'color': '#ACA100')),
    ('school-specific', 3, 'Nur bei uns', json_object('type': 'simple', 'color': '#B24DAD')),
    ('department-specific', 4, 'Abteilungsspezifisch', json_object('type': 'multi-select', 'choices': json_array(select json_object('id': id, 'short_name': name, 'long_name': long_name, 'color': color) from department)));
DROP TABLE department;

-- Add building floors
ALTER TABLE project ADD COLUMN floor VARCHAR;
