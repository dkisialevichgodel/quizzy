using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzy.API.DTOs;
using Quizzy.API.Services;

namespace Quizzy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttemptsController(AttemptService attemptService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("start")]
    public async Task<ActionResult<AttemptResultDto>> Start(StartAttemptRequest request)
    {
        try
        {
            return Ok(await attemptService.StartAttempt(UserId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/answer")]
    public async Task<ActionResult<AttemptAnswerDto>> SubmitAnswer(int id, SubmitAnswerRequest request)
    {
        try
        {
            return Ok(await attemptService.SubmitAnswer(UserId, id, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<AttemptResultDto>> Complete(int id)
    {
        try
        {
            return Ok(await attemptService.CompleteAttempt(UserId, id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttemptResultDto>> GetResult(int id)
    {
        try
        {
            return Ok(await attemptService.GetAttemptResult(UserId, id));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<AttemptHistoryDto>>> GetHistory()
    {
        return Ok(await attemptService.GetHistory(UserId));
    }
}
