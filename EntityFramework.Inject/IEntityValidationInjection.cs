using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Method.Inject;

namespace EntityFramework.Inject
{
	public interface IEntityValidationInjection : IMethodInjection
	{
		void OnValidateEntity(DbEntityValidationResult result, DbEntityEntry entityEntry, IDictionary<object, object> items);
	}
}