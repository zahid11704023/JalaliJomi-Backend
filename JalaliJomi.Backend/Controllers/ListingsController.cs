using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JalaliJomi.Backend.Data;
using JalaliJomi.Backend.Models.Dtos;

namespace JalaliJomi.Backend.Controllers
{
    [ApiController]
    [Route("api/listings")]
    public class ListingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ListingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? location,
            [FromQuery] string? propertyType,
            [FromQuery] string? transactionType,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var query = _context.Listings
                .Include(l => l.Property)
                .Where(l => l.Status == "Active")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(l => l.Location.ToLower().Contains(location.ToLower()));

            if (!string.IsNullOrWhiteSpace(propertyType))
                query = query.Where(l => l.Property.PropertyType.ToLower() == propertyType.ToLower());

            if (!string.IsNullOrWhiteSpace(transactionType))
                query = query.Where(l => l.TransactionType.ToLower() == transactionType.ToLower());

            if (minPrice.HasValue)
                query = query.Where(l => l.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(l => l.Price <= maxPrice.Value);

            var listings = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var result = listings.Select(l => new ListingSummaryDto
            {
                ListingId = l.ListingId,
                Title = l.Title,
                Price = l.Price,
                Location = l.Location,
                Photos = string.IsNullOrWhiteSpace(l.Photos)
                    ? Array.Empty<string>()
                    : l.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                TransactionType = l.TransactionType,
                Status = l.Status,
                CreatedAt = l.CreatedAt,
                City = l.Property.City,
                PropertyType = l.Property.PropertyType,
                Area = l.Property.Area,
                Rooms = l.Property.Rooms
            });

            return Ok(result);
        }
    }
}