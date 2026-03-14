using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzy.API.DTOs;
using Quizzy.API.Services;

namespace Quizzy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizzesController(QuizService quizService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<QuizListDto>>> GetAll()
    {
        var isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";
        return Ok(isAdmin ? await quizService.GetAll() : await quizService.GetActive());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuizDetailDto>> GetById(int id)
    {
        try
        {
            var isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";
            return Ok(await quizService.GetById(id, isAdmin));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/play")]
    [Authorize]
    public async Task<ActionResult<List<QuestionPlayDto>>> GetForPlay(int id)
    {
        try
        {
            return Ok(await quizService.GetQuestionsForPlay(id));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuizListDto>> Create(CreateQuizRequest request)
    {
        return Ok(await quizService.Create(request));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuizListDto>> Update(int id, UpdateQuizRequest request)
    {
        try
        {
            return Ok(await quizService.Update(id, request));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await quizService.Delete(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
