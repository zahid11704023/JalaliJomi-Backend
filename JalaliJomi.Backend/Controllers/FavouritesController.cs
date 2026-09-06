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
    [Route("api/favourites")]
    [Authorize] // every action here requires a logged-in user, so it's simpler to put it once at the class level
    public class FavouritesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<RegisteredUser> _userManager;

        public FavouritesController(AppDbContext context, UserManager<RegisteredUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> MyFavourites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

            var favourites = await _context.Favourites
                .Include(f => f.Listing)
                    .ThenInclude(l => l.Property)
                .Where(f => f.RegisteredUserId == user.Id)
                .OrderByDescending(f => f.SavedAt)
                .ToListAsync();

            var result = favourites.Select(f => new FavouriteDto
            {
                FavouriteId = f.FavouriteId,
                SavedAt = f.SavedAt,
                ListingId = f.Listing.ListingId,
                Title = f.Listing.Title,
                Price = f.Listing.Price,
                Location = f.Listing.Location,
                Photos = string.IsNullOrWhiteSpace(f.Listing.Photos)
                    ? Array.Empty<string>()
                    : f.Listing.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                TransactionType = f.Listing.TransactionType,
                Status = f.Listing.Status,
                City = f.Listing.Property.City,
                PropertyType = f.Listing.Property.PropertyType,
                Area = f.Listing.Property.Area,
                Rooms = f.Listing.Property.Rooms
            });

            return Ok(result);
        }

        [HttpPost("{listingId:int}")]
        public async Task<IActionResult> Add(int listingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

            var listingExists = await _context.Listings.AnyAsync(l => l.ListingId == listingId);
            if (!listingExists)
                return NotFound(new { error = "Listing not found." });

            var alreadyFavourited = await _context.Favourites
                .AnyAsync(f => f.RegisteredUserId == user.Id && f.ListingId == listingId);

            if (alreadyFavourited)
                return Conflict(new { error = "Listing is already in your favourites." });

            var favourite = new Favourite
            {
                RegisteredUserId = user.Id,
                ListingId = listingId,
                SavedAt = DateTime.UtcNow
            };
            _context.Favourites.Add(favourite);
            await _context.SaveChangesAsync();

            return Ok(new { favouriteId = favourite.FavouriteId, listingId = favourite.ListingId });
        }

        [HttpDelete("{listingId:int}")]
        public async Task<IActionResult> Remove(int listingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

            var favourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.RegisteredUserId == user.Id && f.ListingId == listingId);

            if (favourite == null)
                return NotFound(new { error = "Favourite not found." });

            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}