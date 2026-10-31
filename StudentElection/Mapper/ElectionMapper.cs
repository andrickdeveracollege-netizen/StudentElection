using StudentElection.Model.Domain;
using StudentElection.Model.DTO;

namespace StudentElection.Mapper;

public static class ElectionMapper
{
    public static Candidate ToDomain(this CandidateCreateDto d) =>
        new()
        {
            Name = d.Name.Trim(),
            Position = d.Position,
            Grade = d.Position == Positions.Representative ? d.Grade : null
        };

    // choices = already validated selections (position key -> candidate ids)
    public static Vote ToVote(this VoteDto d, Dictionary<string, List<int>> choices)
    {
        var v = new Vote
        {
            VoterName = d.Name.Trim(),
            VoterIdNo = d.VoterIdNo.Trim(),
            VoterGrade = d.Grade,
            VotedAt = DateTime.Now
        };
        foreach (var (position, ids) in choices) v.SetChoices(position, ids);
        return v;
    }

    public static CandidateChoiceDto ToChoice(this Candidate c) => new(c.Id, c.Name);

    public static CandidateResponseDto ToResponse(this Candidate c) =>
        new(c.Id, c.Name, c.Position, Positions.LabelOf(c.Position), c.Grade);

    public static ResultDto ToResult(this Candidate c, int votes) =>
        new(c.Id, c.Name, c.Position, c.Grade, votes);

    public static VoteRecordDto ToRecord(this Vote v, Dictionary<int, string> names)
    {
        var choices = new Dictionary<string, string?>();
        foreach (var p in Positions.All)
        {
            var picked = v.GetChoices(p.Key)
                .Where(names.ContainsKey)
                .Select(id => names[id])
                .ToList();
            choices[p.Key] = picked.Count > 0 ? string.Join(", ", picked) : null;
        }
        return new VoteRecordDto(v.Id, v.VoterName, v.VoterIdNo, v.VoterGrade, v.VotedAt, choices);
    }
}