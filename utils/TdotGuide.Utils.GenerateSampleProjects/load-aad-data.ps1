Push-Location $PSScriptRoot

Connect-MgGraph

$OrganizerGroupId = (Get-MgGroup -Filter "displayName eq 'GrpLehrer'").Id
Get-MgGroupMemberAsUser -GroupId $OrganizerGroupId -All -Property Id,GivenName,Surname,UserPrincipalName `
    | Select-Object Id,GivenName,Surname,UserPrincipalName `
    | ConvertTo-Json > .\organizer-candidates.json

Pop-Location
