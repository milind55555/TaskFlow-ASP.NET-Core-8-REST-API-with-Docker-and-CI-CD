using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Email).HasMaxLength(256);

        b.Entity<Project>().Property(p => p.Name).HasMaxLength(200);
        b.Entity<Project>()
            .HasOne(p => p.Owner).WithMany(u => u.Projects)
            .HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<TaskItem>().Property(t => t.Title).HasMaxLength(200);
        b.Entity<TaskItem>()
            .HasOne(t => t.Project).WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}
