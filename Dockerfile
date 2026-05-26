# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VotingActiveService.sln ./
COPY src/Voting.Active.Api/Voting.Active.Api.csproj src/Voting.Active.Api/
COPY src/Voting.Active.Application/Voting.Active.Application.csproj src/Voting.Active.Application/
COPY src/Voting.Active.Domain/Voting.Active.Domain.csproj src/Voting.Active.Domain/
COPY src/Voting.Active.Infrastructure/Voting.Active.Infrastructure.csproj src/Voting.Active.Infrastructure/

RUN dotnet restore VotingActiveService.sln

COPY . .
RUN dotnet publish src/Voting.Active.Api/Voting.Active.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Voting.Active.Api.dll"]
