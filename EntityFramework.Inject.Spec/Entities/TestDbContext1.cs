using System.Data.Entity;

namespace EntityFramework.Inject.Spec.Entities
{
	public class TestDbContext1 : DbContext
	{
		public TestDbContext1(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}
	}
}