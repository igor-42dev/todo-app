using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TodoApp.Api.Data.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TodoApp.Api.Models.Task>
{
    public void Configure(EntityTypeBuilder<TodoApp.Api.Models.Task> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Descricao).IsRequired().HasMaxLength(255);
    }
}
