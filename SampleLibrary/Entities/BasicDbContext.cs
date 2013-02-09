using System.Data.Entity;

namespace SampleLibrary.Entities
{
	public class BasicDbContext : DbContext
	{
		public BasicDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}

		public DbSet<ActionType> ActionTypes { get; set; }
	}
}