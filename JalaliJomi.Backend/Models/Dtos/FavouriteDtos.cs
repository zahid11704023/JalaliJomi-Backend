namespace JalaliJomi.Backend.Models.Dtos
{
    // Reuses ListingSummaryDto's shape for the actual listing data returned by GET /api/favourites,
    // just wrapped with the favouriteId and savedAt so the frontend can "unfavourite" and show a date.
    public class FavouriteDto
    {
        public int FavouriteId { get; set; }
        public DateTime SavedAt { get; set; }

        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string[] Photos { get; set; } = Array.Empty<string>();
        public string TransactionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public int Rooms { get; set; }
    }
}