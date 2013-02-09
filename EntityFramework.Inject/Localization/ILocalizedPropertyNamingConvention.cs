using System.Reflection;

namespace EntityFramework.Inject.Localization
{
	/// <summary>
	/// Provides names for localized properties and columns.
	/// </summary>
	public interface ILocalizedPropertyNamingConvention
	{
		/// <summary>
		/// Returns localized property name for the specified locale's index within complex type. E.g. "Value2" for locale's index 2.
		/// </summary>
		string GetPropertyName(int localeIndex);

		/// <summary>
		/// Returns localized column name for the specified locale's index. E.g. "Name_2" for locale's index 2.
		/// </summary>
		string GetDbColumnName(PropertyInfo property, int localeIndex);

		/// <summary>
		/// Returns main (for main locale) property name within complex type. E.g. "Value".
		/// </summary>
		string MainPropertyName { get; }
	}
}