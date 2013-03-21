using System.Data.Entity;

namespace EntityFramework.Inject.Spec.Entities
{
	public class TestDbContext2 : DbContext
	{
		public TestDbContext2(string nameOrConnectionString) : base(nameOrConnectionString)
		{
		}
	}
}