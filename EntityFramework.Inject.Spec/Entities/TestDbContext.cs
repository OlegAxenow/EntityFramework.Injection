using System.Data.Entity;

namespace EntityFramework.Inject.Spec.Entities
{
	public class TestDbContext : DbContext
	{
		public TestDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}
	}
}