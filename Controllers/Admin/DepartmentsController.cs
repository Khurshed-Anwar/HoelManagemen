using Asp.Versioning;
using HotelManagement.Models.Admin;
using HotelManagement.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/departments")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DepartmentsController(IDepartmentService departmentService) : ControllerBase
    {
        // GET api/v1/admin/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await departmentService.GetAllAsync();
            return Ok(departments);
        }

        // GET api/v1/admin/departments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await departmentService.GetByIdAsync(id);
            if (department is null)
                return Problem(
                    detail:     $"Department with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(department);
        }

        // POST api/v1/admin/departments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            var created = await departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "1" }, created);
        }

        // PUT api/v1/admin/departments/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
        {
            var updated = await departmentService.UpdateAsync(id, dto);
            if (updated is null)
                return Problem(
                    detail:     $"Department with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(updated);
        }

        // DELETE api/v1/admin/departments/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await departmentService.DeleteAsync(id);
            if (!success)
                return Problem(
                    detail:     $"Department with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return NoContent();
        }

        // GET api/v1/admin/departments/{id}/roles
        [HttpGet("{id:int}/roles")]
        public async Task<IActionResult> GetRoles(int id)
        {
            var department = await departmentService.GetByIdAsync(id);
            if (department is null)
                return Problem(
                    detail:     $"Department with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            var roles = await departmentService.GetRolesAsync(id);
            return Ok(roles);
        }
    }
}
