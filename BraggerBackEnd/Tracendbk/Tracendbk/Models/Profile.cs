using System.ComponentModel.DataAnnotations;

namespace Braggerbk.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string JobTitle { get; set; }
        public string Industry { get; set; }
        public string Contact { get; set; }
        public string ProfilePicture { get; set; }
    }
}
