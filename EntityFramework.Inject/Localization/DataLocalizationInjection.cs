using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFramework.Inject.Localization
{
	/// <summary>
	/// Supports data localization with one of the <see cref="ILocalizedPropertyNamingConvention"/>.
	/// </summary>
	public class DataLocalizationInjection : IDataLocalizationInjection
	{
		private readonly ILocalizedPropertyNamingConvention _convention;
		private readonly int _localeIndex;

		/// <summary>
		/// Initializes injection with required parameters.
		/// </summary>
		/// <param name="convention">One of the <see cref="ILocalizedPropertyNamingConvention"/> implementation.</param>
		/// <param name="localeIndex">Index of locale to use. 
		/// 0 cause using all indexed properties (default property ignored) and 1-5 cause using appropriate column for default property.</param>
		public DataLocalizationInjection(ILocalizedPropertyNamingConvention convention, int localeIndex)
		{
			if (convention == null) throw new ArgumentNullException("convention");
			if (localeIndex < 0) throw new ArgumentOutOfRangeException("localeIndex");

			_convention = convention;
			_localeIndex = localeIndex;
		}

		public void ConfigureProperty<TEntity>(DbModelBuilder modelBuilder, 
			Expression<Func<TEntity, string>> expression) where TEntity : class
		{
			var propertyConfiguration = modelBuilder.Entity<TEntity>().Property(expression);

			if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new ArgumentOutOfRangeException("expression");
			var memberExpression = (MemberExpression)expression.Body;
			var valuePropertyInfo = memberExpression.Member as PropertyInfo;

			if (valuePropertyInfo == null) throw new ArgumentOutOfRangeException("expression");

			if (memberExpression.Expression.NodeType != ExpressionType.MemberAccess) throw new ArgumentOutOfRangeException("expression");
			var complexPropertyInfo = ((MemberExpression)memberExpression.Expression).Member as PropertyInfo;

			if (complexPropertyInfo == null) throw new ArgumentOutOfRangeException("expression");

			ConfigureProperty(complexPropertyInfo, valuePropertyInfo.Name, propertyConfiguration);
		}

		public void ConfigureProperty(PropertyInfo complexProperty, string valuePropertyName, StringPropertyConfiguration propertyConfiguration)
		{
			if (_localeIndex == 0)
			{
				// map indexed properties
				if (valuePropertyName.Length > "Value".Length)
				{
					var index = int.Parse(valuePropertyName.Substring("Value".Length));
					propertyConfiguration.HasColumnName(_convention.GetDbColumnName(complexProperty, index));
				}
			}
			else
			{
				// map default property to appropriate index
				if (valuePropertyName == "Value")
					propertyConfiguration.HasColumnName(_convention.GetDbColumnName(complexProperty, _localeIndex));
			}
		}

		public void OnModelCreating(DbModelBuilder modelBuilder, DbContext context)
		{
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