using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Models.Requests;
using TodoApp.Api.Models.Responses;
using TodoApp.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Tests.Controllers;

public class TaskControllerTests
{
    private readonly AppDbContext _context;
    private readonly TaskController _controller;

    public TaskControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new TaskController(_context);
    }

    [Fact]
    public async Task GetAll_ReturnsOkComLista()
    {
        var task1 = new TodoApp.Api.Models.Task { Id = Guid.NewGuid(), Descricao = "Task 1" };
        var task2 = new TodoApp.Api.Models.Task { Id = Guid.NewGuid(), Descricao = "Task 2" };
        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponse>>().Subject;
        tasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList()
    {
        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponse>>().Subject;
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var taskId = Guid.NewGuid();
        var task = new TodoApp.Api.Models.Task { Id = taskId, Descricao = "Test Task" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(taskId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TaskResponse>().Subject;
        response.Id.Should().Be(taskId);
        response.Descricao.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var request = new TaskRequest { Descricao = "New Task" };

        var result = await _controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<TaskResponse>().Subject;
        response.Descricao.Should().Be("New Task");
        response.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_ValidRequest_AddsToDatabase()
    {
        var request = new TaskRequest { Descricao = "New Task" };

        await _controller.Create(request);

        var tasks = await _context.Tasks.ToListAsync();
        tasks.Should().HaveCount(1);
        tasks.First().Descricao.Should().Be("New Task");
    }

    [Fact]
    public async Task Update_ExistingTask_ReturnsOk()
    {
        var taskId = Guid.NewGuid();
        var task = new TodoApp.Api.Models.Task { Id = taskId, Descricao = "Old Description" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new TaskRequest { Descricao = "Updated Description" };

        var result = await _controller.Update(taskId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TaskResponse>().Subject;
        response.Descricao.Should().Be("Updated Description");
    }

    [Fact]
    public async Task Update_NonExistingTask_ReturnsNotFound()
    {
        var request = new TaskRequest { Descricao = "Updated Description" };

        var result = await _controller.Update(Guid.NewGuid(), request);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ExistingTask_ReturnsNoContent()
    {
        var taskId = Guid.NewGuid();
        var task = new TodoApp.Api.Models.Task { Id = taskId, Descricao = "Task to Delete" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _controller.Delete(taskId);

        result.Should().BeOfType<NoContentResult>();
        var tasks = await _context.Tasks.ToListAsync();
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NonExistingTask_ReturnsNotFound()
    {
        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }
}
