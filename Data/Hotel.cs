namespace HotelManagement.Data
{
    public class Hotel
    {
        public int id { get; set; }
        public  string Name { get; set; }
        public string Address { get; set; }
        public int Rating { get; set; }

        public int CountryId { get; set; }
        public Country? Country { get; set; }
    }
}
