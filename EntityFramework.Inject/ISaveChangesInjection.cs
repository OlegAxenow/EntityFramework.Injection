using System.Data.Entity;
using Method.Inject;

namespace EntityFramework.Inject
{
	public interface ISaveChangesInjection : IMethodInjection
	{
		void OnBeforeSaveChanges(DbContext context);

		void OnAfterSaveChanges(DbContext context);
	}
}