#!/bin/bash

KCADM='docker compose exec app /opt/keycloak/bin/kcadm.sh'

read -p "Keycloak admin user name: " ADMIN_USER_NAME
read -r -s -p "Keycloak admin user password: " ADMIN_USER_PASSWORD
$KCADM config credentials --server http://localhost:8080 --realm master --user $ADMIN_USER_NAME --password "$ADMIN_USER_PASSWORD"

$KCADM create clients --target-realm htlvb \
    --set 'name=HTLVB TdoT Guide' \
    --set clientId=tdot-guide \
    --set enabled=true \
    --set rootUrl=https://admin.tdot.htlvb.at/ \
    --set baseUrl=https://admin.tdot.htlvb.at/ \
    --set "redirectUris=[\"https://localhost:7206/authentication/login-callback\", \"https://admin.tdot.htlvb.at/authentication/login-callback\"]" \
    --set "webOrigins=[\"+\"]" \
    --set publicClient=true \
    --set frontchannelLogout=true \
    --set "attributes.\"frontchannel.logout.url\"=https://admin.tdot.htlvb.at/" \
    --set "attributes.\"post.logout.redirect.uris\"=https://localhost:7206/authentication/logout-callback##https://admin.tdot.htlvb.at/authentication/logout-callback" \
    --set "attributes.\"pkce.code.challenge.method\"=S256"

CLIENT_ID=`$KCADM get clients --target-realm htlvb --fields id,clientId | jq '.[] | select(.clientId == "tdot-guide") | .id' --raw-output`

# Client isn't added as audience by default (see https://github.com/keycloak/keycloak/issues/12415#issuecomment-1571690295)
# TdoT Guide instead skips audience validation

$KCADM create clients/$CLIENT_ID/roles --target-realm htlvb --set name=all-projects-editor
$KCADM create clients/$CLIENT_ID/roles --target-realm htlvb --set name=project-creator
$KCADM create clients/$CLIENT_ID/roles --target-realm htlvb --set name=project-reader

# Organizer candidates are loaded from Keycloak using the access token of the current user
$KCADM add-roles --target-realm htlvb --gname Lehrer --cclientid realm-management --rolename view-users
$KCADM add-roles --target-realm htlvb --gname Lehrer --cclientid tdot-guide --rolename project-reader
$KCADM add-roles --target-realm htlvb --gname Lehrer --cclientid tdot-guide --rolename project-creator
$KCADM add-roles --target-realm htlvb --uusername dlas --cclientid tdot-guide --rolename all-projects-editor
$KCADM add-roles --target-realm htlvb --uusername eggj --cclientid tdot-guide --rolename all-projects-editor
$KCADM add-roles --target-realm htlvb --uusername leim --cclientid tdot-guide --rolename all-projects-editor
