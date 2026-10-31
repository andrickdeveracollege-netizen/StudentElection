using StudentElection.Model.Domain;

namespace StudentElection.Repository;

public interface ICandidateRepository
{
    Task<List<Candidate>> GetAllAsync();
    Task<Candidate?> GetByIdAsync(int id);
    Task AddAsync(Candidate candidate);
    Task RemoveAsync(Candidate candidate);
    Task DeleteAllAsync();
}