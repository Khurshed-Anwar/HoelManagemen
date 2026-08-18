using Asp.Versioning;
using HotelManagement.Models.Admin;
using HotelManagement.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController(IUserManagementService userService) : ControllerBase
    {
        // GET api/v1/admin/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET api/v1/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user is null)
                return Problem(
                    detail:     $"User with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(user);
        }

        // PUT api/v1/admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            var updated = await userService.UpdateUserAsync(id, dto);
            if (updated is null)
                return Problem(
                    detail:     $"User with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(updated);
        }

        // POST api/v1/admin/users/{id}/roles
        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRoles(string id, [FromBody] AssignRolesDto dto)
        {
            await userService.AssignRolesAsync(id, dto);
            return NoContent();
        }

        // POST api/v1/admin/users/{id}/lock
        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(string id)
        {
            var success = await userService.LockUserAsync(id);
            if (!success)
                return Problem(
                    detail:     $"User with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return NoContent();
        }

        // POST api/v1/admin/users/{id}/unlock
        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> Unlock(string id)
        {
            var success = await userService.UnlockUserAsync(id);
            if (!success)
                return Problem(
                    detail:     $"User with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return NoContent();
        }

        // POST api/v1/admin/users/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return Problem(
                    detail:     "NewPassword cannot be empty.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title:      "Validation Failed");

            var success = await userService.ResetPasswordAsync(id, dto.NewPassword);
            if (!success)
                return Problem(
                    detail:     $"User with id '{id}' was not found or password reset failed.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return NoContent();
        }

        // DELETE api/v1/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await userService.DeleteUserAsync(id);
            if (!success)
                return Problem(
                    detail:     $"User with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return NoContent();
        }
    }
}
