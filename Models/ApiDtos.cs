using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    // ---- Requests ----
    public class EventDto
    {
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string Description { get; set; } = string.Empty;
        [Required] public DateTime Date { get; set; }
        [Required, MaxLength(150)] public string Location { get; set; } = string.Empty;
        [Range(0.1, 1000)] public double DistanceKm { get; set; }
        [Required] public EventType EventType { get; set; }
    }

    public class CategoryDto
    {
        [Required, MaxLength(50)] public string Name { get; set; } = string.Empty;
    }

    public class EnrolDto
    {
        [Required] public int CategoryId { get; set; }
    }

    public class ResultDto
    {
        [Required] public int EnrolmentId { get; set; }
        [Required] public TimeSpan FinishTime { get; set; }  
        [Range(1, int.MaxValue)] public int Position { get; set; }
    }

    public class UpdateResultDto
    {
        [Required] public TimeSpan FinishTime { get; set; }
        [Range(1, int.MaxValue)] public int Position { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
        public RaceDay.Data.Gender Gender { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
    }

    // ---- Responses ----
    public record CategoryResponse(int Id, string Name);

    public record EventResponse(int Id, string Name, string Description, DateTime Date,
        string Location, double DistanceKm, EventType EventType, string? OrganiserId,
        List<CategoryResponse> Categories);

    public record EnrolmentResponse(int Id, int EventId, string EventName, int CategoryId,
        string CategoryName, string ParticipantId, string ParticipantName, DateTime EnrolledOn);

    public record ResultResponse(int Id, int EnrolmentId, int EventId, string EventName,
        string ParticipantName, string CategoryName, TimeSpan FinishTime, int Position);

    public record ProfileResponse(string Id, string Email, string FirstName, string LastName,
        RaceDay.Data.Gender Gender, string? PhoneNumber, IList<string> Roles);
}
