namespace JalaliJomi.Backend.Models.Dtos
{
    public class ListingSummaryDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string[] Photos { get; set; } = Array.Empty<string>();
        public string TransactionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Property fields, flattened into the same response (no nested object)
        public string City { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public int Rooms { get; set; }
    }
}