#!/bin/bash

# Generates SQL that replaces Entra object ids of project organizers and co-organizers with Keycloak user ids.
# Users are matched by short name (Entra user principal name without domain = Keycloak user name).
# Usage (from Keycloak deployment directory): ./migrate-organizers-to-keycloak.sh > migrate-organizers.sql

KCADM='docker compose exec app /opt/keycloak/bin/kcadm.sh'
ORGANIZER_GROUP_ID=6c766d94-3dec-4cf5-94f7-b327b40c56b2

read -p "Keycloak admin user name: " ADMIN_USER_NAME
read -r -s -p "Keycloak admin user password: " ADMIN_USER_PASSWORD
echo >&2
$KCADM config credentials --server http://localhost:8080 --realm master --user $ADMIN_USER_NAME --password "$ADMIN_USER_PASSWORD" >&2

USERS=`$KCADM get groups/$ORGANIZER_GROUP_ID/members --target-realm htlvb --query max=-1 \
    | jq --raw-output '
        def sql: "'\''" + ((. // "") | gsub("'\''"; "'\'''\''")) + "'\''";
        [.[] | select(.id != null and .username != null) | "    (\(.username | ascii_upcase | sql), \(.id | sql), \(.firstName | sql), \(.lastName | sql))"]
        | join(",\n")'`
if [ -z "$USERS" ]; then
    echo "No organizer candidates found in Keycloak." >&2
    exit 1
fi

cat <<EOF
BEGIN;

CREATE TEMPORARY TABLE keycloak_user(
    short_name VARCHAR PRIMARY KEY NOT NULL,
    id UUID NOT NULL,
    first_name VARCHAR NOT NULL,
    last_name VARCHAR NOT NULL
) ON COMMIT DROP;
INSERT INTO keycloak_user (short_name, id, first_name, last_name) VALUES
$USERS;

CREATE FUNCTION pg_temp.migrate_organizer(organizer JSONB) RETURNS JSONB AS \$\$
    SELECT COALESCE(
        (
            SELECT json_build_object('id', u.id, 'first_name', u.first_name, 'last_name', u.last_name, 'short_name', u.short_name)::jsonb
            FROM keycloak_user u
            WHERE u.short_name = upper(organizer->>'short_name')
        ),
        organizer
    )
\$\$ LANGUAGE SQL;

UPDATE project SET
    organizer = pg_temp.migrate_organizer(organizer::jsonb)::json,
    co_organizers = COALESCE(
        (SELECT json_agg(pg_temp.migrate_organizer(o)) FROM jsonb_array_elements(co_organizers::jsonb) o),
        '[]'::json
    );

-- Organizers that couldn't be migrated
SELECT p.title, o->>'short_name' AS short_name, o->>'first_name' AS first_name, o->>'last_name' AS last_name
FROM project p, jsonb_array_elements(jsonb_build_array(p.organizer::jsonb) || p.co_organizers::jsonb) o
WHERE (o->>'id')::uuid NOT IN (SELECT id FROM keycloak_user)
ORDER BY short_name, p.title;

COMMIT;
EOF
