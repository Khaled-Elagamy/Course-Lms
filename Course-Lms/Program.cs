using Course_Lms.Data;
using Course_Lms.Data.Repositories;
using Course_Lms.Logic.Extensions;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models;
using Course_Lms.Models.StripeSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = false; options.Password.RequireDigit = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
}
)
				.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<EfUnitOfWork>(); // Adjust the lifetime scope as needed

builder.Services.AddControllersWithViews();

builder.Services.RegisterServiceReflection(typeof(ICourseService)); //reflection for all services

builder.Services.AddMemoryCache();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
	app.UseExceptionHandler("/Home/Error");
	app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");
	app.UseHsts();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Courses}/{action=Index}");
app.MapRazorPages();
DataSeeder.SeedUsersAndRolesAsync(app).Wait();
DataSeeder.Seed(app).Wait();
app.Run();
