using Asp.Versioning;
using HotelManagement.Data;
using HotelManagement.Models.Auth;
using HotelManagement.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        // POST api/v1/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDetails registration)
        {
            var errors = await authService.RegisterAsync(registration);
            var errorList = errors.ToList();

            if (errorList.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(
                    errorList.ToDictionary(_ => string.Empty, e => new[] { e })));

            return Ok(new { message = "Registration successful. You have been assigned the Reader role." });
        }

        // POST api/v1/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDetails login)
        {
            var response = await authService.LoginAsync(login);

            if (response is null)
                return Problem(
                    detail:     "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title:      "Authentication Failed");

            return Ok(response);
        }

        // POST api/v1/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            // Extract userId from the expired access token sent in Authorization header
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Problem(
                    detail:     "User identity could not be determined.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title:      "Unauthorized");

            var response = await authService.RefreshTokenAsync(userId, request.RefreshToken);

            if (response is null)
                return Problem(
                    detail:     "Refresh token is invalid, expired or has been revoked.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title:      "Token Refresh Failed");

            return Ok(response);
        }

        // POST api/v1/auth/revoke
        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request)
        {
            await authService.RevokeTokenAsync(request.RefreshToken);
            return Ok(new { message = "Token revoked successfully. You have been logged out." });
        }

        // GET api/v1/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Problem(
                    detail:     "User identity could not be determined.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title:      "Unauthorized");

            var userInfo = await authService.GetCurrentUserAsync(userId);

            if (userInfo is null)
                return Problem(
                    detail:     $"User not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(userInfo);
        }
    }
}

