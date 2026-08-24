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
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaskController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _context.Tasks
            .Select(t => new TaskResponse
            {
                Id = t.Id,
                Descricao = t.Descricao
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        var response = new TaskResponse
        {
            Id = task.Id,
            Descricao = task.Descricao
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskRequest request)
    {
        var task = new TodoApp.Api.Models.Task
        {
            Id = Guid.NewGuid(),
            Descricao = request.Descricao
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var response = new TaskResponse
        {
            Id = task.Id,
            Descricao = task.Descricao
        };

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskRequest request)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        task.Descricao = request.Descricao;
        await _context.SaveChangesAsync();

        var response = new TaskResponse
        {
            Id = task.Id,
            Descricao = task.Descricao
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
