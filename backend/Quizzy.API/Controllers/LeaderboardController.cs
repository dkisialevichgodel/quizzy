using Microsoft.AspNetCore.Mvc;
using Quizzy.API.DTOs;
using Quizzy.API.Models;
using Quizzy.API.Services;

namespace Quizzy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController(LeaderboardService leaderboardService) : ControllerBase
{
    [HttpGet("{difficulty}")]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> Get(Difficulty difficulty)
    {
        return Ok(await leaderboardService.GetLeaderboard(difficulty));
    }
}
