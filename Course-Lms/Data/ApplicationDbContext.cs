using Course_Lms.Data.Base;
using Course_Lms.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Course_Lms.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public DbSet<Course> Courses { get; set; }
		public DbSet<Instructor> Instructors { get; set; }
		public DbSet<Chapter> Chapters { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<UserProgress> UserProgresses { get; set; }
		public DbSet<Purchase> Purchases { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		#region Override Methods to have created at and updated at
		public override int SaveChanges()
		{
			AddTimestamps();
			return base.SaveChanges();
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			AddTimestamps();
			return await base.SaveChangesAsync();
		}

		private void AddTimestamps()
		{
			var entities = ChangeTracker.Entries()
				.Where(x => x.Entity is IEntityBase && (x.State == EntityState.Added || x.State == EntityState.Modified));

			foreach (var entity in entities)
			{
				var now = DateTime.Now; // current datetime

				if (entity.State == EntityState.Added)
				{
					((IEntityBase)entity.Entity).CreatedAt = now;
				}
				((IEntityBase)entity.Entity).UpdatedAt = now;
			}
		}
		#endregion
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<Course>()
			.HasOne(c => c.Category)
			.WithMany(cat => cat.Courses)
			.HasForeignKey(c => c.CategoryId);
			modelBuilder.Entity<Course>()
			 .HasIndex(c => c.Title)
			 .IsUnique();



			modelBuilder.Entity<Chapter>()
				.HasOne(ch => ch.Course)
				.WithMany(c => c.Chapters)
				.HasForeignKey(ch => ch.CourseId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserProgress>()
				.HasOne(up => up.Chapter)
				.WithMany(ch => ch.UserProgress)
				.HasForeignKey(up => up.ChapterId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Purchase>()
				.HasOne(p => p.Course)
				.WithMany(c => c.Purchases)
				.HasForeignKey(p => p.CourseId)
				.OnDelete(DeleteBehavior.Cascade);



			base.OnModelCreating(modelBuilder);
		}
	}
}
/*
builder.Property(e => e.UpdatedAt)
	   .ValueGeneratedOnUpdate();
*/