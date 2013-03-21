using System.Data.Entity;

namespace EntityFramework.Inject.Spec.Entities
{
	public class TestConcurrencyDbContext : DbContext
	{
		public TestConcurrencyDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}
	}
}