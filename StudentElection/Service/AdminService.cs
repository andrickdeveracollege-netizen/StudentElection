using StudentElection.Mapper;
using StudentElection.Model.Domain;
using StudentElection.Model.DTO;
using StudentElection.Repository;

namespace StudentElection.Service;

public class AdminService : IAdminService
{
    private readonly ICandidateRepository _candidates;
    private readonly IVoteRepository _votes;
    private readonly string _adminPassword;

    public AdminService(ICandidateRepository candidates, IVoteRepository votes, IConfiguration config)
    {
        _candidates = candidates;
        _votes = votes;
        _adminPassword = config["AdminPassword"]
            ?? throw new InvalidOperationException("AdminPassword must be configured.");
    }

    public bool Login(string password) => password == _adminPassword;

    public List<PositionDto> GetPositions() =>
        Positions.All.Select(p => new PositionDto(p.Key, p.Label)).ToList();

    public async Task<List<CandidateResponseDto>> GetCandidatesAsync() =>
        (await _candidates.GetAllAsync()).Select(c => c.ToResponse()).ToList();

    public async Task AddCandidateAsync(CandidateCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ServiceException(400, "Please enter the candidate's name.");
        if (!Positions.IsValid(dto.Position))
            throw new ServiceException(400, "Invalid position.");
        if (dto.Position == Positions.Representative && (dto.Grade is null or < 8 or > 12))
            throw new ServiceException(400, "Representative candidates need a grade level (8-12).");

        await _candidates.AddAsync(dto.ToDomain());
    }

    public async Task DeleteCandidateAsync(int id)
    {
        var candidate = await _candidates.GetByIdAsync(id)
            ?? throw new ServiceException(404, "Candidate not found.");

        if (await _votes.HasVotesForCandidateAsync(id))
            throw new ServiceException(409, "This candidate already has votes and cannot be deleted.");

        await _candidates.RemoveAsync(candidate);
    }

    public async Task<List<ResultDto>> GetResultsAsync()
    {
        var candidates = await _candidates.GetAllAsync();
        var counts = await _votes.CountByCandidateAsync();

        return candidates
            .Select(c => c.ToResult(counts.GetValueOrDefault(c.Id)))
            .OrderBy(r => Positions.OrderOf(r.Position))
            .ThenBy(r => r.Grade)
            .ThenByDescending(r => r.Votes)
            .ToList();
    }

    public async Task<List<VoteRecordDto>> GetVotesAsync()
    {
        var votes = await _votes.GetAllAsync();
        var names = (await _candidates.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        return votes.Select(v => v.ToRecord(names)).ToList();
    }

    public async Task ResetAsync(string mode)
    {
        switch (mode)
        {
            case "votes":
                await _votes.DeleteAllAsync();
                break;
            case "all":
                await _votes.DeleteAllAsync();        // votes first (foreign keys)
                await _candidates.DeleteAllAsync();
                break;
            default:
                throw new ServiceException(400, "Invalid reset mode.");
        }
    }
}