namespace StudentElection.Model.Domain;

public class Candidate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";   // see Positions.All
    public int? Grade { get; set; }              // only for Representative (8-12)
}