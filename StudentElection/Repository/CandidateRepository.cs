using Microsoft.EntityFrameworkCore;
using StudentElection.Model.Data;
using StudentElection.Model.Domain;

namespace StudentElection.Repository;

public class CandidateRepository : ICandidateRepository
{
    private readonly AppDBContext _db;
    public CandidateRepository(AppDBContext db) => _db = db;

    public async Task<List<Candidate>> GetAllAsync()
    {
        var list = await _db.Candidates.AsNoTracking().ToListAsync();
        return list
            .OrderBy(c => Positions.OrderOf(c.Position))
            .ThenBy(c => c.Grade)
            .ThenBy(c => c.Name)
            .ToList();
    }

    public async Task<Candidate?> GetByIdAsync(int id) => await _db.Candidates.FindAsync(id);

    public async Task AddAsync(Candidate candidate)
    {
        _db.Candidates.Add(candidate);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(Candidate candidate)
    {
        _db.Candidates.Remove(candidate);
        await _db.SaveChangesAsync();
    }

    public Task DeleteAllAsync() =>
        _db.Database.ExecuteSqlRawAsync(_db.Database.IsNpgsql()
            ? "DELETE FROM \"Candidates\"; ALTER TABLE \"Candidates\" ALTER COLUMN \"Id\" RESTART WITH 1;"
            : "DELETE FROM dbo.Candidates; DBCC CHECKIDENT ('dbo.Candidates', RESEED, 0);");
}
