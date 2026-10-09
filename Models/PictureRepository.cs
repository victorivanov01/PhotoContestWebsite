using PhotoContestWebsite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PhotoContestWebsiteFinal.Models
{
    public static class PictureRepository
    {
        public static List<Picture> Pictures { get; private set; } = new List<Picture>();

        // Default statuses
        public static bool IsContestActive { get; set; } = true; // Default to contest active
        public static bool IsSubmissionActive { get; set; } = true; // Default to submission active

        private static string DataFilePath => Path.Combine(Directory.GetCurrentDirectory(), "Data", "pictures.json");
        private static string ContestStatusFilePath => Path.Combine(Directory.GetCurrentDirectory(), "Data", "contestStatus.json");
        private static string SubmissionStatusFilePath => Path.Combine(Directory.GetCurrentDirectory(), "Data", "submissionStatus.json");

        static PictureRepository()
        {
            Load();
        }

        // Load pictures and statuses
        public static void Load()
        {
            try
            {
                // Load pictures
                if (File.Exists(DataFilePath))
                {
                    var jsonData = File.ReadAllText(DataFilePath);
                    Pictures = JsonSerializer.Deserialize<List<Picture>>(jsonData) ?? new List<Picture>();
                }

                // Load contest status
                if (File.Exists(ContestStatusFilePath))
                {
                    var statusData = File.ReadAllText(ContestStatusFilePath);
                    IsContestActive = JsonSerializer.Deserialize<bool>(statusData);
                }

                // Load submission status
                if (File.Exists(SubmissionStatusFilePath))
                {
                    var submissionStatusData = File.ReadAllText(SubmissionStatusFilePath);
                    IsSubmissionActive = JsonSerializer.Deserialize<bool>(submissionStatusData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        // Save pictures and statuses
        public static void Save()
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(Pictures, new JsonSerializerOptions { WriteIndented = true });
                var directory = Path.GetDirectoryName(DataFilePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(DataFilePath, jsonData);

                SaveContestStatus();
                SaveSubmissionStatus();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        // Save contest status
        public static void SaveContestStatus()
        {
            try
            {
                var statusData = JsonSerializer.Serialize(IsContestActive);
                File.WriteAllText(ContestStatusFilePath, statusData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving contest status: {ex.Message}");
            }
        }

        // Save submission status
        public static void SaveSubmissionStatus()
        {
            try
            {
                var submissionStatusData = JsonSerializer.Serialize(IsSubmissionActive);
                File.WriteAllText(SubmissionStatusFilePath, submissionStatusData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving submission status: {ex.Message}");
            }
        }

        // Increment likes for a picture
        public static void IncrementLikes(int pictureId)
        {
            try
            {
                var picture = Pictures.FirstOrDefault(p => p.Id == pictureId);
                if (picture != null)
                {
                    picture.Likes++;
                    Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error incrementing likes for Picture ID: {pictureId}: {ex.Message}");
            }
        }

        // Get pictures sorted by likes
        public static List<Picture> GetPicturesSortedByLikes()
        {
            try
            {
                return Pictures.OrderByDescending(p => p.Likes).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving pictures sorted by likes: {ex.Message}");
                return new List<Picture>();
            }
        }
    }
}
