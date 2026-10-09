using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using PhotoContestWebsite.Models;
using PhotoContestWebsiteFinal.Models;

namespace PhotoContestWebsite.Controllers
{
    public class AdminController : Controller
    {
        // Toggle the submission status (active/inactive)
        [HttpPost]
        public IActionResult ToggleSubmissionStatus()
        {
            // Toggle the submission status
            PictureRepository.IsSubmissionActive = !PictureRepository.IsSubmissionActive;

            // Save the updated status
            PictureRepository.SaveSubmissionStatus();

            // Return the new status with a message
            string message = PictureRepository.IsSubmissionActive ? "Submissions are now active." : "Submissions have been closed.";
            return Json(new { success = true, isActive = PictureRepository.IsSubmissionActive, message });
        }

        // Get the current submission status
        [HttpGet]
        public IActionResult GetSubmissionStatus()
        {
            return Json(new { isActive = PictureRepository.IsSubmissionActive });
        }

        // Toggle the contest status
        [HttpPost]
        public IActionResult ToggleContest()
        {
            // Toggle the contest status
            PictureRepository.IsContestActive = !PictureRepository.IsContestActive;

            // Save the updated status to persist between sessions
            PictureRepository.Save();

            string message = PictureRepository.IsContestActive ? "The contest has started." : "The contest has ended.";
            return Json(new { success = true, message });
        }

        // Get the current contest status
        [HttpGet]
        public IActionResult GetContestStatusJson()
        {
            // Return the contest status as JSON
            return Json(new { isActive = PictureRepository.IsContestActive });
        }

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

        public IActionResult Index(string category, bool? isDisplayed, bool? isWinner, DateTime? fromDate, DateTime? toDate, bool sortByLikes = false)
        {
            // Base list of pictures
            var pictures = PictureRepository.Pictures.AsQueryable();

            // Apply filter for category
            if (!string.IsNullOrEmpty(category))
            {
                pictures = pictures.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            // Apply filter for display status
            if (isDisplayed.HasValue)
            {
                pictures = pictures.Where(p => p.IsDisplayed == isDisplayed.Value);
            }

            // Apply filter for winner status
            if (isWinner.HasValue)
            {
                pictures = pictures.Where(p => p.IsWinner == isWinner.Value);
            }

            // Apply filter for submission date range
            if (fromDate.HasValue)
            {
                pictures = pictures.Where(p => p.SubmissionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                pictures = pictures.Where(p => p.SubmissionDate <= toDate.Value);
            }

            // Sort by likes if requested, otherwise by category
            if (sortByLikes)
            {
                pictures = pictures.OrderByDescending(p => p.Likes);
            }
            else
            {
                pictures = pictures.OrderBy(p => p.Category);
            }

            // Pass the contest status to the view
            ViewData["IsContestActive"] = PictureRepository.IsContestActive;

            return View(pictures.ToList());
        }

        [HttpPost]
        public IActionResult ToggleDisplay(int id)
        {
            var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == id);
            if (picture != null)
            {
                picture.IsDisplayed = !picture.IsDisplayed;
                PictureRepository.Save();
                return Json(new { success = true, isDisplayed = picture.IsDisplayed });
            }
            return Json(new { success = false, message = "Picture not found" });
        }

        [HttpPost]
        public IActionResult ToggleWinner(int id)
        {
            var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == id);
            if (picture != null)
            {
                picture.IsWinner = !picture.IsWinner;
                PictureRepository.Save();
                return Json(new { success = true, isWinner = picture.IsWinner });
            }
            return Json(new { success = false, message = "Picture not found" });
        }

[HttpGet]
        public IActionResult GetPhotoDetails(int id)
        {
            var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == id);
            if (picture != null)
            {
                return Json(picture);
            }
            return Json(new { success = false, message = "Picture not found" });
        }

