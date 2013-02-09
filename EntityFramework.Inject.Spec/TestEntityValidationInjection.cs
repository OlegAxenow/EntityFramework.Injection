using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Method.Inject;

namespace EntityFramework.Inject.Spec
{
	public class TestEntityValidationInjection : MethodInjection, IEntityValidationInjection
	{
		public TestEntityValidationInjection()
		{
			Entries = new List<DbEntityEntry>();
		}

		public List<DbEntityEntry> Entries { get; private set; }

		public void OnValidateEntity(DbEntityValidationResult result, DbEntityEntry entityEntry, IDictionary<object, object> items)
		{
			Entries.Add(entityEntry);
		}
	}
}