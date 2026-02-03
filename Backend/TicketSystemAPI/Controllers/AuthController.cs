using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Models;
using TicketSystemAPI.Services;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseService _dbService;

        public AuthController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Username and password are required"
                    });
                }

                var user = await _dbService.AuthenticateUser(request.Username, request.Password);

                if (user != null)
                {
                    user.Password = string.Empty; // Don't send password back
                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        User = user
                    });
                }

                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("admins")]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetAdmins()
        {
            try
            {
                var admins = await _dbService.GetAdminUsers();
                return Ok(new ApiResponse<List<User>>
                {
                    Success = true,
                    Message = "Admins retrieved successfully",
                    Data = admins
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<User>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }
    }
}
