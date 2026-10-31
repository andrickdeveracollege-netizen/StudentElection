using StudentElection.Model.DTO;

namespace StudentElection.Service;

public interface IStudentService
{
    Task<CheckResultDto> CheckAsync(VoterDto voter);
    Task<string> VoteAsync(VoteDto vote);
}