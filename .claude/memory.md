# Voting Active Service Memory

## Project Overview

Voting Active Service is a distributed local electoral voting node responsible for processing active voting operations inside the distributed electoral system.

This service is NOT the central authority.

The Electoral Server is the source of truth and distributes electoral configuration to voting nodes.

Voting Active operates locally after synchronization and must continue functioning even if connectivity with the Electoral Server is lost.

---

# Architecture

## Style

- Modular Monolith
- Clean Architecture
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core

## Layers

- Api
- Application
- Domain
- Infrastructure

---

# Responsibilities

Voting Active Service responsibilities:

- synchronize electoral configuration from Electoral Server
- process votes locally
- validate voters
- prevent double voting
- maintain auditability
- support active/inactive validation
- operate independently after synchronization

Voting Active is NOT responsible for:
- creating elections
- creating candidates
- creating voters
- acting as central authority

---

# Distributed Architecture

## Electoral Server

Responsibilities:
- manage elections
- manage candidates
- manage voters
- manage jurors
- manage voting places
- manage voting terminals
- expose GET /nodo

Acts as:
- central authority
- source of truth

## Voting Active Service

Acts as:
- local voting node
- configuration consumer
- autonomous vote processor

Synchronization flow:

Electoral Server
    ->
GET /nodo
    ->
Voting Active
    ->
Local PostgreSQL persistence

Voting flow:

Voting Terminal
    ->
Voting Active
    ->
Local vote persistence

Votes are currently stored locally only.

---

# Critical Architectural Rules

## Anonymous Voting

Votes MUST remain anonymous.

The Vote entity MUST NOT contain:
- VoterId
- Voter navigation property

Double voting prevention uses:
- Voter.HasVoted

The system must NEVER be able to determine which candidate a voter selected.

This rule must NEVER be reverted.

---

## Vote Traceability

Votes must preserve:
- VotingTerminalId
- VotingTableId
- VotingPlaceId

This allows:
- auditability
- fraud detection
- terminal invalidation
- table invalidation
- place invalidation

Without breaking vote anonymity.

---

## Active/Inactive Validation

VotingPlace, VotingTable, and VotingTerminal can be marked inactive.

Inactive components must reject new votes.

Future support may invalidate previously emitted votes from compromised components.

---

## Synchronization Rules

Synchronization must use UPSERT behavior.

Never blindly recreate data.

Synchronization must NEVER overwrite critical local operational state such as:
- Voter.HasVoted

Local operational integrity always has priority over remote configuration refreshes.

---

# Security

Implemented:
- JWT authentication
- terminal authentication
- Swagger bearer authentication
- anonymous vote persistence

Pending:
- Ed25519 verification
- distributed secrets
- public/private terminal keys
- role-based authorization

---

# Current Implemented Features

Implemented:
- PostgreSQL integration
- EF Core migrations
- Elections persistence
- Candidates persistence
- Voters persistence
- Jurors persistence
- Voting places persistence
- Voting tables persistence
- Voting terminals persistence
- JWT authentication
- terminal login
- vote casting
- anti double voting
- anonymous voting
- auditability
- active/inactive validation
- Swagger integration
- Electoral Server synchronization
- GET /nodo integration via ElectoralServerClient
- Upsert synchronization for all entities
- HasVoted preservation during voter synchronization

---

# Integration Architecture

## Electoral Server Client

Location:
Infrastructure/Integrations/ElectoralServer/

Files:
- ElectoralServerSettings.cs
- ElectoralServerClient.cs
- DTOs/
- Services/INodeSynchronizationService.cs
- Services/NodeSynchronizationService.cs

Responsibilities:
- typed HttpClient integration
- GET /nodo consumption
- Bearer authentication
- DTO deserialization
- distributed synchronization

---

## Sync Endpoint

Endpoint:
POST /sync/node

Characteristics:
- JWT protected
- triggers full synchronization
- persists local configuration
- preserves local operational state

---

## Key Synchronization Rules

- Upsert by Id for all entities
- Preserve Voter.HasVoted
- Preserve local operational integrity
- Synchronize IsActive state
- Single SaveChangesAsync at end of synchronization
- Avoid destructive updates

Document conflicts during voter synchronization are resolved by:
- removing stale seeded voter
- inserting server voter
- preserving HasVoted state

---

# Development Conventions

- Controllers must remain thin
- Business logic belongs in services
- Use DTOs for requests/responses
- Keep Infrastructure isolated
- Use dependency injection
- Use async/await consistently
- Avoid direct entity exposure in APIs
- Prefer incremental refactors over rewrites

NEVER:
- regenerate the entire project
- rewrite authentication flow
- remove working integrations
- break Swagger auth
- break terminal login

---

# Pending Features

Pending:
- periodic synchronization
- automatic synchronization retries
- Ed25519 vote verification
- retroactive vote invalidation
- juror authentication
- distributed node support
- queue/event synchronization
- encrypted vote transport
- vote forwarding to Electoral Server

---

# Important Decisions

## Anonymous Vote Refactor

Originally the Vote entity stored:
- VoterId
- Voter relationship

This was removed because it violated vote secrecy.

Current design:
- Voter.HasVoted prevents double voting
- Votes remain completely anonymous

This decision is permanent.

---

# Change Log

## Initial System
- Created modular monolith structure
- Configured PostgreSQL
- Added EF Core migrations
- Implemented vote persistence

## Security Phase
- Added JWT authentication
- Added terminal authorization

## Auditability Phase
- Added voting place/table/terminal tracking
- Added active/inactive validation

## Privacy Refactor
- Removed VoterId from Vote entity
- Implemented anonymous voting architecture

## Distributed Architecture Phase
- Added Electoral Server synchronization
- Added GET /nodo integration
- Voting Active became distributed node consumer