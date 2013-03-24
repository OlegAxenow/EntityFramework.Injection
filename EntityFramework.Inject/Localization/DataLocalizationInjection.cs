using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFramework.Inject.Localization
{
	/// <summary>
	/// Supports data localization with one of the <see cref="ILocalizedPropertyNamingConvention"/>.
	/// </summary>
	public class DataLocalizationInjection : IDataLocalizationInjection
	{
		private const int ValuePropertiesCount = 5;
		private readonly ILocalizedPropertyNamingConvention _convention;
		private readonly int _localeIndex;
		private readonly Func<Type, object> _createInitializer;

		/// <summary>
		/// Initializes injection with required parameters.
		/// </summary>
		/// <param name="convention">One of the <see cref="ILocalizedPropertyNamingConvention"/> implementation.</param>
		/// <param name="localeIndex">Index of locale to use. 
		/// 0 cause using all indexed properties (default property ignored) and 1-5 cause using appropriate column for default property.</param>
		/// <param name="createInitializer">Optional <see cref="Func{T1,TResult}"/> to create <see cref="IDatabaseInitializer{TContext}"/>.
		/// If not specified null will be used.</param>
		public DataLocalizationInjection(ILocalizedPropertyNamingConvention convention, int localeIndex,
			Func<Type, object> createInitializer = null)
		{
			if (convention == null) throw new ArgumentNullException("convention");
			if (localeIndex < 0 || localeIndex > ValuePropertiesCount) throw new ArgumentOutOfRangeException("localeIndex");

			_convention = convention;
			_localeIndex = localeIndex;
			_createInitializer = createInitializer ?? (t => null);
		}

		public void ConfigureProperty<TEntity>(DbModelBuilder modelBuilder, 
			Expression<Func<TEntity, string>> expression) where TEntity : class
		{
			if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new ArgumentOutOfRangeException("expression");
			
			var memberExpression = (MemberExpression)expression.Body;
			var valuePropertyInfo = memberExpression.Member as PropertyInfo;

			if (valuePropertyInfo == null) throw new ArgumentOutOfRangeException("expression");
			var valuePropertyName = valuePropertyInfo.Name;

			if (memberExpression.Expression.NodeType != ExpressionType.MemberAccess) throw new ArgumentOutOfRangeException("expression");
			var complexPropertyInfo = ((MemberExpression)memberExpression.Expression).Member as PropertyInfo;

			if (complexPropertyInfo == null) throw new ArgumentOutOfRangeException("expression");

			if (_localeIndex == 0)
			{
				// map indexed properties
				if (valuePropertyName.Length > "Value".Length)
				{
					var index = int.Parse(valuePropertyName.Substring("Value".Length));
					modelBuilder.Entity<TEntity>().Property(expression).HasColumnName(_convention.GetDbColumnName(complexPropertyInfo, index));
				}
			}
			else
			{
				// map default property to appropriate index
				if (valuePropertyName == "Value")
					modelBuilder.Entity<TEntity>().Property(expression).HasColumnName(_convention.GetDbColumnName(complexPropertyInfo, _localeIndex));
			}
		}

		public void OnModelCreating(DbModelBuilder modelBuilder, DbContext context)
		{
			context.SetInitializer(_createInitializer);
			IgnoreProperties(modelBuilder, _localeIndex == 0);
		}

		private static void IgnoreProperties(DbModelBuilder modelBuilder, bool defaultOnly)
		{
			if (defaultOnly)
			{
				modelBuilder.ComplexType<LocalizedStrings>().Ignore(x => x.Value);
				modelBuilder.ComplexType<ComputedLocalizedStrings>().Ignore(x => x.Value);
			}
			else
			{
				modelBuilder.ComplexType<LocalizedStrings>().Ignore(x => x.Value1);
				modelBuilder.ComplexType<LocalizedStrings>().Ignore(x => x.Value2);
				modelBuilder.ComplexType<LocalizedStrings>().Ignore(x => x.Value3);
				modelBuilder.ComplexType<LocalizedStrings>().Ignore(x => x.Value4);
				modelBuilder.ComplexType<LocalizedStrings>().Ignore(x => x.Value5);

				modelBuilder.ComplexType<ComputedLocalizedStrings>().Ignore(x => x.Value1);
				modelBuilder.ComplexType<ComputedLocalizedStrings>().Ignore(x => x.Value2);
				modelBuilder.ComplexType<ComputedLocalizedStrings>().Ignore(x => x.Value3);
				modelBuilder.ComplexType<ComputedLocalizedStrings>().Ignore(x => x.Value4);
				modelBuilder.ComplexType<ComputedLocalizedStrings>().Ignore(x => x.Value5);
			}
		}

		public string UniqueKey
		{
			get { return "DataLocalizationInjection" + _convention.GetType().Name + _localeIndex; }
		}

		public int LocaleIndex
		{
			get { return _localeIndex; }
		}
	}
}