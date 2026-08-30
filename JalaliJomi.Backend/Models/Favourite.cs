namespace JalaliJomi.Backend.Models
{
    public class Favourite
    {
        public int FavouriteId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        
        public int RegisteredUserId { get; set; }
        public RegisteredUser User { get; set; } = null!;

        public int ListingId { get; set; } 
        public Listing Listing { get; set; } = null!;
    }
}