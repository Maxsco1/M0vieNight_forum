using Forum.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure;

public sealed class ForumDbContext(DbContextOptions<ForumDbContext> options) : DbContext(options)
{
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>()
        .HasMany(p => p.Comments)
        .WithOne(c => c.Post)
        .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Comment>()
        .HasOne(c => c.Post)
        .WithMany(p => p.Comments)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
        .HasOne<Comment>()
        .WithMany(p => p.Replies)
        .HasForeignKey("ParentCommentId")
        .OnDelete(DeleteBehavior.Restrict);
    }
}