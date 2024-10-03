using Microsoft.AspNetCore.Authorization;
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
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using Braggerbk.Models;

namespace Braggerbk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AchievementsController : ControllerBase
    {
        private readonly SqlConnection _conn;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        // Consolidated constructor
        public AchievementsController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        // Get all achievements by user 
        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllAchievements()
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            try
            {
                var achievements = new List<Achievement>();
                using (SqlCommand cmd = new SqlCommand("usp_GetAllAchievementsByUserWithTags", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    await _conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var achievement = new Achievement
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                // Split AchievementTags by commas to create a list of Tag objects
                                AchievementTags = new List<Tag>()
                            };

                            var tagNames = reader.GetString(reader.GetOrdinal("AchievementTags"));
                            if (!string.IsNullOrWhiteSpace(tagNames))
                            {
                                foreach (var tagName in tagNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    achievement.AchievementTags.Add(new Tag { TagName = tagName });
                                }
                            }

                            achievements.Add(achievement);
                        }
                    }
                }
                return Ok(achievements);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving achievements: {e.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // Get achievement by ID
        // Get achievement by ID
        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            try
            {
                Achievement achievement = null;
                using (SqlCommand cmd = new SqlCommand("usp_GetAchievementById", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    await _conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            achievement = new Achievement
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                AchievementTags = new List<Tag>()
                            };

                            // Get the tags for this achievement
                            var tagNames = reader.IsDBNull(reader.GetOrdinal("AchievementTags")) ? null : reader.GetString(reader.GetOrdinal("AchievementTags"));
                            if (!string.IsNullOrWhiteSpace(tagNames))
                            {
                                foreach (var tagName in tagNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    achievement.AchievementTags.Add(new Tag { TagName = tagName });
                                }
                            }
                        }
                    }
                }

                if (achievement == null)
                {
                    return NotFound("Achievement not found.");
                }

                return Ok(achievement);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving achievement: {e.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAchievement([FromBody] Achievement achievement)

        {
            var userIdNullable = GetUserIdFromToken(); // This is assumed to be nullable (int?)

            if (!userIdNullable.HasValue)
            {
                return Unauthorized("User not authenticated.");
            }

            int userId = userIdNullable.Value; // Safely access the value


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

                    // AI-generated tags (only new ones)
                    var aiGeneratedTags = achievement.AiGeneratedTags?
                        .Where(t => !existingTagNames.Contains(t)) // Only new AI-generated tags
                        .Distinct()
                        .ToList();

                    // Create the achievement and get the ID
                    int achievementId = await CreateAchievementInDatabase(achievement, userId);
                    if (achievementId <= 0)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create achievement.");
                    }

                    // Add AI-generated tags to the database and associate them with the achievement
                    if (aiGeneratedTags != null && aiGeneratedTags.Count > 0)
                    {
                        await AddAiGeneratedTags(achievementId, aiGeneratedTags);
                    }

                    // Add user-selected tags to the achievement
                    if (selectedTags != null && selectedTags.Count > 0)
                    {
                        await AddUserSelectedTags(achievementId, selectedTags);
                    }
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
                    await _conn.CloseAsync();
                }
            }
        }

        // Helper method to create achievement in the database
        private async Task<int> CreateAchievementInDatabase(Achievement achievement, int userId)
        {
            using (SqlCommand cmd = new SqlCommand("usp_InsertAchievement", _conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Title", achievement.Title);
                cmd.Parameters.AddWithValue("@Description", achievement.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", achievement.Date);
                cmd.Parameters.AddWithValue("@UserId", userId);

                // Safely cast the result to an integer (checking for decimal)
                var result = await cmd.ExecuteScalarAsync();
                if (result is decimal decimalResult)
                {
                    return Convert.ToInt32(decimalResult); // Convert decimal to int
                }
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        // Helper method to add AI-generated tags
        private async Task AddAiGeneratedTags(int achievementId, List<string> aiGeneratedTags)
        {
            foreach (var tagName in aiGeneratedTags)
            {
                int tagId;

                // Create a new tag since it is AI-generated
                using (SqlCommand cmd = new SqlCommand("usp_CreateTag", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TagName", tagName);

                    var result = await cmd.ExecuteScalarAsync();

                    // Safely cast decimal to int
                    if (result is decimal decimalResult)
                    {
                        tagId = Convert.ToInt32(decimalResult);
                    }
                    else
                    {
                        tagId = result != null ? Convert.ToInt32(result) : 0;
                    }
                }

                // Associate the newly created tag with the achievement
                await AssociateTagWithAchievement(achievementId, tagId);
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

                    var result = await cmd.ExecuteScalarAsync();

                    // Safely cast decimal to int
                    if (result is decimal decimalResult)
                    {
                        tagId = Convert.ToInt32(decimalResult);
                    }
                    else
                    {
                        tagId = result != null ? Convert.ToInt32(result) : 0;
                    }
                }

                if (tagId > 0)
                {
                    // Tag exists, associate it with the achievement
                    await AssociateTagWithAchievement(achievementId, tagId);
                }
            }
        }


        // Helper method to associate a tag with an achievement
        private async Task AssociateTagWithAchievement(int achievementId, int tagId)
        {
            if (tagId > 0)
            {
                Console.WriteLine($"Associating tagId: {tagId} with achievementId: {achievementId}");

                using (SqlCommand cmd = new SqlCommand("usp_AssociateTagWithAchievement", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AchievementId", achievementId);
                    cmd.Parameters.AddWithValue("@TagId", tagId);
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine($"TagId: {tagId} associated with AchievementId: {achievementId}");
                }
            }
            else
            {
                Console.WriteLine($"Invalid tagId: {tagId} for association.");
            }
        }




        // Update achievement
        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> UpdateAchievement(int id, [FromBody] Achievement achievement)
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
                using (SqlCommand cmd = new SqlCommand("usp_UpdateAchievementWithTags", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AchievementId", id);
                    cmd.Parameters.AddWithValue("@Title", achievement.Title);
                    cmd.Parameters.AddWithValue("@Description", achievement.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Date", achievement.Date);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Tags", string.Join(",", achievement.AchievementTags.Select(t => t.TagName)));

                    await _conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        // Call the method to add tags to the updated achievement
                        await AddUserSelectedTags(id, achievement.AchievementTags.Select(t => t.TagName).ToList());
                        return Ok("Achievement updated successfully.");
                    }
                    else
                    {
                        return NotFound("Achievement not found.");
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating achievement: {e.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



        // Search achievements by tag
        [HttpGet]
        [Route("SearchByTags")]
        public async Task<IActionResult> SearchByTags([FromQuery] string tags)
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            try
            {
                var achievements = new List<Achievement>();
                var achievementDictionary = new Dictionary<int, Achievement>(); // Use a dictionary to track achievements by Id

                using (SqlCommand cmd = new SqlCommand("usp_SearchAchievementsByTag", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Tag", tags);
                    cmd.Parameters.AddWithValue("@UserId", userId); // Pass UserId to the stored procedure

                    await _conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var achievementId = reader.GetInt32(reader.GetOrdinal("Id"));

                            // Check if the achievement already exists in the dictionary
                            if (!achievementDictionary.TryGetValue(achievementId, out var achievement))
                            {
                                achievement = new Achievement
                                {
                                    Id = achievementId,
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    AchievementTags = new List<Tag>() // Initialize the list
                                };

                                achievementDictionary[achievementId] = achievement; // Add to the dictionary
                            }

                            // Retrieve and add the tag
                            if (!reader.IsDBNull(reader.GetOrdinal("TagName")))
                            {
                                var tagName = reader.GetString(reader.GetOrdinal("TagName"));
                                achievement.AchievementTags.Add(new Tag { TagName = tagName });
                            }
                        }
                    }
                }

                // Convert the dictionary values back to a list
                achievements = achievementDictionary.Values.ToList();

                return Ok(achievements);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving achievements: {e.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        // Delete achievement
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_DeleteAchievement", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);

                    await _conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return Ok("Achievement deleted successfully.");
                    }
                    else
                    {
                        return NotFound("Achievement not found.");
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting achievement: {e.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        [HttpGet]
        [Route("GetTags")]
        public async Task<IActionResult> GetTags()
        {
            List<Tag> tags = new List<Tag>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_conn.ConnectionString)) // Use your connection string here
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT Id, TagName FROM Tags", conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tags.Add(new Tag
                                {
                                    Id = reader.GetInt32(0),
                                    TagName = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                return Ok(tags); // Return the list of tags
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving tags: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tags: {ex.Message}");
            }
        }


        /*   // Add tags to an AI achievement (helper method)
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



        // API key and endpoint configuration
        private readonly string _azureApiKey = ""; // Replace with your actual key
        private readonly string _endpoint = ""; // Update the endpoint as necessary

        [HttpGet]
        [Route("GetTagSuggestions")]
        public async Task<IActionResult> GetTagSuggestions([FromQuery] string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest("Description cannot be empty.");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", _azureApiKey);

                var requestBody = new
                {
                    messages = new[]
                    {
                    new { role = "user", content = $"Generate 2 or 3 keywords based on the following description: {description}" }
                },
                    max_tokens = 10, // Adjust based on expected length of suggestions
                    temperature = 0.5 // Adjust based on desired creativity of suggestions
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Send the request to Azure OpenAI
                var response = await client.PostAsync(_endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var openAiResponse = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);

                    // Check if choices are available and extract the text
                    if (openAiResponse?.Choices != null && openAiResponse.Choices.Count > 0)
                    {
                        var tagSuggestions = openAiResponse.Choices.Select(choice => choice.Message.Content.Trim()).ToList();
                        return Ok(tagSuggestions); // Return the list of tag suggestions
                    }

                    return NotFound("No suggestions found.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Error fetching tag suggestions: {errorContent}");
            }
        }



        // Get the user ID from JWT token
        private int? GetUserIdFromToken()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }
    }
}
