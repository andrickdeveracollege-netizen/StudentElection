namespace StudentElection.Model.Domain;

public record PositionInfo(string Key, string Label, int MaxPicks = 1);

public static class Positions
{
    public const string Representative = "Representative";

    // Order here = order shown on the ballot, chart and Excel
    public static readonly List<PositionInfo> All = new()
    {
        new("President",      "PRESIDENT"),
        new("VicePresident",  "VICE PRESIDENT"),
        new("Secretary",      "SECRETARY"),
        new("Treasurer",      "TREASURER"),
        new("Auditor",        "AUDITOR"),
        new("PIO",            "PUBLIC INFORMATION OFFICER (PIO)"),
        new("PO",             "PROTOCOL OFFICER (PO)"),
        new("Representative", "REPRESENTATIVE", 2)   // can pick up to 2
    };

    public static bool IsValid(string? key) => All.Any(p => p.Key == key);

    public static string LabelOf(string key) =>
        All.FirstOrDefault(p => p.Key == key)?.Label ?? key;

    public static int OrderOf(string key) => All.FindIndex(p => p.Key == key);

    public static int MaxPicksOf(string key) =>
        All.FirstOrDefault(p => p.Key == key)?.MaxPicks ?? 1;
}