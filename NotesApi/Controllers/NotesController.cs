using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

namespace NotesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly NotesContext _context;

    public NotesController(NotesContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotes() =>
        await _context.Notes.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetNote(int id)
    {
        var note = await _context.Notes.FindAsync(id);
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

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    [Authorize]  // 需要驗證
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] Note updatedNote)
    {
        if (id != updatedNote.Id)
            return BadRequest("Note ID mismatch.");

        // 從 JWT 取得當前 UserId
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        // 找出欲更新的 Note 且必須是該使用者擁有的
        var note = await _context.Notes
            .Where(n => n.Id == id && n.UserId == userId)
            .FirstOrDefaultAsync();

        if (note == null)
            return NotFound();

        // 更新可修改欄位
        note.Title = updatedNote.Title;
        note.Content = updatedNote.Content;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Notes.Any(n => n.Id == id && n.UserId == userId))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }


    [Authorize]  // 需要驗證
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
