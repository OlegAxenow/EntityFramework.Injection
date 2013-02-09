## Add your own method builders

In rare cases you can want to add new implementation of IMethodBuilder.

For Entity Framework 5.0 a reasonable IMethodBuilder's already implemented. But you still can to override some behavior with your own implementation.
You can use SaveChangesBuilder as an example and don't forget to use EmitHelper, if applicable.

## Implementation details

Each `IDbContextInjection` should correspond to one `IMethodBuilder` with the help of `MethodBuilderRegistry`.

`DbContextFactory` uses `InjectedAssemblyBuilder` to build dynamic assembly with one or more types (see `Append` method).
`InjectedAssemblyBuilder` in its turn, uses `InjectedTypeBuilder` to build class with constructor and `InjectionSet` field and calls each IMethodBuilder to add one method to generated context.

Generated class can looks like this:

```cs
public class BasicDbContext_generated : BasicDbContext
	{
		private readonly InjectionSet _injectionSet;

		public BasicDbContext_generated(InjectionSet ps, string name) : base(name)
		{
			_injectionSet = ps;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var injections = _injectionSet.GetInjections<IModelCreationInjection>();

			for (int i = 0; i < injections.Length; i++)
				injections[i].OnModelCreating(modelBuilder);
		}

		public override int SaveChanges()
		{
			var injections = _injectionSet.GetInjections<ISaveChangesInjection>();

			for (int i = 0; i < injections.Length; i++)
				injections[i].OnBeforeSaveChanges(this);
			
			int result = base.SaveChanges();

			for (int i = 0; i < injections.Length; i++)
				injections[i].OnBeforeSaveChanges(this);
			
			return result;
		}

		protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
		{
			var result = base.ValidateEntity(entityEntry, items);

			var injections = _injectionSet.GetInjections<IEntityValidationInjection>();

			for (int i = 0; i < injections.Length; i++)
				injections[i].OnValidateEntity(result, entityEntry, items);
		
			return result;
		}
	}
```