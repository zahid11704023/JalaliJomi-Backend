namespace JalaliJomi.Backend.Models
{
    public class ContactMessage
    {
        public int MessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Sent";

        public int SenderId { get; set; }
        public RegisteredUser Sender { get; set; } = null!;

        public int ListingId { get; set; }
        public Listing Listing { get; set; } = null!;

    }
}