namespace StudentElection.Model.DTO;

public record LoginDto(string Password);

public record PositionDto(string Key, string Label);

public record CandidateCreateDto(string Name, string Position, int? Grade);

public record CandidateResponseDto(int Id, string Name, string Position, string PositionLabel, int? Grade);

public record ResultDto(int Id, string Name, string Position, int? Grade, int Votes);

public record VoteRecordDto(
    int Id,
    string VoterName,
    string VoterIdNo,
    int VoterGrade,
    DateTime VotedAt,
    Dictionary<string, string?> Choices);

// Mode: "votes" (delete votes only) or "all" (delete votes and candidates)
public record ResetDto(string Mode);