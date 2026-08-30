namespace JalaliJomi.Backend.Models
{
    public class Listing
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Photos { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int PropertyOwnerId { get; set; }
        public PropertyOwner Owner { get; set; } = null!;


        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;


        public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();
        public ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();

    }
}