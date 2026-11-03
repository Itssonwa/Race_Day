namespace RaceDay.Models.ViewModels
{
    public class UserRolesViewModel
    {
        public string ParticipantId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string> ();
    }
}
