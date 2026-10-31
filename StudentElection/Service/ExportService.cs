using ClosedXML.Excel;
using StudentElection.Model.Domain;
using StudentElection.Repository;

namespace StudentElection.Service;

public class ExportService : IExportService
{
    private readonly ICandidateRepository _candidates;
    private readonly IVoteRepository _votes;

    public ExportService(ICandidateRepository candidates, IVoteRepository votes)
    {
        _candidates = candidates;
        _votes = votes;
    }

    public async Task<byte[]> ExportExcelAsync()
    {
        var votes = (await _votes.GetAllAsync()).OrderBy(v => v.VotedAt).ToList();
        var candidates = await _candidates.GetAllAsync();
        var names = candidates.ToDictionary(c => c.Id, c => c.Name);
        var counts = await _votes.CountByCandidateAsync();

        using var wb = new XLWorkbook();

        // Sheet 1: one row per voter
        var ws1 = wb.Worksheets.Add("Votes");
        var headers = new List<string> { "ID", "Voter Name", "Voter ID No.", "Voter Grade", "Date/Time" };
        headers.AddRange(Positions.All.Select(p => p.Label));
        for (int i = 0; i < headers.Count; i++) ws1.Cell(1, i + 1).Value = headers[i];

        int r = 2;
        foreach (var v in votes)
        {
            ws1.Cell(r, 1).Value = v.Id;
            ws1.Cell(r, 2).Value = v.VoterName;
            ws1.Cell(r, 3).Value = v.VoterIdNo;
            ws1.Cell(r, 4).Value = v.VoterGrade;
            ws1.Cell(r, 5).Value = v.VotedAt;

            int col = 6;
            foreach (var p in Positions.All)
            {
                var picked = v.GetChoices(p.Key)
                    .Where(names.ContainsKey)
                    .Select(id => names[id]);
                ws1.Cell(r, col++).Value = string.Join(", ", picked);
            }
            r++;
        }
        ws1.Row(1).Style.Font.Bold = true;
        ws1.Columns().AdjustToContents();

        // Sheet 2: results per candidate
        var ws2 = wb.Worksheets.Add("Results");
        ws2.Cell(1, 1).Value = "Position";
        ws2.Cell(1, 2).Value = "Grade (Rep only)";
        ws2.Cell(1, 3).Value = "Candidate";
        ws2.Cell(1, 4).Value = "Votes";

        r = 2;
        foreach (var c in candidates)
        {
            ws2.Cell(r, 1).Value = Positions.LabelOf(c.Position);
            if (c.Grade.HasValue) ws2.Cell(r, 2).Value = c.Grade.Value;
            ws2.Cell(r, 3).Value = c.Name;
            ws2.Cell(r, 4).Value = counts.GetValueOrDefault(c.Id);
            r++;
        }
        ws2.Row(1).Style.Font.Bold = true;
        ws2.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}