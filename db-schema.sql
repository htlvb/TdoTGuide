DROP TABLE IF EXISTS project;

CREATE TABLE project(
    id UUID PRIMARY KEY NOT NULL,
    title VARCHAR NOT NULL,
    description VARCHAR NOT NULL,
    location VARCHAR NOT NULL,
    organizer JSON NOT NULL,
    co_organizers JSON NOT NULL,
    date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME
);
