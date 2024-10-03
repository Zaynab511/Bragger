/*using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tracendbk.Models;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
namespace Tracendbk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly SqlConnection _conn;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        // Consolidated constructor
        public TagsController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAchievement([FromBody] Achievement achievement)
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            if (achievement == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid achievement data.");
            }

            try
            {
                await _conn.OpenAsync();

                // Fetch existing tags from the database
                var tagsResult = await GetTags();
                if (tagsResult is OkObjectResult okObjectResult)
                {
                    var existingTags = okObjectResult.Value as List<Tag>;
                    var existingTagNames = existingTags.Select(tag => tag.TagName).ToList();

                    // Separate user-selected tags
                    var selectedTags = achievement.AchievementTags
                        .Where(t => existingTagNames.Contains(t.TagName)) // Existing tags only
                        .Select(t => t.TagName)
                        .Distinct()
                        .ToList();

                    // AI-generated tags (optional)
                    var aiGeneratedTags = achievement.AiGeneratedTags?
                        .Where(t => !existingTagNames.Contains(t)) // Only keep new AI-generated tags
                        .Distinct()
                        .ToList();

                    // Create the achievement first
                    int achievementId = 0;
                    using (SqlCommand cmd = new SqlCommand("usp_InsertAchievement", _conn)) // Assume you have a stored procedure named usp_InsertAchievement
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Title", achievement.Title);
                        cmd.Parameters.AddWithValue("@Description", achievement.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Date", achievement.Date);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        // Get the achievement ID after insertion
                        achievementId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        Console.WriteLine($"Achievement created with ID: {achievementId}");
                    }

                    if (achievementId == 0)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create achievement.");
                    }

                    // Process AI-generated tags
                    if (aiGeneratedTags != null)
                    {
                        foreach (var aiTag in aiGeneratedTags)
                        {
                            await AddTagToDatabase(aiTag); // Ensure this method checks for duplicates
                        }
                    }

                    // Associate selected user tags and AI-generated tags
                    await AddTagsToAchievement(achievementId, selectedTags, aiGeneratedTags);

                    return Ok("Achievement created successfully.");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving tags: " + (tagsResult as ObjectResult)?.Value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating achievement: {e.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating achievement: {e.Message}");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    await _conn.CloseAsync(); // Close the connection only if it's open
                }
            }
        }

        // Helper method to associate user-selected tags and AI-generated tags
        private async Task AddTagsToAchievement(int achievementId, List<string> selectedTags, List<string> aiGeneratedTags)
        {
            var allTags = new List<string>();

            // Add user-selected tags
            if (selectedTags != null)
            {
                allTags.AddRange(selectedTags);
            }

            // Add AI-generated tags
            if (aiGeneratedTags != null)
            {
                allTags.AddRange(aiGeneratedTags);
            }

            foreach (var tagName in allTags.Distinct()) // Ensure uniqueness
            {
                int tagId;

                // Check if the tag exists
                using (SqlCommand cmd = new SqlCommand("usp_GetTagIdByName", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TagName", tagName);

                    tagId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                if (tagId > 0)
                {
                    // Associate the tag with the achievement
                    using (SqlCommand cmd = new SqlCommand("usp_AssociateTagWithAchievement", _conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AchievementId", achievementId);
                        cmd.Parameters.AddWithValue("@TagId", tagId);

                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                            Console.WriteLine($"Successfully associated TagId {tagId} with AchievementId {achievementId}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error associating TagId {tagId} with AchievementId {achievementId}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // If the tag doesn't exist, create it
                    await AddTagToDatabase(tagName);
                }
            }
        }

        // Helper method to add tags to the database if not already present
        private async Task AddTagToDatabase(string tagName)
        {
            using (SqlCommand cmd = new SqlCommand("usp_CreateTag", _conn)) // Ensure your stored procedure can handle duplicate checking
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TagName", tagName);
                try
                {
                    int tagId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    Console.WriteLine($"Tag '{tagName}' created with ID {tagId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating tag '{tagName}': {ex.Message}");
                }
            }
        }

        // Get the user ID from JWT token
        private int? GetUserIdFromToken()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        }
    }

}*/

