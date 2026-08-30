namespace JalaliJomi.Backend.Models
{
    public class PropertyOwner : RegisteredUser
    {
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}