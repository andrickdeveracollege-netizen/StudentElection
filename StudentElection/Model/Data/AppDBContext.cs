using Microsoft.EntityFrameworkCore;
using StudentElection.Model.Domain;

namespace StudentElection.Model.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Candidate>(e =>
        {
            e.ToTable("Candidates");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Position).HasMaxLength(30).IsRequired();
        });

        b.Entity<Vote>(e =>
        {
            e.ToTable("Votes");
            if (Database.IsNpgsql())
                e.Property(x => x.VotedAt).HasColumnType("timestamp without time zone");
            e.Property(x => x.VoterName).HasMaxLength(100).IsRequired();
            e.Property(x => x.VoterIdNo).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.VoterIdNo).IsUnique();
        });
    }
}
