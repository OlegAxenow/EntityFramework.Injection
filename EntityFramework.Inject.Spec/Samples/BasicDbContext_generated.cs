using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Method.Inject;
using SampleLibrary.Entities;

// ReSharper disable ForCanBeConvertedToForeach
namespace EntityFramework.Inject.Spec.Samples
{
	/// <summary>
	/// Sample for context generated with Reflection.Emit.
	/// </summary>
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

			var injections = _injectionSet.GetInjections<IDataLocalizationInjection>();

			for (int i = 0; i < injections.Length; i++)
			{
				injections[i].OnModelCreating(modelBuilder, this);

				if (injections[i].LocaleIndex == 0)
				{
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value1);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value2);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value3);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value4);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value5);

					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryComputed.Value1);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryComputed.Value2);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryComputed.Value3);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryComputed.Value4);
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryComputed.Value5);
				}
				else
				{
					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value);

					injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryComputed.Value);
				}
			}
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

		public DbSet<Category> Categories { get; set; }
	}
}