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
		private readonly ILocalizedPropertyNamingConvention _convention;
		private readonly int _localeIndex;
		private readonly Func<Type, object> _createInitializer;

		/// <summary>
		/// Initializes injection with required parameters.
		/// </summary>
		/// <param name="localeIndex">Index of locale to use. 
		///     0 cause using all indexed properties (default property ignored) and 1-5 cause using appropriate column for default property.</param>
		/// <param name="convention">One of the <see cref="ILocalizedPropertyNamingConvention"/> implementation. 
		/// If not specified, new LocalizedPropertyNamingConvention("_") will be used.</param>
		/// <param name="createInitializer">Optional <see cref="Func{T1,TResult}"/> to create <see cref="IDatabaseInitializer{TContext}"/>.
		/// If not specified null will be used.</param>
		public DataLocalizationInjection(int localeIndex, ILocalizedPropertyNamingConvention convention = null, Func<Type, object> createInitializer = null)
		{
			if (localeIndex < 0) throw new ArgumentOutOfRangeException("localeIndex");
			if (convention == null) convention = new LocalizedPropertyNamingConvention("_");

			_convention = convention;
			_localeIndex = localeIndex;
			_createInitializer = createInitializer ?? (t => null);
		}

		public void ConfigureProperty<TEntity>(DbModelBuilder modelBuilder, 
			Expression<Func<TEntity, string>> expression) where TEntity : class
		{
			var memberExpression = GetMemberExpression(expression);
			var valuePropertyName = GetValuePropertyName(memberExpression);

			if (memberExpression.Expression.NodeType != ExpressionType.MemberAccess) throw new ArgumentOutOfRangeException("expression");
			var complexPropertyInfo = ((MemberExpression)memberExpression.Expression).Member as PropertyInfo;

			if (complexPropertyInfo == null) throw new ArgumentOutOfRangeException("expression");

			if (_localeIndex == 0)
			{
				// map indexed properties
				if (valuePropertyName.Length > "Value".Length)
				{
					var index = GetPropertyIndex(valuePropertyName);
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

		private static MemberExpression GetMemberExpression<TEntity>(Expression<Func<TEntity, string>> expression)
		{
			if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new ArgumentOutOfRangeException("expression");

			var memberExpression = (MemberExpression)expression.Body;
			return memberExpression;
		}

		public void IgnoreProperty<TComplexType>(DbModelBuilder modelBuilder,
			Expression<Func<TComplexType, string>> expression) where TComplexType : class
		{
			var memberExpression = GetMemberExpression(expression);
			var valuePropertyName = GetValuePropertyName(memberExpression);

			if (_localeIndex == 0)
			{
				// ignore default property
				if (valuePropertyName == "Value")
					modelBuilder.ComplexType<TComplexType>().Ignore(expression);	
			}
			else
			{
				// ignore indexed properties
				if (valuePropertyName.Length > "Value".Length)
					modelBuilder.ComplexType<TComplexType>().Ignore(expression);
			}
		}

		private static string GetValuePropertyName(MemberExpression expression)
		{
			var valuePropertyInfo = expression.Member as PropertyInfo;

			if (valuePropertyInfo == null) throw new ArgumentOutOfRangeException("expression");
			var valuePropertyName = valuePropertyInfo.Name;
			return valuePropertyName;
		}

		private static int GetPropertyIndex(string valuePropertyName)
		{
			return int.Parse(valuePropertyName.Substring("Value".Length));
		}

		public void OnModelCreating(DbModelBuilder modelBuilder, DbContext context)
		{
			context.SetInitializer(_createInitializer);
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