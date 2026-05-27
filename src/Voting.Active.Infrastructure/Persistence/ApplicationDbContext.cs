using Microsoft.EntityFrameworkCore;
using Voting.Active.Domain.Entities;

namespace Voting.Active.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Election> Elections => Set<Election>();

    public DbSet<VotingPlace> VotingPlaces => Set<VotingPlace>();

    public DbSet<VotingTable> VotingTables => Set<VotingTable>();

    public DbSet<Candidate> Candidates => Set<Candidate>();

    public DbSet<Voter> Voters => Set<Voter>();

    public DbSet<VotingTerminal> VotingTerminals => Set<VotingTerminal>();

    public DbSet<Vote> Votes => Set<Vote>();

    public DbSet<Juror> Jurors => Set<Juror>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Election>(entity =>
        {
            entity.ToTable("elections");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ElectionType)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<VotingPlace>(entity =>
        {
            entity.ToTable("voting_places");

            entity.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany<VotingTable>()
                .WithOne(v => v.VotingPlace)
                .HasForeignKey(v => v.VotingPlaceId);
        });

        modelBuilder.Entity<VotingTable>(entity =>
        {
            entity.ToTable("voting_tables");

            entity.Property(v => v.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(v => v.VotingPlace)
                .WithMany()
                .HasForeignKey(v => v.VotingPlaceId);

            entity.HasMany(v => v.VotingTerminals)
                .WithOne(v => v.VotingTable)
                .HasForeignKey(v => v.VotingTableId);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("candidates");

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.Document)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.Party)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasOne(c => c.Election)
                .WithMany()
                .HasForeignKey(c => c.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Voter>(entity =>
        {
            entity.ToTable("voters");

            entity.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(v => v.Document)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(v => v.Document)
                .IsUnique();

            entity.HasOne(v => v.Election)
                .WithMany()
                .HasForeignKey(v => v.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VotingTerminal>(entity =>
        {
            entity.ToTable("voting_terminals");

            entity.Property(v => v.Secret)
                .IsRequired();

            entity.Property(v => v.PublicKey)
                .IsRequired();

            entity.HasOne(v => v.VotingTable)
                .WithMany()
                .HasForeignKey(v => v.VotingTableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Juror>(entity =>
        {
            entity.ToTable("jurors");

            entity.Property(j => j.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(j => j.Document)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(j => j.Election)
                .WithMany()
                .HasForeignKey(j => j.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("votes");

            entity.Property(v => v.Signature)
                .IsRequired();

            entity.HasOne(v => v.Candidate)
                .WithMany()
                .HasForeignKey(v => v.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.VotingTerminal)
                .WithMany()
                .HasForeignKey(v => v.VotingTerminalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.VotingTable)
                .WithMany()
                .HasForeignKey(v => v.VotingTableId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.VotingPlace)
                .WithMany()
                .HasForeignKey(v => v.VotingPlaceId)
                .OnDelete(DeleteBehavior.Restrict);

        });
    }
}