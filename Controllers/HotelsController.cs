using Asp.Versioning;
using HotelManagement.Authorization.Attributes;
using HotelManagement.Data;
using HotelManagement.Models.Hotels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/hotels")]
    [ApiController]
    [Authorize]
    public class HotelsController(HotelListringDbContext context) : ControllerBase
    {
        // GET api/v1/hotels — all authenticated users
        [HttpGet]
        [RequirePermission("Hotels.Read")]
        public async Task<IActionResult> GetAll()
        {
            var hotels = await context.Hotels
                .Include(h => h.Country)
                .Select(h => ToDto(h))
                .ToListAsync();
            return Ok(hotels);
        }

        // GET api/v1/hotels/{id} — all authenticated users
        [HttpGet("{id:int}")]
        [RequirePermission("Hotels.Read")]
        public async Task<IActionResult> GetById(int id)
        {
            var hotel = await FindHotelAsync(id);
            if (hotel is null) return HotelNotFound(id);

            return Ok(ToDto(hotel));
        }

        // POST api/v1/hotels — Admin, SalesAdmin, ITAdmin, FinanceAdmin
        [HttpPost]
        [RequirePermission("Hotels.Create")]
        public async Task<IActionResult> Create([FromBody] CreateHotelDto dto)
        {
            if (!await CountryExistsAsync(dto.CountryId))
                return CountryNotFound(dto.CountryId);

            var hotel = new Hotel
            {
                Name      = dto.Name,
                Address   = dto.Address,
                Rating    = dto.Rating,
                CountryId = dto.CountryId
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = hotel.id, version = "1" }, ToDto(hotel));
        }

        // PUT api/v1/hotels/{id} — Admin, SalesAdmin, ITAdmin
        [HttpPut("{id:int}")]
        [RequirePermission("Hotels.Update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHotelDto dto)
        {
            var hotel = await FindHotelAsync(id);
            if (hotel is null) return HotelNotFound(id);

            if (!await CountryExistsAsync(dto.CountryId))
                return CountryNotFound(dto.CountryId);

            hotel.Name      = dto.Name;
            hotel.Address   = dto.Address;
            hotel.Rating    = dto.Rating;
            hotel.CountryId = dto.CountryId;

            await context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/v1/hotels/{id} — Admin, SalesAdmin
        [HttpDelete("{id:int}")]
        [RequirePermission("Hotels.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await FindHotelAsync(id);
            if (hotel is null) return HotelNotFound(id);

            context.Hotels.Remove(hotel);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private Task<Hotel?> FindHotelAsync(int id) =>
            context.Hotels.Include(h => h.Country).FirstOrDefaultAsync(h => h.id == id);

        private Task<bool> CountryExistsAsync(int countryId) =>
            context.Countries.AnyAsync(c => c.CountryId == countryId);

        private IActionResult HotelNotFound(int id) =>
            Problem(detail: $"Hotel with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound, title: "Not Found");

        private IActionResult CountryNotFound(int id) =>
            Problem(detail: $"Country with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound, title: "Not Found");

        private static HotelDto ToDto(Hotel h) => new()
        {
            Id          = h.id,
            Name        = h.Name,
            Address     = h.Address,
            Rating      = h.Rating,
            CountryId   = h.CountryId,
            CountryName = h.Country?.Name
        };
    }
}
