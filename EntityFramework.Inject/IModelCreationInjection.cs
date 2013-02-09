using System.Data.Entity;
using Method.Inject;

namespace EntityFramework.Inject
{
	public interface IModelCreationInjection : IMethodInjection
	{
		void OnModelCreating(DbModelBuilder modelBuilder);
	}
}