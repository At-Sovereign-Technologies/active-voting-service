# Project Context

This repository contains the Voting Active Service of the electoral system.

Architecture:
- Modular monolith using Clean Architecture
- ASP.NET Core Web API
- PostgreSQL with Entity Framework Core

Responsibilities:
- Operate as a local voting node
- Synchronize electoral configuration from Electoral Server
- Process anonymous votes locally
- Prevent double voting
- Audit voting terminals/tables/places
- Support active/inactive component validation

Important architectural rules:
- Votes MUST remain anonymous
- Vote entity MUST NOT contain VoterId
- Double voting prevention uses Voter.HasVoted
- Electoral Server is the source of truth
- Voting Active only consumes synchronized configuration

Synchronization:
- Electoral Server exposes GET /nodo
- Voting Active downloads and stores local configuration
- Local voting must continue even if central server becomes unavailable

Security:
- JWT authentication for terminals
- Future support for Ed25519 signatures

Current stack:
- .NET 8
- Entity Framework Core
- PostgreSQL
- Swagger