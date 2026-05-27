# Testing Guide — Voting Active Service

## Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 8.x | Build and run |
| Docker + Compose | any recent | PostgreSQL |
| curl / Postman / Swagger UI | — | Manual API testing |

---

## 1. Start the Database

```bash
docker-compose up -d
```

The compose file starts PostgreSQL on `localhost:5432` with:

```
database : voting_db
username : postgres
password : postgres
```

---

## 2. Apply Migrations

```bash
dotnet ef database update \
  --project src/Voting.Active.Infrastructure \
  --startup-project src/Voting.Active.Api
```

This creates all tables including `jurors` (added in the latest migration).

---

## 3. Run the API

```bash
dotnet run --project src/Voting.Active.Api
```

The API starts on `https://localhost:7xxx` and `http://localhost:5xxx` (exact ports are printed on startup).

Swagger UI is available at:

```
https://localhost:<port>/swagger
```

On first startup `DataSeeder` runs and inserts:

| Entity | Value |
|---|---|
| Election | "Presidenciales 2026" — type `presidential` |
| VotingPlace | "Puesto Central Bogota" |
| VotingTable | code `MESA-001` |
| VotingTerminal | `Secret` = random UUID printed in the DB |
| Candidate 1 | Carlos Perez — doc `123456789` — Partido Azul |
| Candidate 2 | Laura Martinez — doc `987654321` — Partido Verde |
| Voter 1 | Juan Gomez — doc `100001` |
| Voter 2 | Maria Lopez — doc `100002` |

> The terminal `Secret` is a random UUID generated at seed time. You need to query it once from the DB to use it in login — see step 4.

---

## 4. Get Seeded Terminal Credentials

Connect to Postgres and run:

```sql
SELECT id, secret FROM voting_terminals LIMIT 1;
```

Save `id` and `secret` — you will need them for login.

---

## 5. End-to-End Voting Flow

### 5.1 Login (get JWT)

```
POST /puesto/login
Content-Type: application/json

{
  "terminalId": "<id from step 4>",
  "secret": "<secret from step 4>"
}
```

Expected response `200`:

```json
{
  "token": "eyJhbGci..."
}
```

Save the token. All protected endpoints require:

```
Authorization: Bearer <token>
```

---

### 5.2 Check voter status (before voting)

```
GET /puesto/votante/100001
```

Expected response `200`:

```json
{
  "voted": false
}
```

---

### 5.3 Get configuration (candidates + terminal info)

```
GET /puesto
```

Expected response `200` with election, candidate list, and voting place. Use the candidate `id` from this response in the next step.

---

### 5.4 Cast a vote

```
POST /puesto/votar
Authorization: Bearer <token>
Content-Type: application/json

{
  "votingTerminalId": "<terminal id from step 4>",
  "voterId": "<voter id from DB or config endpoint>",
  "candidateId": "<candidate id from GET /puesto>",
  "signature": "test-signature"
}
```

Expected response `200`:

```json
{
  "success": true,
  "message": "Vote successfully cast"
}
```

---

### 5.5 Confirm double-vote prevention

Repeat the exact same request from 5.4.

Expected response `400`:

```json
{
  "success": false,
  "message": "Voter has already voted"
}
```

---

### 5.6 Verify voter status changed

```
GET /puesto/votante/100001
```

Expected response `200`:

```json
{
  "voted": true
}
```

---

### 5.7 Verify vote anonymity

```
GET /puesto/votos
Authorization: Bearer <token>
```

Expected response: a list of votes where **no `voterId` field is present**. Each vote only exposes:

```json
{
  "voteId": "...",
  "candidateName": "...",
  "terminalId": "...",
  "tableId": "...",
  "placeId": "...",
  "isValid": true
}
```

---

## 6. Active/Inactive Validation

### Mark terminal inactive and attempt to vote

```sql
UPDATE voting_terminals SET "IsActive" = false WHERE id = '<terminal id>';
```

```
POST /puesto/votar
Authorization: Bearer <token>
{ ... }
```

Expected `400`:

```json
{ "success": false, "message": "Voting terminal is inactive" }
```

Same test applies to `voting_tables` (`IsActive = false` → `"Voting table is inactive"`) and `voting_places` (`"Voting place is inactive"`).

Restore with `SET "IsActive" = true` to continue testing.

---

## 7. Electoral Server Synchronization

### 7.1 Configure the Electoral Server URL

In `appsettings.Development.json`:

```json
"ElectoralServer": {
  "BaseUrl": "https://localhost:8080",
  "NodeSecret": "SUPER_SECRET_NODE_TOKEN"
}
```

Change `BaseUrl` to wherever the Electoral Server is running.

---

### 7.2 Trigger synchronization

```
POST /sync/node
Authorization: Bearer <token>
```

Expected response `200` when the Electoral Server is reachable:

```json
{
  "electionSynced": true,
  "candidatesSynced": 5,
  "votingPlacesSynced": 1,
  "votingTablesSynced": 3,
  "votingTerminalsSynced": 6,
  "votersSynced": 150,
  "jurorsSynced": 4,
  "synchronizedAt": "2026-05-27T..."
}
```

Expected response when Electoral Server is unreachable: `500` with connection error details.

---

### 7.3 Verify sync result

After a successful sync, query the DB:

```sql
SELECT id, name FROM elections;
SELECT id, name, document FROM voters LIMIT 10;
SELECT id, name FROM jurors LIMIT 10;
```

Confirm the data matches what the Electoral Server sent.

---

