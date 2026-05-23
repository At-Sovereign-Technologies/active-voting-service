# Voting Active Service

## Descripción

Voting Active Service es un microservicio desarrollado en .NET 8 para la gestión de votación activa dentro de un sistema electoral distribuido.

El sistema permite:

* Cargar configuración electoral.
* Gestionar puestos, mesas y terminales de votación.
* Validar votantes.
* Emitir votos.
* Evitar doble votación.
* Controlar estados activos/inactivos de componentes electorales.
* Proteger endpoints mediante JWT.
* Mantener trazabilidad básica de votos.

El proyecto fue construido utilizando una arquitectura basada en separación de capas siguiendo principios de Clean Architecture.

---

# Arquitectura

El proyecto está organizado en cuatro capas principales:

```text
src/
├── Voting.Active.Api
├── Voting.Active.Application
├── Voting.Active.Domain
└── Voting.Active.Infrastructure
```

## Voting.Active.Api

Contiene:

* Controllers
* Configuración JWT
* Swagger
* Middlewares
* Punto de entrada del sistema

## Voting.Active.Application

Contiene:

* DTOs
* Contratos de aplicación
* Modelos de entrada y salida

## Voting.Active.Domain

Contiene:

* Entidades del dominio
* Reglas centrales del negocio
* Relaciones entre entidades

## Voting.Active.Infrastructure

Contiene:

* Entity Framework Core
* PostgreSQL
* DbContext
* Configuraciones Fluent API
* Migraciones
* Seed de datos

---

# Tecnologías Utilizadas

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Swagger / OpenAPI
* JWT Authentication

---

# Funcionalidades Implementadas

## 1. Configuración Electoral

Endpoint:

```http
GET /puesto
```

Permite obtener:

* Elección activa
* Candidatos
* Puesto de votación
* Terminales disponibles

Respuesta esperada:

```json
{
  "election": {},
  "candidates": [],
  "votingPlace": {}
}
```

---

## 2. Validación de Votante

Endpoint:

```http
GET /puesto/votante/{documento}
```

Permite verificar si un ciudadano ya ejerció su voto.

Respuesta:

```json
{
  "voted": false
}
```

---

## 3. Emisión de Voto

Endpoint:

```http
POST /puesto/votar
```

Permite registrar un voto dentro del sistema.

Validaciones implementadas:

* Existencia del votante
* Existencia del candidato
* Existencia de la terminal
* Prevención de doble voto
* Validación de terminal activa
* Validación de mesa activa
* Validación de puesto activo
* Persistencia del voto
* Marcado de votante como ya votó

Ejemplo:

```json
{
  "votingTerminalId": "GUID",
  "voterId": "GUID",
  "candidateId": "GUID",
  "signature": "SIGNATURE_TEST"
}
```

---

## 4. Prevención de Doble Voto

El sistema implementa dos mecanismos:

### Validación lógica

```csharp
if (voter.HasVoted)
```

### Restricción única en base de datos

Se implementó un índice único sobre:

```text
Votes.VoterId
```

Esto evita condiciones de carrera y doble votación concurrente.

---

## 5. Control de Componentes Activos

Las siguientes entidades contienen el atributo:

```csharp
IsActive
```

Entidades:

* VotingPlace
* VotingTable
* VotingTerminal

Esto permite:

* Desactivar máquinas comprometidas
* Invalidar mesas
* Deshabilitar puestos completos
* Bloquear emisión de votos desde componentes no autorizados

---

## 6. Autenticación JWT

El sistema implementa autenticación mediante JWT.

### Login de Terminal

Endpoint:

```http
POST /puesto/login
```

La terminal se autentica utilizando:

* TerminalId
* Secret

La API genera un JWT que posteriormente se utiliza para emitir votos.

---

## 7. Protección de Endpoints

El endpoint:

```http
POST /puesto/votar
```

requiere:

```http
Authorization: Bearer TOKEN
```

Solo terminales autenticadas pueden emitir votos.

---

# Base de Datos

El sistema utiliza PostgreSQL.

## Entidades principales

* Elections
* Candidates
* Voters
* VotingPlaces
* VotingTables
* VotingTerminals
* Votes

---

# Seed de Datos

El proyecto incluye un DataSeeder que crea automáticamente:

* Elección de prueba
* Candidatos
* Puesto
* Mesa
* Terminal
* Votantes

Esto permite probar el sistema inmediatamente después de levantar la API.

---

# Ejecución del Proyecto

## 1. Restaurar paquetes

```bash
dotnet restore
```

## 2. Levantar PostgreSQL

```bash
docker compose up -d
```

## 3. Ejecutar migraciones

```bash
dotnet ef database update \
--project src/Voting.Active.Infrastructure \
--startup-project src/Voting.Active.Api
```

## 4. Ejecutar API

```bash
dotnet run --project src/Voting.Active.Api
```

---

# Swagger

Disponible en:

```text
http://localhost:5239/swagger
```

---

# Flujo General del Sistema

```text
1. Terminal inicia sesión
2. API genera JWT
3. Terminal consulta configuración electoral
4. Jurado valida identidad del votante
5. Sistema verifica si el ciudadano ya votó
6. Terminal emite voto autenticado
7. Sistema persiste el voto
8. Sistema marca al votante como votó
```

---

# Seguridad Implementada

* JWT Authentication
* Restricción única para doble voto
* Validación de terminales activas
* Validación de mesas activas
* Validación de puestos activos
* Protección de endpoints sensibles

---

# Mejoras Futuras

## 1. Firma Criptográfica Ed25519

Implementar firma digital real de votos para:

* Integridad
* No repudio
* Auditoría criptográfica

---

## 2. Redis Distributed Lock

Agregar locks distribuidos para evitar concurrencia extrema en emisión de votos.

---

## 3. CQRS

Separar lectura y escritura para mejorar escalabilidad.

---

## 4. Kafka / RabbitMQ

Implementar event sourcing y procesamiento asíncrono de votos.

---

## 5. Conteo en Tiempo Real

Agregar sistema de resultados electorales en vivo.

---

## 6. Auditoría Avanzada

Agregar historial detallado de eventos electorales.

---

## 7. Replicación Multi Nodo

Permitir múltiples nodos electorales sincronizados.

---

# Estado Actual

El proyecto actualmente cuenta con un flujo funcional completo de votación activa:

* Configuración electoral
* Validación de votantes
* Emisión de votos
* Persistencia en PostgreSQL
* Protección JWT
* Prevención de doble voto
* Control de componentes activos

El sistema se encuentra preparado para evolucionar hacia arquitecturas distribuidas más avanzadas.
