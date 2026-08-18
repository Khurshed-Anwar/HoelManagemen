using Asp.Versioning;
using HotelManagement.Authorization.Attributes;
using HotelManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/countries")]
    [ApiController]
    [Authorize]
    public class CountriesController(HotelListringDbContext context) : ControllerBase
    {
        // GET api/v1/countries — all authenticated users
        [HttpGet]
        [RequirePermission("Countries.Read")]
        public async Task<IActionResult> GetAll()
        {
            var countries = await context.Countries.ToListAsync();
            return Ok(countries);
        }

        // GET api/v1/countries/{id} — all authenticated users
        [HttpGet("{id:int}")]
        [RequirePermission("Countries.Read")]
        public async Task<IActionResult> GetById(int id)
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
                return Problem(
                    detail:     $"Country with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            return Ok(country);
        }

        // POST api/v1/countries — Admin, SalesAdmin, ITAdmin
        [HttpPost]
        [RequirePermission("Countries.Create")]
        public async Task<IActionResult> Create([FromBody] Country country)
        {
            context.Countries.Add(country);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = country.CountryId, version = "1" }, country);
        }

        // PUT api/v1/countries/{id} — Admin, SalesAdmin, ITAdmin, FinanceAdmin
        [HttpPut("{id:int}")]
        [RequirePermission("Countries.Update")]
        public async Task<IActionResult> Update(int id, [FromBody] Country country)
        {
            if (id != country.CountryId)
                return Problem(
                    detail:     "Route id does not match body CountryId.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title:      "Bad Request");

            context.Entry(country).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await context.Countries.AnyAsync(c => c.CountryId == id))
                    return Problem(
                        detail:     $"Country with id '{id}' was not found.",
                        statusCode: StatusCodes.Status404NotFound,
                        title:      "Not Found");

                throw;
            }

            return NoContent();
        }

        // DELETE api/v1/countries/{id} — Admin, FinanceAdmin
        [HttpDelete("{id:int}")]
        [RequirePermission("Countries.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
                return Problem(
                    detail:     $"Country with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title:      "Not Found");

            context.Countries.Remove(country);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
