using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PhotoContestWebsite.Models;
using PhotoContestWebsiteFinal.Models;


namespace PhotoContestWebsite.Controllers
{
    public class HomeController : Controller
    {
        // Category mapping from query strings to full category names
        private Dictionary<string, string> categoryMapping = new Dictionary<string, string>
        {
            { "landscape", "landscape photography" },
            { "nature", "nature photography" },
            { "portrait", "portrait photography" },
            { "street", "street photography" },
            { "event", "event photography" },
            { "wedding", "wedding photography" },
            { "product", "product photography" }
        };

        // Home page
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contest(string category)
        {
            var normalizedCategory = category.ToLowerInvariant().Trim();
            var fullCategory = categoryMapping.ContainsKey(normalizedCategory)
                ? categoryMapping[normalizedCategory]
                : null;

            // Set category-specific banner information
            if (fullCategory == "landscape photography")
            {
                ViewData["BannerImage"] = "/images/home page images/landscapebanner.jpg"; // Adjust path accordingly
                ViewData["BannerText"] = "Explore the beauty of nature through breathtaking landscapes.";
            }
            else if (fullCategory == "nature photography")
            {
                ViewData["BannerImage"] = "/images/home page images/animalbanner.jpg";
                ViewData["BannerText"] = "Immerse yourself in the wonders of nature captured in every shot.";
            }
            else if (fullCategory == "portrait photography")
            {
                ViewData["BannerImage"] = "/images/home page images/portraitbanner.jpg";
                ViewData["BannerText"] = "Capture the essence of humanity through stunning portraits.";
            }
            else if (fullCategory == "street photography")
            {
                ViewData["BannerImage"] = "/images/home page images/streetbanner.jpg";
                ViewData["BannerText"] = "The vibrant pulse of life, captured on the streets.";
            }
            else if (fullCategory == "event photography")
            {
                ViewData["BannerImage"] = "/images/home page images/eventbanner.jpg";
                ViewData["BannerText"] = "Freeze the moments of excitement and celebration at events.";
            }
            else if (fullCategory == "wedding photography")
            {
                ViewData["BannerImage"] = "/images/home page images/weddingbanner.jpg";
                ViewData["BannerText"] = "Cherish the most special day with timeless wedding photography.";
            }
            else if (fullCategory == "product photography")
            {
                ViewData["BannerImage"] = "/images/home page images/productbanner.jpg";
                ViewData["BannerText"] = "Highlighting the finest details of your product through the lens.";
            }
            else
            {
                // Default category if none of the above
                ViewData["BannerImage"] = "/images/default-banner.jpg";
                ViewData["BannerText"] = "Explore the world of photography.";
            }

            var filteredPictures = PictureRepository.Pictures
                .Where(p => p.Category.Equals(fullCategory, StringComparison.OrdinalIgnoreCase) && p.IsDisplayed)
                .ToList();

            ViewData["Category"] = fullCategory ?? "Default Category";

            return View(filteredPictures);
        }


        // Display pictures marked as winners
        public IActionResult Winners()
        {
            // Get all pictures from the repository
            var pictures = PictureRepository.Pictures;

            // Group the pictures by category
            var groupedPictures = pictures
                .GroupBy(p => p.Category) // Group by the category
                .ToList(); // Convert to a list of groups

            return View(groupedPictures);  // Pass the grouped pictures to the view
        }



        // Picture submission page
        public IActionResult SubmitPicture()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitPicture(Picture picture, IFormFile imageFile)
        {
            try
            {
                if (imageFile != null)
                {
                    var imageDirectoryPath = Path.Combine("wwwroot", "images");

                    if (!Directory.Exists(imageDirectoryPath))
                    {
                        Directory.CreateDirectory(imageDirectoryPath);
                    }

                    var filePath = Path.Combine(imageDirectoryPath, imageFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    picture.ImageUrl = $"/images/{imageFile.FileName}";
                }

                var normalizedCategory = picture.Category.ToLowerInvariant().Trim();
                picture.Category = categoryMapping.ContainsKey(normalizedCategory)
                    ? categoryMapping[normalizedCategory]
                    : "Uncategorized";

                // Generate a unique ID
                picture.Id = Guid.NewGuid().GetHashCode();

                // Initialize other properties
                picture.IsDisplayed = false;
                picture.IsWinner = false;

                // Add the new picture to the repository
                PictureRepository.Pictures.Add(picture);
                PictureRepository.Save(); // Persist changes

                return Json(new { success = true, message = "Picture submitted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // Voting for a picture
        [HttpPost]
        public IActionResult LikePicture([FromBody] int id)
        {
            if (!PictureRepository.IsContestActive)
            {
                return Json(new { success = false, message = "The contest has ended. Voting is disabled." });
            }

            var votedPictures = Request.Cookies["VotedPictures"];
            var votedList = new List<string>();

            if (!string.IsNullOrEmpty(votedPictures))
            {
                votedList = votedPictures.Split(',').ToList();
            }

            if (votedList.Contains(id.ToString()))
            {
                return Json(new { success = false, message = "You have already voted for this picture." });
            }

            var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == id);
            if (picture != null)
            {
                PictureRepository.IncrementLikes(id);
                PictureRepository.Save();

                votedList.Add(id.ToString());
                Response.Cookies.Append("VotedPictures", string.Join(",", votedList), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });

                return Json(new { success = true, message = "Thank you for voting!", likes = picture.Likes });
            }

            return Json(new { success = false, message = "Picture not found." });
        }



        // Picture details page
        public IActionResult PictureDetails(int id)
        {
            var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == id);
            if (picture == null)
            {
                return NotFound();
            }

            return View(picture); // Ensure this returns a single picture
        }

    }
}
