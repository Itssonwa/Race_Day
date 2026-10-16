using RaceDay.Data;

namespace RaceDay.Models
{
    public enum EventType { Run, Walk, Cycle }

    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public EventType EventType { get; set; }
        public string? OrganiserId { get; set; }
        public List<Category> Categories { get; set; } = new();
    }

    public class Category
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;   // e.g. "Under 20", "21km"
    }

    public class Enrolment
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
    }

    public class Result
    {
        public int Id { get; set; }
        public int EnrolmentId { get; set; }
        public Enrolment? Enrolment { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }
}
