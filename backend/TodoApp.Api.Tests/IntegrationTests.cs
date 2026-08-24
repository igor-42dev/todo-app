using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TodoApp.Api.Data;
using TodoApp.Api.Models.Requests;
using TodoApp.Api.Models.Responses;

namespace TodoApp.Api.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "6ba265a1ca9a4c988564e19e898d486c95427294b05f41f0bb0a4e228ed7b4ef",
                ["Jwt:Issuer"] = "TodoApp.Api",
                ["Jwt:Audience"] = "TodoApp.Frontend",
                ["Jwt:ExpirationInMinutes"] = "60"
            };

            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });
        });
    }
}

public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public IntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@email.com",
            Senha = "Admin@123"
        };

        var response = await _client.PostAsJsonAsync("/api/Login", loginRequest);

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Login failed with {response.StatusCode}: {body}");
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    private async Task AuthenticateClientAsync()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task FullFlow_Login_CreateTask_CreateItem_Update_Delete()
    {
        var token = await GetAuthTokenAsync();
        token.Should().NotBeEmpty();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var taskRequest = new TaskRequest { Descricao = "Integration Test Task" };
        var taskResponse = await _client.PostAsJsonAsync("/api/Task", taskRequest);
        taskResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTask = await taskResponse.Content.ReadFromJsonAsync<TaskResponse>();
        createdTask!.Descricao.Should().Be("Integration Test Task");

        var getTaskResponse = await _client.GetAsync($"/api/Task/{createdTask.Id}");
        getTaskResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedTask = await getTaskResponse.Content.ReadFromJsonAsync<TaskResponse>();
        fetchedTask!.Id.Should().Be(createdTask.Id);

        var getAllTasksResponse = await _client.GetAsync("/api/Task");
        getAllTasksResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var itemRequest = new ItemRequest
        {
            Descricao = "Integration Test Item",
            Concluido = false,
            TaskId = createdTask.Id
        };
        var itemResponse = await _client.PostAsJsonAsync("/api/Item", itemRequest);
        itemResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdItem = await itemResponse.Content.ReadFromJsonAsync<ItemResponse>();
        createdItem!.Descricao.Should().Be("Integration Test Item");
        createdItem.TaskId.Should().Be(createdTask.Id);

        var getItemResponse = await _client.GetAsync($"/api/Item/{createdItem.Id}");
        getItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getItemsByTaskResponse = await _client.GetAsync($"/api/Item?taskId={createdTask.Id}");
        getItemsByTaskResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateTaskRequest = new TaskRequest { Descricao = "Updated Task" };
        var updateTaskResponse = await _client.PutAsJsonAsync($"/api/Task/{createdTask.Id}", updateTaskRequest);
        updateTaskResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateItemRequest = new ItemRequest
        {
            Descricao = "Updated Item",
            Concluido = true,
            TaskId = createdTask.Id
        };
        var updateItemResponse = await _client.PutAsJsonAsync($"/api/Item/{createdItem.Id}", updateItemRequest);
        updateItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteItemResponse = await _client.DeleteAsync($"/api/Item/{createdItem.Id}");
        deleteItemResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteTaskResponse = await _client.DeleteAsync($"/api/Task/{createdTask.Id}");
        deleteTaskResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeletedTaskResponse = await _client.GetAsync($"/api/Task/{createdTask.Id}");
        getDeletedTaskResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateItem_InvalidTaskId_ReturnsBadRequest()
    {
        await AuthenticateClientAsync();

        var itemRequest = new ItemRequest
        {
            Descricao = "Item with invalid task",
            Concluido = false,
            TaskId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/Item", itemRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTask_NonExistingId_ReturnsNotFound()
    {
        await AuthenticateClientAsync();

        var response = await _client.GetAsync($"/api/Task/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unauthorized_Access_WithoutToken_Returns401()
    {
        var unauthorizedClient = _factory.CreateClient();

        var response = await unauthorizedClient.GetAsync("/api/Task");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
