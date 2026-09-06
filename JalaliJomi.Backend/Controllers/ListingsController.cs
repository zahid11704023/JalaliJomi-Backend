using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JalaliJomi.Backend.Data;
using JalaliJomi.Backend.Models;
using JalaliJomi.Backend.Models.Dtos;

namespace JalaliJomi.Backend.Controllers
{
    [ApiController]
    [Route("api/listings")]
    public class ListingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<RegisteredUser> _userManager;

        private static readonly string[] ValidTransactionTypes = { "Sale", "Rent" };

        public ListingsController(AppDbContext context, UserManager<RegisteredUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _context.Listings
                .Include(l => l.Property)
                .Include(l => l.Owner)
                .FirstOrDefaultAsync(l => l.ListingId == id && l.Status == "Active");

            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var dto = new ListingDetailDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Location = listing.Location,
                Photos = string.IsNullOrWhiteSpace(listing.Photos)
                    ? Array.Empty<string>()
                    : listing.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                TransactionType = listing.TransactionType,
                Status = listing.Status,
                CreatedAt = listing.CreatedAt,
                City = listing.Property.City,
                PropertyType = listing.Property.PropertyType,
                Area = listing.Property.Area,
                Rooms = listing.Property.Rooms,
                OwnerId = listing.Owner.Id,
                OwnerName = listing.Owner.FullName,
                OwnerRegistrationDate = listing.Owner.RegistrationDate
            };

            return Ok(dto);
        }

        [Authorize]
[HttpGet("mine")]
public async Task<IActionResult> MyListings()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

    var listings = await _context.Listings
        .Include(l => l.Property)
        .Where(l => l.PropertyOwnerId == user.Id)
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Publish([FromBody] CreateListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelStateToFieldErrors());

            if (!ValidTransactionTypes.Contains(dto.TransactionType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new ErrorResponseDto
                {
                    Errors = new Dictionary<string, string[]>
                    {
                        ["transactionType"] = new[] { "Must be either 'Sale' or 'Rent'." }
                    }
                });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

            var property = new Property
            {
                Address = dto.Address,
                City = dto.City,
                PropertyType = dto.PropertyType,
                Area = dto.Area,
                Rooms = dto.Rooms
            };
            _context.Properties.Add(property);
            await _context.SaveChangesAsync(); // need PropertyId before creating the Listing

            var listing = new Listing
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                Photos = dto.Photos == null || dto.Photos.Length == 0
                    ? string.Empty
                    : string.Join(",", dto.Photos),
                TransactionType = dto.TransactionType,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PropertyOwnerId = user.Id,
                PropertyId = property.PropertyId
            };
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            var resultDto = new ListingDetailDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Location = listing.Location,
                Photos = dto.Photos ?? Array.Empty<string>(),
                TransactionType = listing.TransactionType,
                Status = listing.Status,
                CreatedAt = listing.CreatedAt,
                City = property.City,
                PropertyType = property.PropertyType,
                Area = property.Area,
                Rooms = property.Rooms,
                OwnerId = user.Id,
                OwnerName = user.FullName,
                OwnerRegistrationDate = user.RegistrationDate
            };

            return CreatedAtAction(nameof(GetById), new { id = listing.ListingId }, resultDto);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] CreateListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelStateToFieldErrors());

            if (!ValidTransactionTypes.Contains(dto.TransactionType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new ErrorResponseDto
                {
                    Errors = new Dictionary<string, string[]>
                    {
                        ["transactionType"] = new[] { "Must be either 'Sale' or 'Rent'." }
                    }
                });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

            var listing = await _context.Listings
                .Include(l => l.Property)
                .FirstOrDefaultAsync(l => l.ListingId == id);

            // Not found OR belongs to someone else -> same 404, so we don't leak which listing IDs exist
            if (listing == null || listing.PropertyOwnerId != user.Id)
                return NotFound(new { error = "Listing not found." });

            listing.Title = dto.Title;
            listing.Description = dto.Description;
            listing.Price = dto.Price;
            listing.Location = dto.Location;
            listing.Photos = dto.Photos == null || dto.Photos.Length == 0
                ? string.Empty
                : string.Join(",", dto.Photos);
            listing.TransactionType = dto.TransactionType;
            listing.UpdatedAt = DateTime.UtcNow;

            listing.Property.Address = dto.Address;
            listing.Property.City = dto.City;
            listing.Property.PropertyType = dto.PropertyType;
            listing.Property.Area = dto.Area;
            listing.Property.Rooms = dto.Rooms;

            await _context.SaveChangesAsync();

            var resultDto = new ListingDetailDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Location = listing.Location,
                Photos = dto.Photos ?? Array.Empty<string>(),
                TransactionType = listing.TransactionType,
                Status = listing.Status,
                CreatedAt = listing.CreatedAt,
                City = listing.Property.City,
                PropertyType = listing.Property.PropertyType,
                Area = listing.Property.Area,
                Rooms = listing.Property.Rooms,
                OwnerId = user.Id,
                OwnerName = user.FullName,
                OwnerRegistrationDate = user.RegistrationDate
            };

            return Ok(resultDto);
        }

        // --- private helper methods ---

        private ErrorResponseDto ModelStateToFieldErrors()
        {
            var errors = ModelState
                .Where(kvp => kvp.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key.ToLower(),
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new ErrorResponseDto { Errors = errors };
        }
    }
}