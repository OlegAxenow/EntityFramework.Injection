using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Inject.Localization
{
	public class LocalizedPropertyNamingConvention : ILocalizedPropertyNamingConvention
	{
		private readonly string _localeSeparator;

		public LocalizedPropertyNamingConvention(string localeSeparator)
		{
			if (localeSeparator == null) throw new ArgumentNullException("localeSeparator");
			if (localeSeparator.Length == 0) throw new ArgumentOutOfRangeException("localeSeparator");
			_localeSeparator = localeSeparator;
		}

		public virtual string GetPropertyName(int localeIndex)
		{
			return MainPropertyName + localeIndex;
		}

		public string GetDbColumnName(PropertyInfo property, int localeIndex)
		{
			if (localeIndex < 1) throw new ArgumentOutOfRangeException("localeIndex");

			if (property == null) throw new ArgumentNullException("property");

			var dbColumnName = GetDbColumnName(property);
			return GetLocalizedDbColumnName(dbColumnName, localeIndex);
		}

		protected virtual string GetLocalizedDbColumnName(string dbColumnName, int localeIndex)
		{
			return dbColumnName + _localeSeparator + localeIndex;
		}

		private string GetDbColumnName(PropertyInfo property)
		{
			var columnAttribute = property
				.GetCustomAttributes(typeof(ColumnAttribute), true).Cast<ColumnAttribute>().FirstOrDefault();

			if (columnAttribute == null || string.IsNullOrEmpty(columnAttribute.Name))
				return property.Name;

			return columnAttribute.Name;
		}

		public string MainPropertyName
		{
			get { return "Value"; }
		}
	}
}