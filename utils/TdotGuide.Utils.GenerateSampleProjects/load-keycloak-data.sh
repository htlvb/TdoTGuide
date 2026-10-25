#!/bin/bash

KCADM='docker compose exec app /opt/keycloak/bin/kcadm.sh'
ORGANIZER_GROUP_ID=6c766d94-3dec-4cf5-94f7-b327b40c56b2

read -p "Keycloak admin user name: " ADMIN_USER_NAME
read -r -s -p "Keycloak admin user password: " ADMIN_USER_PASSWORD
$KCADM config credentials --server http://localhost:8080 --realm master --user $ADMIN_USER_NAME --password "$ADMIN_USER_PASSWORD"

$KCADM get groups/$ORGANIZER_GROUP_ID/members --target-realm htlvb --query max=-1 \
    | jq '[.[] | { id, firstName, lastName, username }]' \
    > "$(dirname "$0")/organizer-candidates.json"
