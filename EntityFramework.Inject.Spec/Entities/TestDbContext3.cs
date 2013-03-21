using System.Data.Entity;

namespace EntityFramework.Inject.Spec.Entities
{
	public class TestDbContext3 : DbContext
	{
		public TestDbContext3(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}
	}
}