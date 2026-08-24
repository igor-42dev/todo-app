using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TodoApp.Api.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<TodoApp.Api.Models.Item>
{
    public void Configure(EntityTypeBuilder<TodoApp.Api.Models.Item> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Descricao).IsRequired().HasMaxLength(255);
        builder.Property(i => i.Concluido).HasDefaultValue(false);
        builder.HasOne(i => i.Task).WithMany().HasForeignKey(i => i.TaskId);
    }
}
