using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data.Configurations;
using TodoApp.Api.Models;

namespace TodoApp.Api.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<TodoApp.Api.Models.Task> Tasks { get; set; } = null!;
    public DbSet<TodoApp.Api.Models.Item> Items { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}
