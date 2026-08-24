using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Models.Requests;
using TodoApp.Api.Models.Responses;
using TodoApp.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Tests.Controllers;

public class ItemControllerTests
{
    private readonly AppDbContext _context;
    private readonly ItemController _controller;

    public ItemControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ItemController(_context);
    }

    private async System.Threading.Tasks.Task<TodoApp.Api.Models.Task> CreateTask(string descricao = "Test Task")
    {
        var task = new TodoApp.Api.Models.Task { Id = Guid.NewGuid(), Descricao = descricao };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    [Fact]
    public async Task GetAll_ReturnsOkComLista()
    {
        var task = await CreateTask();
        var item1 = new TodoApp.Api.Models.Item { Id = Guid.NewGuid(), Descricao = "Item 1", TaskId = task.Id };
        var item2 = new TodoApp.Api.Models.Item { Id = Guid.NewGuid(), Descricao = "Item 2", TaskId = task.Id };
        _context.Items.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAll(null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<ItemResponse>>().Subject;
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithTaskIdFilter_ReturnsFilteredList()
    {
        var task1 = await CreateTask("Task 1");
        var task2 = await CreateTask("Task 2");
        var item1 = new TodoApp.Api.Models.Item { Id = Guid.NewGuid(), Descricao = "Item 1", TaskId = task1.Id };
        var item2 = new TodoApp.Api.Models.Item { Id = Guid.NewGuid(), Descricao = "Item 2", TaskId = task2.Id };
        _context.Items.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAll(task1.Id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<ItemResponse>>().Subject;
        items.Should().HaveCount(1);
        items.First().TaskId.Should().Be(task1.Id);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var result = await _controller.GetAll(null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<ItemResponse>>().Subject;
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var task = await CreateTask();
        var itemId = Guid.NewGuid();
        var item = new TodoApp.Api.Models.Item { Id = itemId, Descricao = "Test Item", Concluido = true, TaskId = task.Id };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(itemId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ItemResponse>().Subject;
        response.Id.Should().Be(itemId);
        response.Descricao.Should().Be("Test Item");
        response.Concluido.Should().BeTrue();
        response.TaskId.Should().Be(task.Id);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ValidTaskId_ReturnsCreatedAtAction()
    {
        var task = await CreateTask();
        var request = new ItemRequest { Descricao = "New Item", Concluido = false, TaskId = task.Id };

        var result = await _controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ItemResponse>().Subject;
        response.Descricao.Should().Be("New Item");
        response.TaskId.Should().Be(task.Id);
    }

    [Fact]
    public async Task Create_ValidTaskId_AddsToDatabase()
    {
        var task = await CreateTask();
        var request = new ItemRequest { Descricao = "New Item", Concluido = false, TaskId = task.Id };

        await _controller.Create(request);

        var items = await _context.Items.ToListAsync();
        items.Should().HaveCount(1);
        items.First().Descricao.Should().Be("New Item");
    }

    [Fact]
    public async Task Create_InvalidTaskId_ReturnsBadRequest()
    {
        var request = new ItemRequest { Descricao = "New Item", Concluido = false, TaskId = Guid.NewGuid() };

        var result = await _controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ExistingItem_ReturnsOk()
    {
        var task = await CreateTask();
        var itemId = Guid.NewGuid();
        var item = new TodoApp.Api.Models.Item { Id = itemId, Descricao = "Old Description", Concluido = false, TaskId = task.Id };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var request = new ItemRequest { Descricao = "Updated Description", Concluido = true, TaskId = task.Id };

        var result = await _controller.Update(itemId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ItemResponse>().Subject;
        response.Descricao.Should().Be("Updated Description");
        response.Concluido.Should().BeTrue();
    }

    [Fact]
    public async Task Update_NonExistingItem_ReturnsNotFound()
    {
        var task = await CreateTask();
        var request = new ItemRequest { Descricao = "Updated", Concluido = false, TaskId = task.Id };

        var result = await _controller.Update(Guid.NewGuid(), request);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_InvalidTaskId_ReturnsBadRequest()
    {
        var task = await CreateTask();
        var itemId = Guid.NewGuid();
        var item = new TodoApp.Api.Models.Item { Id = itemId, Descricao = "Item", Concluido = false, TaskId = task.Id };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var request = new ItemRequest { Descricao = "Updated", Concluido = false, TaskId = Guid.NewGuid() };

        var result = await _controller.Update(itemId, request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ExistingItem_ReturnsNoContent()
    {
        var task = await CreateTask();
        var itemId = Guid.NewGuid();
        var item = new TodoApp.Api.Models.Item { Id = itemId, Descricao = "Item to Delete", Concluido = false, TaskId = task.Id };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var result = await _controller.Delete(itemId);

        result.Should().BeOfType<NoContentResult>();
        var items = await _context.Items.ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NonExistingItem_ReturnsNotFound()
    {
        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }
}
