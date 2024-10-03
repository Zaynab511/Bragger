using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System;
using Braggerbk.Models;

namespace Braggerbk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SqlConnection _conn;
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid user data.");
            }

            try
            {
                // Check if the email already exists
                bool emailExists = await EmailExists(user.Email);
                if (emailExists)
                {
                    return BadRequest("Email already in use.");
                }

                using (SqlCommand cmd = new SqlCommand("usp_RegisterUser", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", user.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash); // Using plain text password
                    cmd.Parameters.AddWithValue("@JobTitle", user.JobTitle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Industry", user.Industry ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Contact", user.Contact ?? (object)DBNull.Value);

                    await _conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    await _conn.CloseAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "User registered successfully." });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Registration failed.");
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique key constraint violation
            {
                return BadRequest("Email already in use.");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error in Register: {e.Message}");
            }
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid login data.");
            }

            try
            {
                var userId = await ValidateUser(loginModel.Email, loginModel.Password);
                if (userId == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                var token = GenerateJwtToken(userId.Value);

                return Ok(new { Token = token });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error during login: {e.Message}");
            }
        }


        [HttpGet]
        [Route("Profile/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetProfile(int userId)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_GetUserProfile", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    await _conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var profile = new Profile
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                JobTitle = reader["JobTitle"].ToString(),
                                Industry = reader["Industry"].ToString(),
                                Contact = reader["Contact"].ToString(),
                                ProfilePicture = reader["ProfilePicture"].ToString()
                            };

                            await _conn.CloseAsync();
                            return Ok(profile);
                        }
                        else
                        {
                            await _conn.CloseAsync();
                            return NotFound("User profile not found.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving profile: {e.Message}");
            }
        }

        [HttpPut]
        [Route("Profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] Profile profile)
        {
            if (profile == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid profile data.");
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_UpdateUserProfile", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", profile.Id);
                    cmd.Parameters.AddWithValue("@Name", profile.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", profile.Email);
                    cmd.Parameters.AddWithValue("@JobTitle", profile.JobTitle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Industry", profile.Industry ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Contact", profile.Contact ?? (object)DBNull.Value);

                    // Handle ProfilePicture as VARBINARY(MAX)
                    byte[] profilePictureBytes = profile.ProfilePicture != null
                        ? Convert.FromBase64String(profile.ProfilePicture)
                        : null;

                    cmd.Parameters.AddWithValue("@ProfilePicture", profilePictureBytes);

                    await _conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    await _conn.CloseAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok("Profile updated successfully.");
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Profile update failed.");
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating profile: {e.Message}");
            }
        }

        private async Task<int?> ValidateUser(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email or password cannot be null or empty.");
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ValidateUser", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", password); // Store plain text password

                    await _conn.OpenAsync();
                    var result = (int?)await cmd.ExecuteScalarAsync();
                    await _conn.CloseAsync();

                    if (result == 1)
                    {
                        using (SqlCommand getUserCmd = new SqlCommand("SELECT Id FROM Users WHERE Email = @Email", _conn))
                        {
                            getUserCmd.Parameters.AddWithValue("@Email", email);

                            await _conn.OpenAsync();
                            var userId = (int?)await getUserCmd.ExecuteScalarAsync();
                            await _conn.CloseAsync();

                            return userId;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in ValidateUser: {ex.Message}");
                throw;
            }
        }
        //email already exist handling 
        private async Task<bool> EmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Email = @Email", _conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);

                    await _conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    await _conn.CloseAsync();

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in EmailExists: {ex.Message}");
                throw;
            }
        }


        private string GenerateJwtToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            if (key.Length == 0)
            {
                throw new InvalidOperationException("JWT secret is not configured or is empty.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"], // Set Issuer
                Audience = _configuration["Jwt:Audience"], // Set Audience
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}

