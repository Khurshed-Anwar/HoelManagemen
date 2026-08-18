using Asp.Versioning;
using HotelManagement.Models.Admin;
using HotelManagement.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/roles")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController(IRoleManagementService roleService) : ControllerBase
    {
        // GET api/v1/admin/roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        // GET api/v1/admin/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role is null)
                return Problem(
                    detail:     $"Role with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(role);
        }

        // POST api/v1/admin/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var created = await roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "1" }, created);
        }

        // DELETE api/v1/admin/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await roleService.DeleteRoleAsync(id);
            if (!success)
                return Problem(
                    detail:     $"Role with id '{id}' was not found or is a protected system role.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title:      "Delete Failed");

            return NoContent();
        }

        // GET api/v1/admin/roles/{id}/permissions
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetPermissions(string id)
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role is null)
                return Problem(
                    detail:     $"Role with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            var permissions = await roleService.GetRolePermissionsAsync(id);
            return Ok(permissions);
        }

        // POST api/v1/admin/roles/{id}/permissions
        [HttpPost("{id}/permissions")]
        public async Task<IActionResult> AssignPermissions(string id, [FromBody] AssignPermissionsDto dto)
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role is null)
                return Problem(
                    detail:     $"Role with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            await roleService.AssignPermissionsAsync(id, dto);
            return NoContent();
        }

        // DELETE api/v1/admin/roles/{id}/permissions/{permissionId}
        [HttpDelete("{id}/permissions/{permissionId:int}")]
        public async Task<IActionResult> RemovePermission(string id, int permissionId)
        {
            var success = await roleService.RemovePermissionAsync(id, permissionId);
            if (!success)
                return Problem(
                    detail:     $"Permission '{permissionId}' is not assigned to role '{id}'.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return NoContent();
        }

        // GET api/v1/admin/roles/{id}/users
        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsers(string id)
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role is null)
                return Problem(
                    detail:     $"Role with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            var users = await roleService.GetUsersInRoleAsync(id);
            return Ok(users);
        }
    }
}
