#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TdoTGuide.Admin.Server/TdoTGuide.Admin.Server.csproj", "TdoTGuide.Admin.Server/"]
COPY ["src/TdoTGuide.Admin.Shared/TdoTGuide.Admin.Shared.csproj", "TdoTGuide.Admin.Shared/"]
COPY ["src/TdoTGuide.Admin.Client/TdoTGuide.Admin.Client.csproj", "TdoTGuide.Admin.Client/"]
COPY ["src/TdoTGuide.Server.Common/TdoTGuide.Server.Common.csproj", "TdoTGuide.Server.Common/"]
RUN dotnet restore "TdoTGuide.Admin.Server/TdoTGuide.Admin.Server.csproj"
COPY src/ .
WORKDIR "/src/TdoTGuide.Admin.Server"
RUN dotnet build "TdoTGuide.Admin.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TdoTGuide.Admin.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TdoTGuide.Admin.Server.dll"]