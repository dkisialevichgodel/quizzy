using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzy.API.DTOs;
using Quizzy.API.Services;

namespace Quizzy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class QuestionsController(QuestionService questionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(CreateQuestionRequest request)
    {
        return Ok(await questionService.Create(request));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionDto>> Update(int id, UpdateQuestionRequest request)
    {
        try
        {
            return Ok(await questionService.Update(id, request));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await questionService.Delete(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
