using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzy.API.Data;
using Quizzy.API.Models;

namespace Quizzy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(AppDbContext db) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var media = await db.MediaFiles.FindAsync(id);
        if (media == null) return NotFound();
        return File(media.Data, media.ContentType, media.FileName);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<object>> Upload(IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { error = "Empty file." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "audio/mpeg", "audio/wav", "audio/ogg" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Unsupported file type." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var media = new MediaFile
        {
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Data = ms.ToArray()
        };

        db.MediaFiles.Add(media);
        await db.SaveChangesAsync();

        return Ok(new { media.Id, media.FileName, media.ContentType });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var media = await db.MediaFiles.FindAsync(id);
        if (media == null) return NotFound();

        // Check if media is referenced by any question or answer option
        var isReferenced = await db.Questions.AnyAsync(q => q.MediaId == id) ||
                           await db.AnswerOptions.AnyAsync(a => a.MediaId == id);
        if (isReferenced)
            return BadRequest(new { error = "Media is still referenced by questions or answer options." });

        db.MediaFiles.Remove(media);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
