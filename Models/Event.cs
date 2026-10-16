using RaceDay.Data;

namespace RaceDay.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? OrganiserId { get; set; }
    }

    public class Enrolment
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
    }
}
