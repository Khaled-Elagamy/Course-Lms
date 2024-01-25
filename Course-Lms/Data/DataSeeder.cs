using Course_Lms.Data.Static;
using Course_Lms.Models;
using Microsoft.AspNetCore.Identity;

namespace Course_Lms.Data
{
	public class DataSeeder
	{
		public static async Task Seed(IApplicationBuilder applicationBuilder)
		{
			using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
			{
				var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
				var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

				context.Database.EnsureCreated();
				//Categories
				if (!context.Categories.Any())
				{
					context.Categories.AddRange(new List<Category>()
					{
						new Category { Name = "Computer Science"},
						new Category { Name = "Music"},
						new Category { Name = "Fitness"},
						new Category { Name = "Photography"},
						new Category { Name = "Engineering"},

					});
					context.SaveChanges();
				}
				if (!context.Courses.Any())
				{
					var instructor = await userManager.FindByEmailAsync("instructor@test.com");

					context.Courses.AddRange(new List<Course>()
					{ new Course
					{
						Title = "Introduction to Programming",
						Description = "Learn the basics of programming",
						ImageUrl="6aa173a_20240117182215851_Backg.png",
						Price = 29.99f,
						IsPublished = true,
						CategoryId = 1,
						CreatedAt = DateTime.Now,
						UpdatedAt = DateTime.Now,
						UserId = instructor.Id,
						Chapters = new List<Chapter>
					{
						new Chapter { Title = "Getting Started", Description = "Introduction to the course", VideoUrl = "f6ddeb18-1ddf-4ed6-810d-c809c6fe086d-1.mp4", Position = 1, IsPublished = true, IsFree = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
						new Chapter { Title = "Variables and Data Types", Description = "Understanding variables and data types", VideoUrl = "f6ddeb18-1ddf-4ed6-810d-c809c6fe086d-2", Position = 2, IsPublished = true, IsFree = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
					}
					},

					new Course
					{
						Title = "Web Development Fundamentals",
						Description = "Explore the fundamentals of web development",
						ImageUrl="246b6f6_20240118044213330_bogom.png",
						Price = 39.99f,
						IsPublished = true,
						CategoryId = 2,
						CreatedAt = DateTime.Now,
						UpdatedAt = DateTime.Now,
						UserId = instructor.Id,
						Chapters = new List<Chapter>
					{
						new Chapter { Title = "HTML Basics", Description = "Introduction to HTML", VideoUrl = "0c6f9923-5507-48c5-a4d7-c2124be673b7-3.mp4", Position = 1, IsPublished = true, IsFree = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
						new Chapter { Title = "CSS Styling", Description = "Styling web pages with CSS", VideoUrl = "d2458713-984a-4052-9dba-75a5b294921c-4.mp4", Position = 2, IsPublished = true, IsFree = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
						new Chapter { Title = "Introduction to JavaScript", Description = "Overview of JavaScript", VideoUrl = "d2458713-984a-4052-9dba-75a5b294921c-5.mp4", Position = 3, IsPublished = false, IsFree = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
					}
					},

				});
					context.SaveChanges();

				}

			}
		}
		public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
		{
			using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
			{

				//Roles
				var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


				if (!await roleManager.RoleExistsAsync(UserRoles.Instructor))
					await roleManager.CreateAsync(new IdentityRole(UserRoles.Instructor));
				if (!await roleManager.RoleExistsAsync(UserRoles.User))
					await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

				//Users
				var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
				string instructorUserEmail = "instructor@test.com";

				var instructorUser = await userManager.FindByEmailAsync(instructorUserEmail);
				if (instructorUser == null)
				{
					var newUser = new ApplicationUser()
					{
						FullName = "Instructor User",
						UserName = "adminUserEmail",
						Email = instructorUserEmail,
						EmailConfirmed = true
					};
					await userManager.CreateAsync(newUser, "Test@1234?");
					await userManager.AddToRoleAsync(newUser, UserRoles.Instructor);

				}


				string appUserEmail = "user@test.com";

				var appUser = await userManager.FindByEmailAsync(appUserEmail);
				if (appUser == null)
				{
					var newAppUser = new ApplicationUser()
					{
						FullName = "Application User",
						UserName = "appUserEmail",
						Email = appUserEmail,
						EmailConfirmed = true
					};
					await userManager.CreateAsync(newAppUser, "Test@1234");
					await userManager.AddToRoleAsync(newAppUser, UserRoles.User);
				}
			}
		}
	}
}
