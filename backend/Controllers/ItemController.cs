using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Models;
using TodoApp.Api.Models.Requests;
using TodoApp.Api.Models.Responses;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItemController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? taskId)
    {
        var query = _context.Items.AsQueryable();

        if (taskId.HasValue)
            query = query.Where(i => i.TaskId == taskId.Value);

        var items = await query
            .Select(i => new ItemResponse
            {
                Id = i.Id,
                Descricao = i.Descricao,
                Concluido = i.Concluido,
                TaskId = i.TaskId
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        var response = new ItemResponse
        {
            Id = item.Id,
            Descricao = item.Descricao,
            Concluido = item.Concluido,
            TaskId = item.TaskId
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemRequest request)
    {
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == request.TaskId);
        if (!taskExists)
            return BadRequest(new { message = "Task not found." });

        var item = new TodoApp.Api.Models.Item
        {
            Id = Guid.NewGuid(),
            Descricao = request.Descricao,
            Concluido = request.Concluido,
            TaskId = request.TaskId
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var response = new ItemResponse
        {
            Id = item.Id,
            Descricao = item.Descricao,
            Concluido = item.Concluido,
            TaskId = item.TaskId
        };

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ItemRequest request)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == request.TaskId);
        if (!taskExists)
            return BadRequest(new { message = "Task not found." });

        item.Descricao = request.Descricao;
        item.Concluido = request.Concluido;
        item.TaskId = request.TaskId;
        await _context.SaveChangesAsync();

        var response = new ItemResponse
        {
            Id = item.Id,
            Descricao = item.Descricao,
            Concluido = item.Concluido,
            TaskId = item.TaskId
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
