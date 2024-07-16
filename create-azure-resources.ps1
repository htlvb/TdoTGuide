$SubscriptionName = "Pay-As-You-Go"
$ServerAppName = "OpenHouseGuide-Server"
$ClientAppName = "OpenHouseGuide-Client"

"=== Logging in"
az account set --name $SubscriptionName
$SubscriptionId = az account show --query id -o tsv

"=== Creating server app registration"
$ServerAppAppRoles = New-TemporaryFile
@"
[
  {
    "allowedMemberTypes": [ "User" ],
    "description": "All project editors can edit others' projects.",
    "displayName": "All project editors",
    "isEnabled": true,
    "value": "Project.Write.All"
  },
  {
    "allowedMemberTypes": [ "User" ],
    "description": "Project editors can create projects.",
    "displayName": "Project editors",
    "isEnabled": true,
    "value": "Project.Write"
  },
  {
    "allowedMemberTypes": [ "User" ],
    "description": "Project viewers can view all projects.",
    "displayName": "Project viewers",
    "isEnabled": true,
    "value": "Project.Read"
  }
]
"@ | Set-Content $ServerAppAppRoles
$MSGraphId = az ad sp list --filter "displayname eq 'Microsoft Graph'" --query "[].appId" -o tsv
$ServerAppRequiredResourceAccesses = New-TemporaryFile
@"
[{
    "resourceAppId": "$MSGraphId",
    "resourceAccess": [
        {
            "id": "$(az ad sp show --id $MSGraphId --query "oauth2PermissionScopes[?value=='GroupMember.Read.All'].id | [0]" -o tsv)",
            "type": "Scope"
        },
        {
            "id": "$(az ad sp show --id $MSGraphId --query "oauth2PermissionScopes[?value=='User.Read.All'].id | [0]" -o tsv)",
            "type": "Scope"
        }
   ]
}]
"@ | Set-Content $ServerAppRequiredResourceAccesses
$ServerAppApiScopes = New-TemporaryFile
@"
{
    "oauth2PermissionScopes": [{
        "adminConsentDescription": "Allows the app to access server app API endpoints.",
        "adminConsentDisplayName": "Access API",
        "id": "$(New-Guid)",
        "isEnabled": true,
        "type": "Admin",
        "userConsentDescription": null,
        "userConsentDisplayName": null,
        "value": "Api.Access"
    }]
}
"@ | Set-Content $ServerAppApiScopes
$ServerApp = az ad app create --display-name $ServerAppName `
    --sign-in-audience AzureADMyOrg `
    --app-roles @$ServerAppAppRoles `
    --required-resource-accesses @$ServerAppRequiredResourceAccesses `
    | ConvertFrom-Json
az ad app update --id $ServerApp.appId --identifier-uris "api://$($ServerApp.appId)"
az ad app update --id $ServerApp.appId --set api=@$ServerAppApiScopes

Remove-Item $ServerAppAppRoles, $ServerAppRequiredResourceAccesses, $ServerAppApiScopes

$ServerAppCredentials = az ad app credential reset --id $ServerApp.appId --display-name Initial --years 2 --append | ConvertFrom-Json

az ad sp create --id $ServerApp.appId -o none
$ServerAppResourceId = az ad sp show --id $ServerApp.appId --query "id" -o tsv
$AppRoleAssigments = @(
    [PSCustomObject]@{PrincipalId = az ad group list --query "[?displayName=='GrpLehrer'].id | [0]" -o tsv; AppRoleName = "Project.Read"}
    [PSCustomObject]@{PrincipalId = az ad group list --query "[?displayName=='GrpLehrer'].id | [0]" -o tsv; AppRoleName = "Project.Write"}
    [PSCustomObject]@{PrincipalId = az ad user list --query "[?userPrincipalName=='DLAS@htlvb.at'].id | [0]" -o tsv; AppRoleName = "Project.Write.All"}
    [PSCustomObject]@{PrincipalId = az ad user list --query "[?userPrincipalName=='EGGJ@htlvb.at'].id | [0]" -o tsv; AppRoleName = "Project.Write.All"}
)
foreach ($Item in $AppRoleAssigments) {
    $AppRoleAssignment = New-TemporaryFile
@"
{
    "principalId": "$($Item.PrincipalId)",
    "resourceId": "$ServerAppResourceId",
    "appRoleId": "$(az ad app show --id $ServerApp.appId --query "appRoles[?value=='$($Item.AppRoleName)'].id | [0]" -o tsv)"
}
"@ | Set-Content $AppRoleAssignment
    az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$ServerAppResourceId/appRoleAssignedTo" --headers "Content-Type=application/json" --body @$AppRoleAssignment -o none
    Remove-Item $AppRoleAssignment
}

"=== Creating client app registration"
$ClientAppRequiredResourceAccesses = New-TemporaryFile
@"
[{
    "resourceAppId": "$($ServerApp.appId)",
    "resourceAccess": [
        {
            "id": "$(az ad sp show --id $ServerApp.appId --query "oauth2PermissionScopes[?value=='Api.Access'].id | [0]" -o tsv)",
            "type": "Scope"
        }
   ]
}]
"@ | Set-Content $ClientAppRequiredResourceAccesses
$ClientApp = az ad app create --display-name $ClientAppName `
    --sign-in-audience AzureADMyOrg `
    --required-resource-accesses @$ClientAppRequiredResourceAccesses `
    | ConvertFrom-Json
$ClientAppSpaRedirectUris = New-TemporaryFile
@"
{
    "redirectUris": [
        "https://localhost/authentication/login-callback",
        "https://tdot-guide.htlvb.at/authentication/login-callback",
    ]
}
"@ | Set-Content $ClientAppSpaRedirectUris
az ad app update --id $ClientApp.appId --set spa=@$ClientAppSpaRedirectUris

Remove-Item $ClientAppRequiredResourceAccesses, $ClientAppSpaRedirectUris

Write-Warning "TODO: Update ServerApiScope in appsettings.json to 'api://$($ServerApp.appId)/Api.Access'"

"=== Giving admin consent to server and client app permissions"
"!!! Login with admin account !!!"
az login --use-device-code --allow-no-subscriptions -o none
az ad app permission admin-consent --id $ServerApp.appId
az ad app permission admin-consent --id $ClientApp.appId
az logout
az account set --name $SubscriptionName

<#
"=== Deleting resources"
az ad app delete --id (az ad app list --filter "displayName eq '$ServerAppName'" --query "[].id" -o tsv)
az ad app delete --id (az ad app list --filter "displayName eq '$ClientAppName'" --query "[].id" -o tsv)
#>
