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
/// Pruebas del endpoint POST /puesto/login.
/// Cubren US-AUD-03 (autenticación JWT de terminales).
/// </summary>
public class LoginTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VotingController _controller;
    private readonly Guid _terminalId = Guid.NewGuid();
    private const string SecretoValido = "secreto-terminal-001";

    public LoginTests()
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
        SeedTerminal();
    }

    // ── US-AUD-03: Login válido retorna token JWT ─────────────────────────────
    [Fact]
    public async Task Login_CredencialesValidas_RetornaTokenJwt()
    {
        var request = new TerminalLoginRequestDto
        {
            TerminalId = _terminalId,
            Secret     = SecretoValido
        };

        var result = await _controller.Login(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TerminalLoginResponseDto>(ok.Value);
        Assert.NotNull(response.Token);
        Assert.NotEmpty(response.Token);
    }

    // ── US-AUD-03: Token tiene formato JWT válido (3 segmentos) ──────────────
    [Fact]
    public async Task Login_CredencialesValidas_TokenTieneFormatoJwt()
    {
        var request = new TerminalLoginRequestDto
        {
            TerminalId = _terminalId,
            Secret     = SecretoValido
        };

        var result = await _controller.Login(request);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TerminalLoginResponseDto>(ok.Value);

        // JWT tiene exactamente 3 segmentos separados por punto
        var partes = response.Token!.Split('.');
        Assert.Equal(3, partes.Length);
    }

    // ── US-AUD-03: Secreto incorrecto es rechazado con 401 ───────────────────
    [Fact]
    public async Task Login_SecretoIncorrecto_Retorna401()
    {
        var request = new TerminalLoginRequestDto
        {
            TerminalId = _terminalId,
            Secret     = "secreto-incorrecto"
        };

        var result = await _controller.Login(request);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    // ── US-AUD-03: Terminal inexistente es rechazada con 401 ─────────────────
    [Fact]
    public async Task Login_TerminalInexistente_Retorna401()
    {
        var request = new TerminalLoginRequestDto
        {
            TerminalId = Guid.NewGuid(),
            Secret     = SecretoValido
        };

        var result = await _controller.Login(request);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    // ── US-AUD-03: Dos terminales con mismo secreto no se confunden ──────────
    [Fact]
    public async Task Login_DosTerminalesDistintas_TokensDiferentes()
    {
        // Segunda terminal con diferente ID pero mismo secreto
        var terminal2Id = Guid.NewGuid();
        _context.VotingTerminals.Add(new VotingTerminal
        {
            Id            = terminal2Id,
            VotingTableId = Guid.NewGuid(),
            Secret        = SecretoValido,
            IsActive      = true
        });
        await _context.SaveChangesAsync();

        var result1 = await _controller.Login(new TerminalLoginRequestDto
            { TerminalId = _terminalId, Secret = SecretoValido });
        var result2 = await _controller.Login(new TerminalLoginRequestDto
            { TerminalId = terminal2Id, Secret = SecretoValido });

        var token1 = ((result1.Result as OkObjectResult)!.Value as TerminalLoginResponseDto)!.Token;
        var token2 = ((result2.Result as OkObjectResult)!.Value as TerminalLoginResponseDto)!.Token;

        // Los tokens deben ser distintos porque codifican terminalIds distintos
        Assert.NotEqual(token1, token2);
    }

    private void SeedTerminal()
    {
        var tableId = Guid.NewGuid();
        _context.VotingTerminals.Add(new VotingTerminal
        {
            Id            = _terminalId,
            VotingTableId = tableId,
            Secret        = SecretoValido,
            IsActive      = true
        });
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();
}
