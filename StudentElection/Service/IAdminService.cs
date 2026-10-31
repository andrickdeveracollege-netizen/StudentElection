using StudentElection.Model.DTO;

namespace StudentElection.Service;

public interface IAdminService
{
    bool Login(string password);
    List<PositionDto> GetPositions();
    Task<List<CandidateResponseDto>> GetCandidatesAsync();
    Task AddCandidateAsync(CandidateCreateDto dto);
    Task DeleteCandidateAsync(int id);
    Task<List<ResultDto>> GetResultsAsync();
    Task<List<VoteRecordDto>> GetVotesAsync();
    Task ResetAsync(string mode);
}