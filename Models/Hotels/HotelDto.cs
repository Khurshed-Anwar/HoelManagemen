namespace HotelManagement.Models.Hotels
{
    public class CreateHotelDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Rating { get; set; }
        public int CountryId { get; set; }
    }

    public class UpdateHotelDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Rating { get; set; }
        public int CountryId { get; set; }
    }

    public class HotelDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Rating { get; set; }
        public int CountryId { get; set; }
        public string? CountryName { get; set; }
    }
}
