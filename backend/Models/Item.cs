namespace TodoApp.Api.Models;

public class Item
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Concluido { get; set; }
    public Guid TaskId { get; set; }
    public Task? Task { get; set; }
}