/*
 * [HttpPost]
         [Route("Create")]
         public async Task<IActionResult> CreateAchievement([FromBody] Achievement achievement)
         {
             var userId = GetUserIdFromToken();

             if (userId == null)
             {
                 return Unauthorized("User not authenticated.");
             }

             if (achievement == null || !ModelState.IsValid)
             {
                 return BadRequest("Invalid achievement data.");
             }

             try
             {
                 await _conn.OpenAsync();

                 // Fetch existing tags from the database
                 var tagsResult = await GetTags();
                 if (tagsResult is OkObjectResult okObjectResult)
                 {
                     var existingTags = okObjectResult.Value as List<Tag>;
                     var existingTagNames = existingTags.Select(tag => tag.TagName).ToList();

                     // Separate user-selected tags
                     var selectedTags = achievement.AchievementTags
                         .Where(t => existingTagNames.Contains(t.TagName)) // Existing tags only
                         .Select(t => t.TagName)
                         .Distinct()
                         .ToList();

                     // AI-generated tags (optional)
                     var aiGeneratedTags = achievement.AiGeneratedTags?
                         .Where(t => !existingTagNames.Contains(t)) // Only keep new AI-generated tags
                         .Distinct()
                         .ToList();
        // Create the achievement first
                    int achievementId = 0;

                     using (SqlCommand cmd = new SqlCommand("usp_InsertAchievementWithTags", _conn))
                     {
                         cmd.CommandType = CommandType.StoredProcedure;
                         cmd.Parameters.AddWithValue("@Title", achievement.Title);
                         cmd.Parameters.AddWithValue("@Description", achievement.Description ?? (object)DBNull.Value);
                         cmd.Parameters.AddWithValue("@Date", achievement.Date);
                         cmd.Parameters.AddWithValue("@UserId", userId);

                         // Create comma-separated string for user-selected tags
                         var userTags = string.Join(",", selectedTags);
                         cmd.Parameters.AddWithValue("@UserTags", userTags);

                        
                        

                         // Add AI-generated tags only if they exist
                         if (aiGeneratedTags != null && aiGeneratedTags.Any())
                         {
                             var aiTags = string.Join(",", aiGeneratedTags);
                             cmd.Parameters.AddWithValue("@AIGeneratedTags", aiTags);
                         }
                         else
                         {
                             cmd.Parameters.AddWithValue("@AIGeneratedTags", DBNull.Value); // Pass NULL if no AI-generated tags
                         }

                         int rowsAffected = await cmd.ExecuteNonQueryAsync();

                         if (rowsAffected == 0)
                         {
                             return StatusCode(StatusCodes.Status500InternalServerError, "No rows were affected during achievement insertion.");
                         }
                        // Get the achievement ID after insertion
                        achievementId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        Console.WriteLine($"Achievement created with ID: {achievementId}");
                     }

                      if (achievementId == 0)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create achievement.");
                    }

                    // Add AI-generated tags
                    if (aiGeneratedTags != null && aiGeneratedTags.Count > 0)
                    {
                        await AddAiGeneratedTags(achievementId, aiGeneratedTags);
                    }

                    // Add user-selected tags
                    if (selectedTags != null && selectedTags.Count > 0)
                    {
                        await AddUserSelectedTags(achievementId, selectedTags);
                    }



                     return Ok("Achievement created successfully.");
                 }
                 else
                 {
                     // Log the exact error message
                     return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving tags: " + (tagsResult as ObjectResult)?.Value);
                 }
             }
             catch (Exception e)
             {
                 Console.WriteLine($"Error creating achievement: {e.Message}");
                 return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating achievement: {e.Message}");
             }
             finally
             {
                 if (_conn.State == ConnectionState.Open)
                 {
                     await _conn.CloseAsync(); // Close the connection only if it's open
                 }
             }
         }


         private async Task AddTagToDatabase(string tagName)
         {
             try
             {
                 // Check if the tag already exists
                 var existingTagsResult = await GetTags(); // Call GetTags without parameters

                 if (existingTagsResult is OkObjectResult okObjectResult)
                 {
                     var tags = okObjectResult.Value as List<Tag>;
                     var existingTagNames = tags.Select(tag => tag.TagName).ToList();

                     if (!existingTagNames.Contains(tagName))
                     {
                         using (SqlCommand cmd = new SqlCommand("usp_InsertTag", _conn)) // Assuming a stored procedure to insert tags
                         {
                             cmd.CommandType = CommandType.StoredProcedure;
                             cmd.Parameters.AddWithValue("@TagName", tagName);
                             await cmd.ExecuteNonQueryAsync();
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Error adding tag to database: {ex.Message}");
                 // Handle the error appropriately
             }
         }
        

        // Add tags to an AI achievement (helper method)
        private async Task AddAiGeneratedTags(int achievementId, List<string> aiGeneratedTags)
        {
            foreach (var tagName in aiGeneratedTags)
            {
                int tagId;

                // Check if the tag already exists
                using (SqlCommand cmd = new SqlCommand("usp_GetTagIdByName", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TagName", tagName);

                    tagId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                if (tagId == 0)
                {
                    // Tag does not exist, create a new tag
                    using (SqlCommand cmd = new SqlCommand("usp_CreateTag", _conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TagName", tagName);

                        tagId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    }
                }

                // Associate the tag with the achievement
                using (SqlCommand cmd = new SqlCommand("usp_AssociateTagWithAchievement", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AchievementId", achievementId);
                    cmd.Parameters.AddWithValue("@TagId", tagId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Helper method to add user-selected tags
        private async Task AddUserSelectedTags(int achievementId, List<string> selectedTags)
        {
            foreach (var tagName in selectedTags)
            {
                int tagId;

                // Check if the tag exists
                using (SqlCommand cmd = new SqlCommand("usp_GetTagIdByName", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TagName", tagName);

                    // Retrieve the TagId
                    tagId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                if (tagId > 0)
                {
                    // Tag exists, associate it with the achievement
                    using (SqlCommand cmd = new SqlCommand("usp_AssociateTagWithAchievement", _conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AchievementId", achievementId);
                        cmd.Parameters.AddWithValue("@TagId", tagId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

 */
