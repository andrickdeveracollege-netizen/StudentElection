using Microsoft.AspNetCore.Mvc;
using StudentElection.Model.DTO;
using StudentElection.Service;

namespace StudentElection.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IExportService _export;

    public AdminController(IAdminService admin, IExportService export)
    {
        _admin = admin;
        _export = export;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto) =>
        _admin.Login(dto.Password) ? Ok() : Unauthorized();

    [HttpGet("positions")]
    public IActionResult GetPositions() => Ok(_admin.GetPositions());

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates() =>
        Ok(await _admin.GetCandidatesAsync());

    [HttpPost("candidates")]
    public async Task<IActionResult> AddCandidate(CandidateCreateDto dto)
    {
        await _admin.AddCandidateAsync(dto);
        return Ok();
    }

    [HttpDelete("candidates/{id:int}")]
    public async Task<IActionResult> DeleteCandidate(int id)
    {
        await _admin.DeleteCandidateAsync(id);
        return Ok();
    }

    [HttpGet("results")]
    public async Task<IActionResult> Results() =>
        Ok(await _admin.GetResultsAsync());

    [HttpGet("votes")]
    public async Task<IActionResult> Votes() =>
        Ok(await _admin.GetVotesAsync());

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(ResetDto dto)
    {
        await _admin.ResetAsync(dto.Mode);
        return Ok();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var bytes = await _export.ExportExcelAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"election-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }
}