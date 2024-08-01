mkdir .\.tools -Force | Out-Null
Invoke-WebRequest https://github.com/tailwindlabs/tailwindcss/releases/download/v3.4.5/tailwindcss-windows-x64.exe -OutFile .\.tools\tailwindcss.exe

Push-Location .\src\TdoTGuide.Admin.Client
dotnet tool restore
dotnet libman restore
Pop-Location
