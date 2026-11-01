using Microsoft.EntityFrameworkCore;
using StudentElection.Model.Data;
using StudentElection.Model.Domain;

namespace StudentElection.Repository;

public class VoteRepository : IVoteRepository
{
    private readonly AppDBContext _db;
    public VoteRepository(AppDBContext db) => _db = db;

    public Task<bool> ExistsByVoterIdAsync(string voterIdNo) =>
        _db.Votes.AnyAsync(v => v.VoterIdNo == voterIdNo);

    public Task<bool> HasVotesForCandidateAsync(int id) =>
        _db.Votes.AnyAsync(v =>
            v.PresidentId == id || v.VicePresidentId == id || v.SecretaryId == id ||
            v.TreasurerId == id || v.AuditorId == id || v.PioId == id ||
            v.PoId == id || v.RepresentativeId == id || v.Representative2Id == id);

    public async Task AddAsync(Vote vote)
    {
        _db.Votes.Add(vote);
        await _db.SaveChangesAsync();   // may throw DbUpdateException (duplicate voter ID)
    }

    public Task<List<Vote>> GetAllAsync() =>
        _db.Votes.AsNoTracking().OrderByDescending(v => v.VotedAt).ToListAsync();

    public async Task<Dictionary<int, int>> CountByCandidateAsync()
    {
        var votes = await _db.Votes.AsNoTracking().ToListAsync();
        var counts = new Dictionary<int, int>();
        foreach (var v in votes)
            foreach (var p in Positions.All)
                foreach (var id in v.GetChoices(p.Key))
                    counts[id] = counts.GetValueOrDefault(id) + 1;
        return counts;
    }

    public Task DeleteAllAsync() =>
        _db.Database.ExecuteSqlRawAsync(_db.Database.IsNpgsql()
            ? "DELETE FROM \"Votes\"; ALTER TABLE \"Votes\" ALTER COLUMN \"Id\" RESTART WITH 1;"
            : "DELETE FROM dbo.Votes; DBCC CHECKIDENT ('dbo.Votes', RESEED, 0);");
}
