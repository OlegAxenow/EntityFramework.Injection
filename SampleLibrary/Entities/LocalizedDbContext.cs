using System.Data.Entity;

namespace SampleLibrary.Entities
{
	public class LocalizedDbContext : DbContext
	{
		public LocalizedDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}

		public DbSet<ActionType> ActionTypes { get; set; }

		public DbSet<Category> Categories { get; set; }
	}
}