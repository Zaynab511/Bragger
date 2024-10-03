using System.ComponentModel.DataAnnotations;

namespace Braggerbk.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Name { get; set; }
        public string JobTitle { get; set; }
        public string Industry { get; set; }
        public string Contact { get; set; }
    }

}