namespace TodoApp.Api.Models.Responses;

public class ItemResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Concluido { get; set; }
    public Guid TaskId { get; set; }
}
