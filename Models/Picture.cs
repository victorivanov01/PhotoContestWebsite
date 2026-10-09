namespace PhotoContestWebsite.Models
{
    public class Picture
    {
        public int Id { get; set; } // Unique identifier for the picture
        public string Title { get; set; } // Title provided during submission
        public string Description { get; set; } // Description provided during submission
        public string Category { get; set; } // Category of the picture
        public string ImageUrl { get; set; } // URL of the picture
        public int Likes { get; set; } // Number of likes the picture has received
        public bool IsDisplayed { get; set; } // Determines if the picture is displayed in categories
        public bool IsWinner { get; set; } // Determines if the picture is marked as a winner
        public string? AdminTitle { get; set; } // Optional custom title set by the admin
        public string? AdminDescription { get; set; } // Optional custom description set by the admin
        public DateTime SubmissionDate { get; set; } // Date and time when the picture was submitted
    }
}
