namespace TodoApp.Api.Models.Requests;

public class ItemRequest
{
    public string Descricao { get; set; } = string.Empty;
    public bool Concluido { get; set; }
    public Guid TaskId { get; set; }
}
