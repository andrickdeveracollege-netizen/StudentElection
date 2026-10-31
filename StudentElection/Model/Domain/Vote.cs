namespace StudentElection.Model.Domain;

public class Vote
{
    public int Id { get; set; }
    public string VoterName { get; set; } = "";
    public string VoterIdNo { get; set; } = "";
    public int VoterGrade { get; set; }

    public int? PresidentId { get; set; }
    public int? VicePresidentId { get; set; }
    public int? SecretaryId { get; set; }
    public int? TreasurerId { get; set; }
    public int? AuditorId { get; set; }
    public int? PioId { get; set; }
    public int? PoId { get; set; }
    public int? RepresentativeId { get; set; }
    public int? Representative2Id { get; set; }

    public DateTime VotedAt { get; set; }

    private static List<int> One(int? id) => id.HasValue ? new List<int> { id.Value } : new List<int>();

    public List<int> GetChoices(string position) => position switch
    {
        "President" => One(PresidentId),
        "VicePresident" => One(VicePresidentId),
        "Secretary" => One(SecretaryId),
        "Treasurer" => One(TreasurerId),
        "Auditor" => One(AuditorId),
        "PIO" => One(PioId),
        "PO" => One(PoId),
        "Representative" => One(RepresentativeId).Concat(One(Representative2Id)).ToList(),
        _ => new List<int>()
    };

    public void SetChoices(string position, List<int> ids)
    {
        int? first = ids.Count > 0 ? ids[0] : null;
        int? second = ids.Count > 1 ? ids[1] : null;

        switch (position)
        {
            case "President": PresidentId = first; break;
            case "VicePresident": VicePresidentId = first; break;
            case "Secretary": SecretaryId = first; break;
            case "Treasurer": TreasurerId = first; break;
            case "Auditor": AuditorId = first; break;
            case "PIO": PioId = first; break;
            case "PO": PoId = first; break;
            case "Representative":
                RepresentativeId = first;
                Representative2Id = second;
                break;
        }
    }
}