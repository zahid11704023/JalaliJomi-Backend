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

    public class CreateListingDto
    {
        // Listing fields
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // "Sale" or "Rent"
        public string[] Photos { get; set; } = Array.Empty<string>();

        // Property fields (flattened in the request body, same pattern as the response DTOs)
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public int Rooms { get; set; }
    }

    public class ContactOwnerDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ListingDetailDto
    {
       public int ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Location { get; set; } = string.Empty;
    public string[] Photos { get; set; } = Array.Empty<string>();
    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string City { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public int Rooms { get; set; }

    // Owner info, per la sezione "Property Owner" della UI di Manuel
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime OwnerRegistrationDate { get; set; } 
    }
}