using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class EventDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }
    }
}
