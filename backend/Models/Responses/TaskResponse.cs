namespace TodoApp.Api.Models.Responses;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
