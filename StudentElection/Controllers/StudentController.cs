using Microsoft.AspNetCore.Mvc;
using StudentElection.Model.DTO;
using StudentElection.Service;

namespace StudentElection.Controllers;

[ApiController]
[Route("api/student")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _service;
    public StudentController(IStudentService service) => _service = service;

    [HttpPost("check")]
    public async Task<IActionResult> Check(VoterDto voter) =>
        Ok(await _service.CheckAsync(voter));

    [HttpPost("vote")]
    public async Task<IActionResult> Vote(VoteDto vote) =>
        Ok(new { message = await _service.VoteAsync(vote) });
}