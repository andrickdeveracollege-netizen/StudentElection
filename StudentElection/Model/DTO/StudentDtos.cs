namespace StudentElection.Model.DTO;

public record VoterDto(string Name, string VoterIdNo, int Grade);

// Selections: position key -> list of candidate ids (e.g. "Representative": [4, 7])
public record VoteDto(string Name, string VoterIdNo, int Grade, Dictionary<string, List<int>> Selections);

public record CandidateChoiceDto(int Id, string Name);

public record BallotPositionDto(string Key, string Label, int MaxPicks, List<CandidateChoiceDto> Candidates);

public record CheckResultDto(int TargetGrade, List<BallotPositionDto> Positions);