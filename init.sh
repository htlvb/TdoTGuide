mkdir -p ./.tools
wget -O ./.tools/tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/download/v3.4.5/tailwindcss-linux-x64 || exit 1
chmod +x ./.tools/tailwindcss

pushd ./src/TdoTGuide.Admin.Client
dotnet tool restore || exit 1
dotnet libman restore || exit 1
popd
