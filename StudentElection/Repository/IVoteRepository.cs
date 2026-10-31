using StudentElection.Model.Domain;

namespace StudentElection.Repository;

public interface IVoteRepository
{
    Task<bool> ExistsByVoterIdAsync(string voterIdNo);
    Task<bool> HasVotesForCandidateAsync(int candidateId);
    Task AddAsync(Vote vote);
    Task<List<Vote>> GetAllAsync();
    Task<Dictionary<int, int>> CountByCandidateAsync();
    Task DeleteAllAsync();
}