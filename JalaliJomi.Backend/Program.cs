using Microsoft.EntityFrameworkCore;              // AGGIUNTO
using JalaliJomi.Backend.Data;                     // AGGIUNTO
using Microsoft.AspNetCore.Identity;               // NUOVO
using JalaliJomi.Backend.Models;                   // NUOVO

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// AGGIUNTO: registra il DbContext con SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// NUOVO: registra Identity, usando AppDbContext come storage
builder.Services.AddIdentity<RegisteredUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// NUOVO: configura il cookie di autenticazione (HttpOnly, non JWT)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    // Siamo un'API: niente redirect a pagine di login, ritorna solo status code
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// NUOVO: CORS per permettere al frontend (Vite) di chiamare l'API con i cookie
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // necessario per i cookie cross-origin
    });
});

var app = builder.Build();

// --- SEED: inserisce dati finti solo se il database è vuoto ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Listings.Any())
    {
        var properties = new List<Property>
        {
            new Property { Address = "House 12, Road 5, Gulshan 2", City = "Dhaka", PropertyType = "Apartment", Area = 1850, Rooms = 3 },
            new Property { Address = "Road 27, Banani", City = "Dhaka", PropertyType = "Penthouse", Area = 3200, Rooms = 5 },
            new Property { Address = "Road 8, Dhanmondi", City = "Dhaka", PropertyType = "Apartment", Area = 1100, Rooms = 2 },
            new Property { Address = "Agrabad Commercial Area", City = "Chittagong", PropertyType = "House", Area = 2400, Rooms = 4 },
            new Property { Address = "Block C, Mirpur", City = "Dhaka", PropertyType = "Apartment", Area = 1450, Rooms = 3 }
        };
        context.Properties.AddRange(properties);
        context.SaveChanges(); // salva subito per ottenere i PropertyId generati

        var listings = new List<Listing>
        {
            new Listing { Title = "Bright apartment in Gulshan 2", Description = "Light-filled family apartment with a quiet outlook.", Price = 8500000, Location = "Gulshan 2, Dhaka", Photos = "https://images.unsplash.com/photo-1560185127-6ed189bf02f4", TransactionType = "Sale", Status = "Active", PropertyOwnerId = 1, PropertyId = properties[0].PropertyId },
            new Listing { Title = "Spacious penthouse in Banani", Description = "A spacious upper-floor home with private terraces.", Price = 12000000, Location = "Banani, Dhaka", Photos = "https://images.unsplash.com/photo-1649083048337-4aeb6dda80bb", TransactionType = "Sale", Status = "Active", PropertyOwnerId = 1, PropertyId = properties[1].PropertyId },
            new Listing { Title = "Cozy apartment in Dhanmondi", Description = "Move-in ready two bedroom apartment near parks.", Price = 42000, Location = "Dhanmondi, Dhaka", Photos = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2", TransactionType = "Rent", Status = "Active", PropertyOwnerId = 1, PropertyId = properties[2].PropertyId },
            new Listing { Title = "Owner-listed home in Agrabad", Description = "A calm home with airy rooms near the city centre.", Price = null, Location = "Agrabad, Chittagong", Photos = "https://images.unsplash.com/photo-1613545325278-f24b0cae1224", TransactionType = "Rent", Status = "Active", PropertyOwnerId = 1, PropertyId = properties[3].PropertyId },
            new Listing { Title = "Modern apartment in Mirpur", Description = "Thoughtfully maintained apartment in a residential community.", Price = 5500000, Location = "Mirpur, Dhaka", Photos = "https://images.unsplash.com/photo-1745794621090-d856c53b0cc2", TransactionType = "Sale", Status = "Active", PropertyOwnerId = 1, PropertyId = properties[4].PropertyId }
        };
        context.Listings.AddRange(listings);
        context.SaveChanges();
    }
}
// --- fine SEED ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");      // NUOVO — deve stare dopo UseRouting, prima di Authentication/Authorization
app.UseAuthentication();           // NUOVO — mancava del tutto, senza questo Identity non funziona
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();