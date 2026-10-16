using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(u => u.Gender)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Entity<Event>()
            .Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Entity<Enrolment>()
            .HasIndex(e => new { e.EventId, e.UserId })
            .IsUnique();

        builder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Result>()
            .HasIndex(r => r.EnrolmentId)
            .IsUnique();
    }
}