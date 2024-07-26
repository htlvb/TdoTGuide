#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TdoTGuide.WebAsm/Server/TdoTGuide.WebAsm.Server.csproj", "TdoTGuide.WebAsm/Server/"]
COPY ["TdoTGuide.WebAsm/Shared/TdoTGuide.WebAsm.Shared.csproj", "TdoTGuide.WebAsm/Shared/"]
COPY ["TdoTGuide.WebAsm/Client/TdoTGuide.WebAsm.Client.csproj", "TdoTGuide.WebAsm/Client/"]
RUN dotnet restore "TdoTGuide.WebAsm/Server/TdoTGuide.WebAsm.Server.csproj"
COPY . .
WORKDIR "/src/TdoTGuide.WebAsm/Server"
RUN dotnet build "TdoTGuide.WebAsm.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TdoTGuide.WebAsm.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TdoTGuide.WebAsm.Server.dll"]