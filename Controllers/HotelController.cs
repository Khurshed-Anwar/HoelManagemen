using HotelManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("/api/hotel")]
public class HotelController : ControllerBase
{
    private static readonly List<Hotel> Hotels =
    [
        new Hotel { Id = 1, Name = "Sunrise Inn", Address = "123 Main St" },
        new Hotel { Id = 2, Name = "Sunset Lodge", Address = "456 Elm St" }
    ];

    [HttpGet]
    public ActionResult<IEnumerable<Hotel>> GetAll()
    {
        return Ok(Hotels);
    }

    [HttpPost]
    public ActionResult<Hotel> Add(Hotel hotel)
    {
        if (Hotels.Any(h => h.Id == hotel.Id))
        {
            return Conflict($"A hotel with Id {hotel.Id} already exists.");
        }

        Hotels.Add(hotel);
        return CreatedAtAction(nameof(GetAll), new { id = hotel.Id }, hotel);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Hotel> Update(int id, Hotel updatedHotel)
    {
        var hotel = Hotels.FirstOrDefault(h => h.Id == id);
        if (hotel is null)
        {
            return NotFound($"Hotel with Id {id} not found.");
        }

        hotel.Name = updatedHotel.Name;
        hotel.Address = updatedHotel.Address;

        return Ok(hotel);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var hotel = Hotels.FirstOrDefault(h => h.Id == id);
        if (hotel is null)
        {
            return NotFound($"Hotel with Id {id} not found.");
        }

        Hotels.Remove(hotel);
        return NoContent();
    }
}
