using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voting.Active.Application.DTOs;
using Voting.Active.Infrastructure.Persistence;
using Voting.Active.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Voting.Active.Api.Controllers;

[ApiController]
[Route("puesto")]
public class VotingController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly IConfiguration _configuration;

    public VotingController(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<VotingConfigurationDto>> GetConfiguration()
    {
        var election = await _context.Elections.FirstOrDefaultAsync();

        if (election is null)
        {
            return NotFound("No active election found");
        }

        var candidates = await _context.Candidates
            .Where(c => c.ElectionId == election.Id)
            .ToListAsync();

        var votingPlace = await _context.VotingPlaces
            .FirstOrDefaultAsync();

        if (votingPlace is null)
        {
            return NotFound("No voting place found");
        }

        var terminals = await _context.VotingTerminals
            .Where(t => t.VotingTable.VotingPlaceId == votingPlace.Id)
            .ToListAsync();

        var response = new VotingConfigurationDto
        {
            Election = new ElectionDto
            {
                Id = election.Id,
                Name = election.Name,
                ElectionType = election.ElectionType,
                StartDate = election.StartDate,
                EndDate = election.EndDate
            },

            Candidates = candidates.Select(c => new CandidateDto
            {
                Id = c.Id,
                Name = c.Name,
                Document = c.Document,
                Party = c.Party,
                PhotoUrl = c.PhotoUrl
            }).ToList(),

            VotingPlace = new VotingPlaceDto
            {
                Id = votingPlace.Id,
                Name = votingPlace.Name,
                Latitude = votingPlace.Latitude,
                Longitude = votingPlace.Longitude,

                Terminals = terminals.Select(t => new VotingTerminalDto
                {
                    Id = t.Id
                }).ToList()
            }
        };

        return Ok(response);
    }

    [HttpGet("votante/{document}")]
    public async Task<ActionResult<VoterStatusDto>> GetVoterStatus(
        string document)
    {
        var voter = await _context.Voters
            .FirstOrDefaultAsync(v => v.Document == document);

        if (voter is null)
        {
            return NotFound("Voter not found");
        }

        return Ok(new VoterStatusDto
        {
            Voted = voter.HasVoted
        });
    }

    [Authorize]
    [HttpPost("votar")]
    public async Task<ActionResult<CastVoteResponseDto>> CastVote(
        [FromBody] CastVoteRequestDto request)
    {
        var voter = await _context.Voters
            .FirstOrDefaultAsync(v => v.Id == request.VoterId);

        if (voter is null)
        {
            return NotFound(new CastVoteResponseDto
            {
                Success = false,
                Message = "Voter not found"
            });
        }

        if (voter.HasVoted)
        {
            return BadRequest(new CastVoteResponseDto
            {
                Success = false,
                Message = "Voter has already voted"
            });
        }

        var candidate = await _context.Candidates
            .FirstOrDefaultAsync(c => c.Id == request.CandidateId);

        if (candidate is null)
        {
            return NotFound(new CastVoteResponseDto
            {
                Success = false,
                Message = "Candidate not found"
            });
        }

        var terminal = await _context.VotingTerminals
            .Include(t => t.VotingTable)
            .ThenInclude(t => t.VotingPlace)
            .FirstOrDefaultAsync(t => t.Id == request.VotingTerminalId);

        if (terminal is null)
        {
            return NotFound(new CastVoteResponseDto
            {
                Success = false,
                Message = "Voting terminal not found"
            });
        }

        if (!terminal.IsActive)
        {
            return BadRequest(new CastVoteResponseDto
            {
                Success = false,
                Message = "Voting terminal is inactive"
            });
        }

        if (!terminal.VotingTable.IsActive)
        {
            return BadRequest(new CastVoteResponseDto
            {
                Success = false,
                Message = "Voting table is inactive"
            });
        }

        if (!terminal.VotingTable.VotingPlace.IsActive)
        {
            return BadRequest(new CastVoteResponseDto
            {
                Success = false,
                Message = "Voting place is inactive"
            });
        }

        var vote = new Vote
        {
            Id = Guid.NewGuid(),

            VotingTerminalId = terminal.Id,

            VotingTableId = terminal.VotingTableId,

            VotingPlaceId = terminal.VotingTable.VotingPlaceId,

            CandidateId = candidate.Id,

            Signature = request.Signature,

            IsValid = true
        };

        await _context.Votes.AddAsync(vote);

        voter.HasVoted = true;

        await _context.SaveChangesAsync();

        return Ok(new CastVoteResponseDto
        {
            Success = true,
            Message = "Vote successfully cast"
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TerminalLoginResponseDto>> Login(
        [FromBody] TerminalLoginRequestDto request)
    {
        var terminal = await _context.VotingTerminals
            .FirstOrDefaultAsync(t =>
                t.Id == request.TerminalId &&
                t.Secret == request.Secret);

        if (terminal is null)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim("terminalId", terminal.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!
            )
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: creds
        );

        return Ok(new TerminalLoginResponseDto
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token)
        });
    }

    [Authorize]
    [HttpGet("votos")]
    public async Task<ActionResult<List<VoteDto>>> GetVotes()
    {
        var votes = await _context.Votes
            .Include(v => v.Candidate)
            .ToListAsync();

        var response = votes.Select(v => new VoteDto
        {
            VoteId = v.Id,

            CandidateName = v.Candidate.Name,

            TerminalId = v.VotingTerminalId,

            TableId = v.VotingTableId,

            PlaceId = v.VotingPlaceId,

            IsValid = v.IsValid
        }).ToList();

        return Ok(response);
    }
}