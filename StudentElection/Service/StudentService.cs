using Microsoft.EntityFrameworkCore;
using StudentElection.Mapper;
using StudentElection.Model.Domain;
using StudentElection.Model.DTO;
using StudentElection.Repository;

namespace StudentElection.Service;

public class StudentService : IStudentService
{
    // Voter's grade -> grade representative they can vote for
    private static readonly Dictionary<int, int> Target = new()
    {
        { 7, 8 }, { 8, 9 }, { 9, 10 }, { 10, 11 }, { 11, 12 }
    };

    private readonly ICandidateRepository _candidates;
    private readonly IVoteRepository _votes;

    public StudentService(ICandidateRepository candidates, IVoteRepository votes)
    {
        _candidates = candidates;
        _votes = votes;
    }

    public async Task<CheckResultDto> CheckAsync(VoterDto voter)
    {
        Validate(voter.Name, voter.VoterIdNo, voter.Grade);

        if (await _votes.ExistsByVoterIdAsync(voter.VoterIdNo.Trim()))
            throw new ServiceException(409, "You have already voted. Only one vote is allowed.");

        var targetGrade = Target[voter.Grade];
        var all = await _candidates.GetAllAsync();

        var positions = Positions.All.Select(p => new BallotPositionDto(
            p.Key,
            Label(p.Key, targetGrade),
            p.MaxPicks,
            Eligible(all, p.Key, targetGrade).Select(c => c.ToChoice()).ToList()
        )).ToList();

        if (positions.All(p => p.Candidates.Count == 0))
            throw new ServiceException(400, "Voting is not open yet. No candidates have been added.");

        return new CheckResultDto(targetGrade, positions);
    }

    public async Task<string> VoteAsync(VoteDto dto)
    {
        Validate(dto.Name, dto.VoterIdNo, dto.Grade);

        if (await _votes.ExistsByVoterIdAsync(dto.VoterIdNo.Trim()))
            throw new ServiceException(409, "You have already voted.");

        var targetGrade = Target[dto.Grade];
        var all = await _candidates.GetAllAsync();
        var choices = new Dictionary<string, List<int>>();

        foreach (var p in Positions.All)
        {
            var eligible = Eligible(all, p.Key, targetGrade).ToList();
            if (eligible.Count == 0) continue;   // no candidates for this position, skip it

            var label = Label(p.Key, targetGrade);
            var picked = new List<int>();
            if (dto.Selections != null &&
                dto.Selections.TryGetValue(p.Key, out var list) && list != null)
                picked = list.Distinct().ToList();

            if (picked.Count == 0)
                throw new ServiceException(400, $"Please select a candidate for {label}.");
            if (picked.Count > p.MaxPicks)
                throw new ServiceException(400, $"You can only select up to {p.MaxPicks} for {label}.");
            if (picked.Any(id => eligible.All(c => c.Id != id)))
                throw new ServiceException(400, $"Invalid candidate selected for {label}.");

            choices[p.Key] = picked;
        }

        if (choices.Count == 0)
            throw new ServiceException(400, "Voting is not open yet. No candidates have been added.");

        try
        {
            await _votes.AddAsync(dto.ToVote(choices));
        }
        catch (DbUpdateException)
        {
            throw new ServiceException(409, "You have already voted.");
        }

        return "Thank you for voting!";
    }

    // Candidates a voter can pick for a position (Representative = target grade only)
    private static IEnumerable<Candidate> Eligible(List<Candidate> all, string position, int targetGrade) =>
        all.Where(c => c.Position == position &&
                       (position != Positions.Representative || c.Grade == targetGrade));

    private static string Label(string key, int targetGrade) =>
        key == Positions.Representative
            ? $"REPRESENTATIVE (GRADE {targetGrade})"
            : Positions.LabelOf(key);

    private static void Validate(string name, string voterId, int grade)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ServiceException(400, "Please enter your name.");
        if (string.IsNullOrWhiteSpace(voterId))
            throw new ServiceException(400, "Please enter your Voter's ID No.");
        if (!Target.ContainsKey(grade))
            throw new ServiceException(400, "Invalid grade level.");
    }
}