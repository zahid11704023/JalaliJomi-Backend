namespace JalaliJomi.Backend.Models
{
    public class Property
    {
        public int PropertyId { get; set; }
        public string Address { get; set; }= string.Empty;
        public string City { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public int Rooms { get; set; }
        public Listing Listing { get; set; } = null!;


    }
}