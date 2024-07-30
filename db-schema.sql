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