### 7.4 Verify HasVoted is preserved on re-sync

1. Cast a vote as a voter (set `HasVoted = true`)
2. Trigger `POST /sync/node` again
3. Query the voter: `HasVoted` must still be `true`

This confirms the node's local voting state is never overwritten by the server.

---

## 8. Unit Tests

> The test files exist in `src/Voting.Active.Tests/` but the project file (`.csproj`) has not been created yet and the project is not registered in the solution. The tests use xUnit with an in-memory database.

### 8.1 Create the test project

```bash
dotnet new xunit -n Voting.Active.Tests \
  --output src/Voting.Active.Tests \
  --framework net8.0

dotnet sln VotingActiveService.sln add \
  src/Voting.Active.Tests/Voting.Active.Tests.csproj
```

### 8.2 Add required references

```bash
dotnet add src/Voting.Active.Tests/Voting.Active.Tests.csproj \
  reference \
  src/Voting.Active.Api/Voting.Active.Api.csproj \
  src/Voting.Active.Application/Voting.Active.Application.csproj \
  src/Voting.Active.Infrastructure/Voting.Active.Infrastructure.csproj

dotnet add src/Voting.Active.Tests/Voting.Active.Tests.csproj \
  package Microsoft.EntityFrameworkCore.InMemory --version 8.0.5

dotnet add src/Voting.Active.Tests/Voting.Active.Tests.csproj \
  package Microsoft.AspNetCore.Mvc.Testing --version 8.0.5
```

### 8.3 Remove the placeholder file

```bash
rm src/Voting.Active.Tests/UnitTest1.cs
```

### 8.4 Run all tests

```bash
dotnet test src/Voting.Active.Tests/Voting.Active.Tests.csproj
```

---

## 9. Test Coverage per File

### LoginTests.cs — POST /puesto/login

| Test | Scenario | Expected |
|---|---|---|
| `Login_CredencialesValidas_RetornaTokenJwt` | Valid terminal + secret | 200 with token |
| `Login_CredencialesValidas_TokenTieneFormatoJwt` | Token format | 3 JWT segments |
| `Login_SecretoIncorrecto_Retorna401` | Wrong secret | 401 |
| `Login_TerminalInexistente_Retorna401` | Unknown terminal id | 401 |
| `Login_DosTerminalesDistintas_TokensDiferentes` | Two terminals, same secret | Different tokens |

### CastVoteTests.cs — POST /puesto/votar

| Test | Scenario | Expected |
|---|---|---|
| `CastVote_VotoValido_Retorna200YMarcaVotante` | Happy path | 200 + HasVoted = true |
| `CastVote_VotoValido_PersisteVotoEnBaseDeDatos` | Persistence check | Vote row exists in DB |
| `CastVote_VotanteYaVoto_Retorna400` | Double vote | 400 |
| `CastVote_VotoDobleRapido_SoloPersistePrimero` | Rapid double vote | Only 1 vote in DB |
| `CastVote_VotanteInexistente_Retorna404` | Unknown voter | 404 |
| `CastVote_CandidatoInexistente_Retorna404` | Unknown candidate | 404 |
| `CastVote_TerminalInexistente_Retorna404` | Unknown terminal | 404 |
| `CastVote_TerminalInactiva_Retorna400` | Inactive terminal | 400 |
| `CastVote_MesaInactiva_Retorna400` | Inactive table | 400 |
| `CastVote_PuestoInactivo_Retorna400` | Inactive place | 400 |
| `CastVote_VotoValido_PersisteFirmaCorrectamente` | Signature persistence | Signature stored |

### VoterStatusTests.cs — GET /puesto/votante/{document}

| Test | Scenario | Expected |
|---|---|---|
| `GetVoterStatus_VotanteNoHaVotado_RetornaFalse` | Voter has not voted | `voted: false` |
| `GetVoterStatus_VotanteYaVoto_RetornaTrue` | Voter has voted | `voted: true` |
| `GetVoterStatus_DocumentoInexistente_Retorna404` | Unknown document | 404 |
| `GetVoterStatus_DocumentoVacio_Retorna404` | Empty document | 404 |
| `GetVoterStatus_DosVotantes_RetornaEstadoCorrecto` | Two voters, correct isolation | Independent state |

---

## 10. Security Checks

| Check | How to verify |
|---|---|
| Protected endpoints require JWT | Call `POST /puesto/votar` without token → 401 |
| JWT is terminal-scoped | Token from terminal A is accepted by terminal B (same issuer/audience) — intended |
| Votes are anonymous | `GET /puesto/votos` response has no `voterId` field |
| HasVoted never reverses | After voting, any re-sync with Electoral Server keeps `HasVoted = true` |
| Inactive components block votes | Set `IsActive = false` in DB → 400 on vote attempt |
| Sync requires auth | Call `POST /sync/node` without token → 401 |

---

## 11. Reset State for Re-testing

```sql
-- Clear votes (safe — anonymous, no voter reference)
DELETE FROM votes;

-- Reset all voters to not voted
UPDATE voters SET "HasVoted" = false;

-- Re-enable all components
UPDATE voting_terminals SET "IsActive" = true;
UPDATE voting_tables SET "IsActive" = true;
UPDATE voting_places SET "IsActive" = true;
```

To reseed from scratch (drops all data):

```sql
DROP TABLE IF EXISTS votes, voters, jurors, candidates,
  voting_terminals, voting_tables, voting_places, elections CASCADE;
```

Then re-run `dotnet ef database update` and restart the API.
