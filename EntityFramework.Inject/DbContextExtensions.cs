using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Inject
{
	public static class DbContextExtensions
	{
		private static readonly MethodInfo SetInitializerInfo = typeof(Database).GetMethod("SetInitializer", BindingFlags.Static | BindingFlags.Public);

		/// <summary>
		/// Returns all <see cref="DbEntityEntry"/> from <paramref name="context"/> with specified <see cref="EntityState"/> 
		/// (all changed by default).</summary>
		/// <remarks> We can use <see cref="ObjectStateEntry"/>, if needed (with <see cref="IObjectContextAdapter"/>.
		/// But we should touch <see cref="DbChangeTracker.Entries()"/> anyway (due to EF implementation).</remarks>
		public static DbEntityEntry[] GetDbStateEntries(this DbContext context,
			EntityState state = EntityState.Added | EntityState.Modified | EntityState.Deleted)
		{
			if (context == null) return new DbEntityEntry[0];
			// returns array to avoid parallel calls problems
			return context.ChangeTracker.Entries().Where(x => (x.State & state) != 0).ToArray();
		}

		public static void SetInitializer(this DbContext context, Func<Type, object> createInitializer)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (createInitializer == null) throw new ArgumentNullException("createInitializer");

			SetInitializerInfo.MakeGenericMethod(context.GetType()).Invoke(null, new[] { createInitializer(context.GetType()) });
		}
	}
}