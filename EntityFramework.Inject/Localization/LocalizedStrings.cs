using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Inject.Localization
{
	/// <summary>
	/// Complex type for localization. <seealso cref="ILocalizedPropertyNamingConvention"/> <seealso cref="IDataLocalizationInjection"/></summary>
	/// <remarks> For computed columns use <see cref="ComputedLocalizedStrings"/>.
	/// For nested properties <see cref="StringLengthAttribute"/> or <see cref="MaxLengthAttribute"/> should not be specified, 
	/// because sharing complex types across all entities.</remarks>
	[ComplexType]
	public class LocalizedStrings
	{
		/// <summary>
		/// Default property, if <see cref="IDataLocalizationInjection.LocaleIndex"/> more than zero 
		/// (column with appropriate index used, but all indexed properties will be ignored).
		/// Otherwise this property will be ignored.</summary>
		public string Value { get; set; }

		/// <summary>
		/// Property for column with index 1 (if <see cref="IDataLocalizationInjection.LocaleIndex"/> equals to zero). 
		/// If <see cref="IDataLocalizationInjection.LocaleIndex"/> more than zero -
		/// column with appropriate index will be used for <see cref="Value"/> and this property will be ignored.</summary>
		public string Value1 { get; set; }

		/// <summary>
		/// Property for column with index 2 (if <see cref="IDataLocalizationInjection.LocaleIndex"/> equals to zero). 
		/// If <see cref="IDataLocalizationInjection.LocaleIndex"/> more than zero -
		/// column with appropriate index will be used for <see cref="Value"/> and this property will be ignored.</summary>
		public string Value2 { get; set; }

		/// <summary>
		/// Property for column with index 3 (if <see cref="IDataLocalizationInjection.LocaleIndex"/> equals to zero). 
		/// If <see cref="IDataLocalizationInjection.LocaleIndex"/> more than zero -
		/// column with appropriate index will be used for <see cref="Value"/> and this property will be ignored.</summary>
		public string Value3 { get; set; }

		/// <summary>
		/// Property for column with index 4 (if <see cref="IDataLocalizationInjection.LocaleIndex"/> equals to zero). 
		/// If <see cref="IDataLocalizationInjection.LocaleIndex"/> more than zero -
		/// column with appropriate index will be used for <see cref="Value"/> and this property will be ignored.</summary>
		public string Value4 { get; set; }

		/// <summary>
		/// Property for column with index 5 (if <see cref="IDataLocalizationInjection.LocaleIndex"/> equals to zero). 
		/// If <see cref="IDataLocalizationInjection.LocaleIndex"/> more than zero -
		/// column with appropriate index will be used for <see cref="Value"/> and this property will be ignored.</summary>
		public string Value5 { get; set; }

		public override string ToString()
		{
			return Value;
		}

		public static implicit operator string(LocalizedStrings localizedStrings)
		{
			return localizedStrings.Value;
		}

		public static implicit operator LocalizedStrings(string str)
		{
			return new LocalizedStrings { Value = str };
		}
	}
}