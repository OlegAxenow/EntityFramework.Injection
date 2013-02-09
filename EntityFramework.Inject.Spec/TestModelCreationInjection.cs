using System.Collections.Generic;
using System.Data.Entity;
using Method.Inject;

namespace EntityFramework.Inject.Spec
{
	public class TestModelCreationInjection : MethodInjection, IModelCreationInjection
	{
		public TestModelCreationInjection()
		{
			ModelBuilders = new List<DbModelBuilder>();
		}

		public List<DbModelBuilder> ModelBuilders { get; private set; }

		public void OnModelCreating(DbModelBuilder modelBuilder)
		{
			ModelBuilders.Add(modelBuilder);
		}
	}
}