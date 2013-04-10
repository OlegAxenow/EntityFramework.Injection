using System;
using System.Data.Entity;
using System.Linq.Expressions;
using EntityFramework.Inject.Localization;

namespace EntityFramework.Inject
{
	/// <summary>
	/// Supports data localization with one of the <see cref="ILocalizedPropertyNamingConvention"/>.
	/// </summary>
	public interface IDataLocalizationInjection : IModelCreationInjection
	{
		/// <summary>
		/// Index of locale to use. 
		/// 0 cause using all indexed properties (default property ignored) and 1-5 cause using appropriate column for default property.
		/// </summary>
		int LocaleIndex { get; }

		void IgnoreProperty<TComplexType>(DbModelBuilder modelBuilder,
			Expression<Func<TComplexType, string>> expression) where TComplexType : class;

		void ConfigureProperty<TEntity>(DbModelBuilder modelBuilder,
			Expression<Func<TEntity, string>> expression) where TEntity : class;
	}
}