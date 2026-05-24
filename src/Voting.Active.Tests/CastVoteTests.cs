namespace Voting.Active.Tests;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Voting.Active.Api.Controllers;
using Voting.Active.Application.DTOs;
using Voting.Active.Domain.Entities;
using Voting.Active.Infrastructure.Persistence;
using Xunit;

/// <summary>
/// Pruebas de integración del endpoint POST /puesto/votar.
/// Cubren US-SEC-02 (voto duplicado), US-FIA-01 (terminal inactiva),
/// US-SEC-03 (candidato/votante inválido) y US-AUD-03 (JWT).
/// </summary>
public class CastVoteTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VotingController _controller;

    // IDs fijos para todos los tests
    private readonly Guid _electionId   = Guid.NewGuid();
    private readonly Guid _placeId      = Guid.NewGuid();
    private readonly Guid _tableId      = Guid.NewGuid();
    private readonly Guid _terminalId   = Guid.NewGuid();
    private readonly Guid _candidateId  = Guid.NewGuid();
    private readonly Guid _voterId      = Guid.NewGuid();

    public CastVoteTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]      = "clave-secreta-de-prueba-suficientemente-larga-256bits!!",
                ["Jwt:Issuer"]   = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();

        _controller = new VotingController(_context, config);
        SeedDatabase();
    }

    // ── US-SEC-02: Voto duplicado es rechazado ────────────────────────────────
    [Fact]
    public async Task CastVote_VotanteYaVoto_Retorna400()
    {
        // Marcar al votante como ya votado
        var voter = await _context.Voters.FindAsync(_voterId);
        voter!.HasVoted = true;
        await _context.SaveChangesAsync();

        var request = RequestValido();
        var result = await _controller.CastVote(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("already voted", response.Message);
    }

    // ── US-SEC-02: Voto válido se registra correctamente ─────────────────────
    [Fact]
    public async Task CastVote_VotoValido_Retorna200YMarcaVotante()
    {
        var request = RequestValido();
        var result = await _controller.CastVote(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(ok.Value);
        Assert.True(response.Success);

        // Verificar que el votante quedó marcado como HasVoted
        var voter = await _context.Voters.FindAsync(_voterId);
        Assert.True(voter!.HasVoted);
    }

    // ── US-SEC-02: El voto queda persistido en BD ─────────────────────────────
    [Fact]
    public async Task CastVote_VotoValido_PersisteVotoEnBaseDeDatos()
    {
        var request = RequestValido();
        await _controller.CastVote(request);

        var votoGuardado = await _context.Votes
            .FirstOrDefaultAsync(v => v.VoterId == _voterId);

        Assert.NotNull(votoGuardado);
        Assert.Equal(_candidateId, votoGuardado.CandidateId);
        Assert.Equal(_terminalId, votoGuardado.VotingTerminalId);
        Assert.True(votoGuardado.IsValid);
    }

    // ── US-SEC-03: Votante inexistente es rechazado ───────────────────────────
    [Fact]
    public async Task CastVote_VotanteInexistente_Retorna404()
    {
        var request = RequestValido();
        request.VoterId = Guid.NewGuid(); // ID que no existe

        var result = await _controller.CastVote(request);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(notFound.Value);
        Assert.False(response.Success);
        Assert.Contains("Voter not found", response.Message);
    }

    // ── US-SEC-03: Candidato inexistente es rechazado ─────────────────────────
    [Fact]
    public async Task CastVote_CandidatoInexistente_Retorna404()
    {
        var request = RequestValido();
        request.CandidateId = Guid.NewGuid(); // ID que no existe

        var result = await _controller.CastVote(request);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(notFound.Value);
        Assert.False(response.Success);
        Assert.Contains("Candidate not found", response.Message);
    }

    // ── US-FIA-01: Terminal inactiva rechaza el voto ──────────────────────────
    [Fact]
    public async Task CastVote_TerminalInactiva_Retorna400()
    {
        var terminal = await _context.VotingTerminals.FindAsync(_terminalId);
        terminal!.IsActive = false;
        await _context.SaveChangesAsync();

        var result = await _controller.CastVote(RequestValido());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("terminal is inactive", response.Message);
    }

    // ── US-FIA-01: Mesa inactiva rechaza el voto ──────────────────────────────
    [Fact]
    public async Task CastVote_MesaInactiva_Retorna400()
    {
        var table = await _context.VotingTables.FindAsync(_tableId);
        table!.IsActive = false;
        await _context.SaveChangesAsync();

        var result = await _controller.CastVote(RequestValido());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("table is inactive", response.Message);
    }

    // ── US-FIA-01: Puesto inactivo rechaza el voto ────────────────────────────
    [Fact]
    public async Task CastVote_PuestoInactivo_Retorna400()
    {
        var place = await _context.VotingPlaces.FindAsync(_placeId);
        place!.IsActive = false;
        await _context.SaveChangesAsync();

        var result = await _controller.CastVote(RequestValido());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("place is inactive", response.Message);
    }

    // ── US-SEC-02: Terminal inexistente es rechazada ──────────────────────────
    [Fact]
    public async Task CastVote_TerminalInexistente_Retorna404()
    {
        var request = RequestValido();
        request.VotingTerminalId = Guid.NewGuid();

        var result = await _controller.CastVote(request);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<CastVoteResponseDto>(notFound.Value);
        Assert.False(response.Success);
        Assert.Contains("terminal not found", response.Message);
    }

    // ── US-AUD-01: La firma del voto se persiste en BD ───────────────────────
    [Fact]
    public async Task CastVote_VotoValido_PersisteFirmaCorrectamente()
    {
        var firmaEsperada = "firma-criptografica-de-prueba-001";
        var request = RequestValido();
        request.Signature = firmaEsperada;

        await _controller.CastVote(request);

        var voto = await _context.Votes.FirstOrDefaultAsync(v => v.VoterId == _voterId);
        Assert.NotNull(voto);
        Assert.Equal(firmaEsperada, voto.Signature);
    }

    // ── US-SEC-02: Dos votos seguidos del mismo votante → solo uno persiste ───
    [Fact]
    public async Task CastVote_VotoDobleRapido_SoloPersistePrimero()
    {
        await _controller.CastVote(RequestValido());
        await _controller.CastVote(RequestValido()); // segundo intento

        var totalVotos = await _context.Votes
            .CountAsync(v => v.VoterId == _voterId);

        Assert.Equal(1, totalVotos);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private CastVoteRequestDto RequestValido() => new()
    {
        VoterId          = _voterId,
        CandidateId      = _candidateId,
        VotingTerminalId = _terminalId,
        Signature        = "firma-valida-test"
    };

    private void SeedDatabase()
    {
        var election = new Election
        {
            Id           = _electionId,
            Name         = "Elección Test",
            ElectionType = "PRESIDENTIAL",
            StartDate    = DateTime.UtcNow.AddDays(-1),
            EndDate      = DateTime.UtcNow.AddDays(1)
        };

        var place = new VotingPlace
        {
            Id        = _placeId,
            Name      = "Puesto Test",
            Latitude  = 4.6097m,
            Longitude = -74.0817m,
            IsActive  = true
        };

        var table = new VotingTable
        {
            Id            = _tableId,
            VotingPlaceId = _placeId,
            IsActive      = true
        };

        var terminal = new VotingTerminal
        {
            Id            = _terminalId,
            VotingTableId = _tableId,
            Secret        = "secreto-test",
            IsActive      = true
        };

        var candidate = new Candidate
        {
            Id         = _candidateId,
            ElectionId = _electionId,
            Name       = "Candidato Test",
            Document   = "123456789",
            Party      = "Partido Test",
            PhotoUrl   = ""
        };

        var voter = new Voter
        {
            Id         = _voterId,
            ElectionId = _electionId,
            Name       = "Votante Test",
            Document   = "987654321",
            HasVoted   = false
        };

        _context.Elections.Add(election);
        _context.VotingPlaces.Add(place);
        _context.VotingTables.Add(table);
        _context.VotingTerminals.Add(terminal);
        _context.Candidates.Add(candidate);
        _context.Voters.Add(voter);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();
}
