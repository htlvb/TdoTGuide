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
