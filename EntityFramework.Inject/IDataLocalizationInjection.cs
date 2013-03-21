using System;
using System.Data.Entity;
using System.Linq.Expressions;

namespace EntityFramework.Inject
{
	public interface IDataLocalizationInjection : IModelCreationInjection
	{
		void ConfigureProperty<TEntity>(DbModelBuilder modelBuilder,
			Expression<Func<TEntity, string>> expression) where TEntity : class;
	}
}