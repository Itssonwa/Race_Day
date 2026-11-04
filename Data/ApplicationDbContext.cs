using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
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
                .Property(e => e.DistanceKm)
                .HasPrecision(8, 2);

            builder.Entity<Enrolment>()
                .HasIndex(e => new { e.EventId, e.ParticipantId })
                .IsUnique();

            builder.Entity<Enrolment>()
               .HasOne(e => e.Category)
               .WithMany(c => c.Enrolments)
               .HasForeignKey(e => e.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();

            builder.Entity<Event>()
               .HasOne(e => e.Organiser)
               .WithMany()
               .HasForeignKey(e => e.OrganiserId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany()
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
