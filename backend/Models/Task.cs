namespace TodoApp.Api.Models;

public class Task
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
