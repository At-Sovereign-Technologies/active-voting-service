namespace Voting.Active.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Voting.Active.Api.Controllers;
using Voting.Active.Application.DTOs;
using Voting.Active.Domain.Entities;
using Voting.Active.Infrastructure.Persistence;
using Xunit;

/// <summary>
/// Pruebas del endpoint GET /puesto/votante/{document}.
/// Cubren US-SEC-03 (verificación de estado del votante antes de emitir voto).
/// </summary>
public class VoterStatusTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VotingController _controller;
    private readonly Guid _electionId = Guid.NewGuid();

    public VoterStatusTests()
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
    }

    // ── US-SEC-03: Votante que no ha votado retorna Voted=false ──────────────
    [Fact]
    public async Task GetVoterStatus_VotanteNoHaVotado_RetornaFalse()
    {
        _context.Voters.Add(new Voter
        {
            Id         = Guid.NewGuid(),
            ElectionId = _electionId,
            Name       = "Juan Pérez",
            Document   = "111222333",
            HasVoted   = false
        });
        await _context.SaveChangesAsync();

        var result = await _controller.GetVoterStatus("111222333");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var status = Assert.IsType<VoterStatusDto>(ok.Value);
        Assert.False(status.Voted);
    }

    // ── US-SEC-03: Votante que ya votó retorna Voted=true ────────────────────
    [Fact]
    public async Task GetVoterStatus_VotanteYaVoto_RetornaTrue()
    {
        _context.Voters.Add(new Voter
        {
            Id         = Guid.NewGuid(),
            ElectionId = _electionId,
            Name       = "María García",
            Document   = "444555666",
            HasVoted   = true
        });
        await _context.SaveChangesAsync();

        var result = await _controller.GetVoterStatus("444555666");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var status = Assert.IsType<VoterStatusDto>(ok.Value);
        Assert.True(status.Voted);
    }

    // ── US-SEC-03: Documento inexistente retorna 404 ─────────────────────────
    [Fact]
    public async Task GetVoterStatus_DocumentoInexistente_Retorna404()
    {
        var result = await _controller.GetVoterStatus("documento-que-no-existe");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ── US-SEC-03: Documento vacío retorna 404 ────────────────────────────────
    [Fact]
    public async Task GetVoterStatus_DocumentoVacio_Retorna404()
    {
        var result = await _controller.GetVoterStatus("");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ── US-SEC-03: Dos votantes con documentos distintos no se confunden ──────
    [Fact]
    public async Task GetVoterStatus_DosVotantes_RetornaEstadoCorrecto()
    {
        _context.Voters.AddRange(
            new Voter { Id = Guid.NewGuid(), ElectionId = _electionId,
                Name = "Votante A", Document = "DOC-A", HasVoted = true },
            new Voter { Id = Guid.NewGuid(), ElectionId = _electionId,
                Name = "Votante B", Document = "DOC-B", HasVoted = false }
        );
        await _context.SaveChangesAsync();

        var resultA = await _controller.GetVoterStatus("DOC-A");
        var resultB = await _controller.GetVoterStatus("DOC-B");

        var statusA = ((resultA.Result as OkObjectResult)!.Value as VoterStatusDto)!;
        var statusB = ((resultB.Result as OkObjectResult)!.Value as VoterStatusDto)!;

        Assert.True(statusA.Voted);
        Assert.False(statusB.Voted);
    }

    public void Dispose() => _context.Dispose();
}