        [HttpPost]
        public IActionResult EditPhoto([FromBody] EditPhotoModel model)
        {
            var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == model.Id);
            if (picture != null)
            {
                picture.Title = model.Title;
                picture.Description = model.Description;
                picture.Category = model.Category;
                PictureRepository.Save();
                return Json(new { success = true, message = "Photo updated successfully" });
            }
            return Json(new { success = false, message = "Picture not found" });
        }

        public class EditPhotoModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
        }

        [HttpPost]
        public IActionResult UploadBulkPhotos(IFormFileCollection imageFiles, string category)
        {
            if (imageFiles != null && imageFiles.Count > 0)
            {
                // Validate category
                var validCategories = new List<string>
                {
                    "landscape photography", "nature photography", "portrait photography",
                    "street photography", "event photography", "wedding photography", "product photography"
                };

                if (string.IsNullOrWhiteSpace(category) || !validCategories.Contains(category.ToLowerInvariant()))
                {
                    category = "Uncategorized"; // Default category if invalid
                }

                var imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imageDirectoryPath))
                {
                    Directory.CreateDirectory(imageDirectoryPath);
                }

                foreach (var imageFile in imageFiles)
                {
                    // Ensure unique filename to avoid overwriting
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imageDirectoryPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    // Create a new Picture entry for each file
                    var newPicture = new Picture
                    {
                        Id = Guid.NewGuid().GetHashCode(), // Generate unique ID
                        Title = Path.GetFileNameWithoutExtension(imageFile.FileName),
                        Description = "Uploaded via bulk upload",
                        Category = category,
                        ImageUrl = $"/images/{uniqueFileName}",
                        Likes = 0,
                        IsDisplayed = false,
                        IsWinner = false,
                        AdminTitle = null,
                        AdminDescription = null,
                        SubmissionDate = DateTime.UtcNow
                    };

                    PictureRepository.Pictures.Add(newPicture);
                }

                PictureRepository.Save(); // Save all changes
                TempData["Message"] = $"{imageFiles.Count} photos uploaded successfully to the '{category}' category!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "No files selected for upload.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteAllPhotos()
        {
            try
            {
                // Optionally, delete all the image files from the server if they exist
                foreach (var picture in PictureRepository.Pictures)
                {
                    var filePath = Path.Combine("wwwroot", picture.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Clear all pictures from the repository
                PictureRepository.Pictures.Clear();
                PictureRepository.Save(); // Save changes to the repository

                TempData["Message"] = "All pictures have been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting all pictures: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            try
            {
                var picture = PictureRepository.Pictures.FirstOrDefault(p => p.Id == id);
                if (picture != null)
                {
                    // Remove the picture from the list
                    PictureRepository.Pictures.Remove(picture);

                    // Delete the associated image file
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", picture.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Save the updated list to JSON
                    PictureRepository.Save();

                    return Json(new { success = true, message = "Photo deleted successfully." });
                }

                return Json(new { success = false, message = "Photo not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ToggleAllDisplays(bool display)
        {
            try
            {
                foreach (var picture in PictureRepository.Pictures)
                {
                    picture.IsDisplayed = display;
                }
                PictureRepository.Save(); // Save the changes to the repository

                return Json(new { success = true, message = display ? "All pictures are now displayed." : "All pictures are now hidden." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult SetWinnersByLikes()
        {
            try
            {
                var groupedPictures = PictureRepository.Pictures.GroupBy(p => p.Category).ToList();

                foreach (var group in groupedPictures)
                {
                    // Get the highest likes
                    var maxLikes = group.Max(p => p.Likes);
                    var topPictures = group.Where(p => p.Likes == maxLikes).ToList();

                    if (topPictures.Count > 1)
                    {
                        // Log or handle ties if needed
                        Console.WriteLine($"Tie detected in category '{group.Key}': {topPictures.Count} pictures have {maxLikes} likes.");
                    }

                    // Select the winner (use SubmissionDate as a tiebreaker)
                    var winner = topPictures
                        .OrderBy(p => p.SubmissionDate) // Secondary criterion
                        .FirstOrDefault();

                    if (winner != null)
                    {
                        winner.IsWinner = true;

                        // Set others as non-winners
                        foreach (var picture in group)
                        {
                            if (picture.Id != winner.Id)
                            {
                                picture.IsWinner = false;
                            }
                        }
                    }
                }

                PictureRepository.Save();

                return Json(new { success = true, message = "Winners set successfully based on likes." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult RemoveAllWinners()
        {
            try
            {
                // Set `IsWinner` to false for all pictures
                foreach (var picture in PictureRepository.Pictures)
                {
                    picture.IsWinner = false;
                }

                PictureRepository.Save(); // Save the changes to the repository

                return Json(new { success = true, message = "All winners have been removed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

    }

}