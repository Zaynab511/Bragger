using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Braggerbk.Models
{
    public class Achievement
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public int UserId { get; set; }

        // Navigation property for the many-to-many relationship
        public List<Tag> AchievementTags { get; set; } = new List<Tag>(); // List of Tag objects
        public List<string> AiGeneratedTags { get; set; } = new List<string>(); // Default to an empty list
    }

    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string TagName { get; set; }
    }

    public class AchievementTag
    {
        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
