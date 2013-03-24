using System.Data.Entity;

namespace EntityFramework.Inject.Spec.Helpers
{
	public class TestDatabaseInitializer<T> : IDatabaseInitializer<T> where T : DbContext
	{
		public void InitializeDatabase(T context)
		{
		}
	}
}