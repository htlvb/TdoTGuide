#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-server
WORKDIR /src
COPY ["src/TdoTGuide.Visitor.Server/TdoTGuide.Visitor.Server.csproj", "TdoTGuide.Visitor.Server/"]
COPY ["src/TdoTGuide.Server.Common/TdoTGuide.Server.Common.csproj", "TdoTGuide.Server.Common/"]
RUN dotnet restore "TdoTGuide.Visitor.Server/TdoTGuide.Visitor.Server.csproj"
COPY src/ .
WORKDIR "/src/TdoTGuide.Visitor.Server"
RUN dotnet build "TdoTGuide.Visitor.Server.csproj" -c Release -o /app/build

FROM build-server AS publish-server
RUN dotnet publish "TdoTGuide.Visitor.Server.csproj" -c Release -o /app/publish

FROM node:18 AS build-client
WORKDIR /src
COPY src/TdoTGuide.Visitor.Client/package.json src/TdoTGuide.Visitor.Client/package-lock.json ./
RUN npm ci
COPY src/TdoTGuide.Visitor.Client ./
RUN npm run build

FROM base AS final
WORKDIR /app
COPY --from=publish-server /app/publish .
COPY --from=build-client /src/dist ./wwwroot
ENTRYPOINT ["dotnet", "TdoTGuide.Visitor.Server.dll"]