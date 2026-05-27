# Verifying the Sync Implementation

This guide walks through running the node and confirming that every part
of the Electoral Server synchronization works correctly.

---

## Prerequisites

- Electoral Server is running at `https://localhost:8080`
- `GET /nodo` returns a valid configuration payload
- The `NodeSecret` configured in this service matches the one the Electoral Server expects

---

## 1. Start the node stack

```bash
# Start PostgreSQL
docker-compose up -d

# Apply migrations (includes the jurors table)
dotnet ef database update \
  --project src/Voting.Active.Infrastructure \
  --startup-project src/Voting.Active.Api

# Run the API
dotnet run --project src/Voting.Active.Api
```

---

## 2. Verify configuration

Open `src/Voting.Active.Api/appsettings.Development.json` and confirm:

```json
"ElectoralServer": {
  "BaseUrl": "https://localhost:8080",
  "NodeSecret": "SUPER_SECRET_NODE_TOKEN"
}
```

`BaseUrl` and `NodeSecret` must match what the Electoral Server expects.

---

## 3. Confirm the Electoral Server is reachable

Before testing the sync endpoint, verify the Electoral Server responds:

```bash
curl -s -o /dev/null -w "%{http_code}" \
  https://localhost:8080/nodo \
  -H "Authorization: Bearer SUPER_SECRET_NODE_TOKEN" \
  --insecure
```

Expected: `200`. If not, fix connectivity before continuing.

---

## 4. Obtain a JWT

After the first run, `DataSeeder` inserts a terminal. Get its credentials:

```sql
SELECT id, secret FROM voting_terminals LIMIT 1;
```

```http
POST /puesto/login
Content-Type: application/json

{
  "terminalId": "<id from query>",
  "secret": "<secret from query>"
}
```

Save the token. Every protected endpoint requires:

```http
Authorization: Bearer <token>
```

---

## 5. Trigger the sync

```
POST /sync/node
Authorization: Bearer <token>
```

### Expected response `200`

```json
{
  "electionSynced": true,
  "candidatesSynced": 3,
  "votingPlacesSynced": 1,
  "votingTablesSynced": 4,
  "votingTerminalsSynced": 4,
  "votersSynced": 150,
  "jurorsSynced": 5,
  "synchronizedAt": "2026-05-27T..."
}
```

The actual counts depend on what the Electoral Server returns.
All counts must be greater than zero and `electionSynced` must be `true`.

---

## 6. Verify each entity was persisted

Run these queries. All must return at least one row.

```sql
-- Election synced from the server
SELECT id, name, election_type, start_date, end_date
FROM elections;

-- Candidates linked to that election
SELECT c.id, c.name, c.party, c.election_id
FROM candidates c
JOIN elections e ON e.id = c.election_id;

-- Voting places
SELECT id, name, latitude, longitude, is_active
FROM voting_places;

-- Voting tables nested under places
SELECT t.id, t.code, t.is_active, t.voting_place_id
FROM voting_tables t
JOIN voting_places p ON p.id = t.voting_place_id;

-- Voting terminals nested under tables
SELECT vt.id, vt.is_active, vt.voting_table_id
FROM voting_terminals vt
JOIN voting_tables t ON t.id = vt.voting_table_id;

-- Voters with has_voted defaulting to false
SELECT id, name, document, has_voted, election_id
FROM voters
LIMIT 10;

-- Jurors (new entity)
SELECT id, name, document, election_id
FROM jurors
LIMIT 10;
```

Cross-check row counts against the summary returned in step 5.

---

## 7. Verify upsert — sync a second time without changes

Call `POST /sync/node` again.

Expected:

- Same `200` response with the same counts.
- No duplicate rows — counts in the DB must not grow.

```sql
SELECT
  (SELECT COUNT(*) FROM elections)       AS elections,
  (SELECT COUNT(*) FROM candidates)      AS candidates,
  (SELECT COUNT(*) FROM voting_places)   AS places,
  (SELECT COUNT(*) FROM voting_tables)   AS tables,
  (SELECT COUNT(*) FROM voting_terminals) AS terminals,
  (SELECT COUNT(*) FROM voters)          AS voters,
  (SELECT COUNT(*) FROM jurors)          AS jurors;
```

Run this before and after the second sync. All numbers must stay the same.
This confirms the upsert "update if exists" path works and does not insert duplicates.

---

## 8. Verify HasVoted is preserved during re-sync

This is the most important invariant: **the Electoral Server must never reset local voting state.**

### Step 1 — cast a vote with a synced voter and terminal

Use IDs from the sync — query a voter and terminal that came from the server:

```sql
-- Pick a voter
SELECT id, document FROM voters WHERE has_voted = false LIMIT 1;

-- Pick a terminal that belongs to an active table and place
SELECT vt.id
FROM voting_terminals vt
JOIN voting_tables t ON t.id = vt.voting_table_id
JOIN voting_places p ON p.id = t.voting_place_id
WHERE vt.is_active = true AND t.is_active = true AND p.is_active = true
LIMIT 1;

-- Pick a candidate
SELECT id FROM candidates LIMIT 1;
```

Cast the vote:

```http
POST /puesto/votar
Authorization: Bearer <token>
Content-Type: application/json

{
  "votingTerminalId": "<terminal id>",
  "voterId": "<voter id>",
  "candidateId": "<candidate id>",
  "signature": "test-sig"
}
```

Confirm the voter is now marked:

```sql
SELECT has_voted FROM voters WHERE id = '<voter id>';
-- expected: true
```

### Step 2 — trigger sync again

```
POST /sync/node
Authorization: Bearer <token>
```

### Step 3 — confirm HasVoted was not reset

```sql
SELECT has_voted FROM voters WHERE id = '<voter id>';
-- must still be: true
```

If `has_voted` reverted to `false` after sync, the implementation has a bug.

---

## 9. Verify 401 without token

```http
POST /sync/node
```

No `Authorization` header.

Expected: `401 Unauthorized`. The sync endpoint must be protected.

---

## 10. Verify error when Electoral Server is unreachable

Stop the Electoral Server (or temporarily set an invalid `BaseUrl` in config and restart the API).

```http
POST /sync/node
Authorization: Bearer <token>
```

Expected: `500` with a connection error.

This confirms that failures in the Electoral Server surface correctly and do not produce a silent empty sync.

---

## Quick checklist

| # | What to verify | Pass condition |
| --- | --- | --- |
| 1 | Electoral Server reachable | `curl GET /nodo` returns 200 |
| 2 | `POST /sync/node` returns 200 | `electionSynced: true`, counts > 0 |
| 3 | Election persisted | Row exists in `elections` |
| 4 | Candidates linked to election | Rows in `candidates` with correct `election_id` |
| 5 | Place → Table → Terminal chain | FK chain valid in DB |
| 6 | Voters persisted | Rows in `voters`, `has_voted = false` |
| 7 | Jurors persisted | Rows in `jurors` |
| 8 | Second sync — no duplicates | All counts unchanged |
| 9 | HasVoted not reset on re-sync | `has_voted` stays `true` after voting + re-sync |
| 10 | No token → 401 | Endpoint rejects unauthenticated calls |
| 11 | Server down → 500 | Error surfaces, no silent empty sync |
