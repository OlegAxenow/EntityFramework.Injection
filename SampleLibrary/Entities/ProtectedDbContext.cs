using System.Data.Entity;

namespace SampleLibrary.Entities
{
	public class ProtectedDbContext : DbContext
	{
		public ProtectedDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}

		public DbSet<ActionType> ActionTypes { get; set; }

		public DbSet<ProtectedCategory> Categories { get; set; }
	}
}