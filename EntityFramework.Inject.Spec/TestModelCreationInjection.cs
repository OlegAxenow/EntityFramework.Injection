using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using Method.Inject;

namespace EntityFramework.Inject.Spec
{
	public class TestModelCreationInjection : MethodInjection, IModelCreationInjection
	{
		public static readonly MethodInfo SetInitializer = typeof(Database).GetMethod("SetInitializer", BindingFlags.Static | BindingFlags.Public);

		public TestModelCreationInjection()
		{
			ModelBuilders = new List<DbModelBuilder>();
			Contexts = new List<DbContext>();
		}

		public List<DbModelBuilder> ModelBuilders { get; private set; }

		public List<DbContext> Contexts { get; private set; }

		public void OnModelCreating(DbModelBuilder modelBuilder, DbContext context)
		{
			context.SetInitializer(t => null);
			ModelBuilders.Add(modelBuilder);
			Contexts.Add(context);
		}
	}
}