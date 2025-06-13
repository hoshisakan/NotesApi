using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;
using NotesApi.Services.IService;

namespace NotesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly ILogger<NotesController> _logger;
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService, ILogger<NotesController> logger)
    {
        _logger = logger;
        _noteService = noteService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Note>>> GetNotes()
    {
        _logger.LogInformation("這是一筆從 Controller 發出的 Log 訊息！");
        return await _noteService.GetAllNotesAsync();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetNote(int id)
    {
        var note = await _noteService.GetNoteByIdAsync(id);
        return note == null ? NotFound() : note;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Note>> CreateNote(Note note)
    {
        // 取得目前登入者的 User Id
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        note.UserId = int.Parse(userIdClaim.Value);

        await _noteService.CreateNoteAsync(note);

        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    [Authorize]  // 需要驗證
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] Note updatedNote)
    {
        if (id != updatedNote.Id)
        return BadRequest("Note ID mismatch.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var result = await _noteService.UpdateNoteAsync(userId, updatedNote);

        if (!result)
            return NotFound();

        return NoContent();
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var result = await _noteService.DeleteNoteAsync(id, userId);
        if (!result)
            return NotFound();

        return NoContent(); // 204 成功刪除
    }
}
