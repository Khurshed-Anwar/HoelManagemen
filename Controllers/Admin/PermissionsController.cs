using Asp.Versioning;
using HotelManagement.Models.Admin;
using HotelManagement.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/permissions")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PermissionsController(
        IPermissionService permissionService,
        IResourcePermissionGenerator generator) : ControllerBase
    {
        // GET api/v1/admin/permissions
        // Returns all permissions grouped by resource
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var all = await permissionService.GetAllPermissionsAsync();

            var grouped = all
                .Select(p =>
                {
                    var parts = p.Split('.', 2);
                    return new PermissionDto
                    {
                        Resource = parts[0],
                        Action   = parts.Length > 1 ? parts[1] : string.Empty,
                        FullName = p
                    };
                })
                .GroupBy(p => p.Resource)
                .Select(g => new ResourcePermissionsDto
                {
                    Resource    = g.Key,
                    Permissions = g.ToList()
                })
                .OrderBy(g => g.Resource)
                .ToList();

            return Ok(grouped);
        }

        // GET api/v1/admin/permissions/resources
        // Returns all distinct resource names
        [HttpGet("resources")]
        public async Task<IActionResult> GetResources()
        {
            var resources = await generator.GetAllResourcesAsync();
            return Ok(resources);
        }

        // POST api/v1/admin/permissions/generate
        // Generates Read, Create, Update, Delete permissions for a new resource
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GeneratePermissionsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ResourceName))
                return Problem(
                    detail:     "ResourceName cannot be empty.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title:      "Validation Failed");

            var created = await generator.GenerateAsync(dto.ResourceName.Trim());

            if (created.Count == 0)
                return Ok(new
                {
                    message     = $"All permissions for '{dto.ResourceName}' already exist.",
                    created     = Array.Empty<string>()
                });

            return Ok(new
            {
                message = $"{created.Count} permission(s) created for '{dto.ResourceName}'.",
                created
            });
        }
    }
}
