using System;
using System.Data.Entity;

namespace EntityFramework.Inject.Localization
{
	/// <summary>
	/// Supports data localization with one of the <see cref="ILocalizedPropertyNamingConvention"/>.
	/// </summary>
	public class DataLocalizationInjection<TComplexType> : IModelCreationInjection
	{
		private readonly ILocalizedPropertyNamingConvention _convention;
		private readonly int _localeIndex;

		public DataLocalizationInjection(ILocalizedPropertyNamingConvention convention, int localeIndex)
		{
			if (convention == null) throw new ArgumentNullException("convention");
			_convention = convention;
			_localeIndex = localeIndex;
		}

		public void OnModelCreating(DbModelBuilder modelBuilder)
		{
			throw new NotImplementedException();
			// modelBuilder.ComplexType<TComplexType>().
		}

		public string UniqueKey
		{
			get { return "DataLocalizationInjection" + typeof(TComplexType).Name + _convention.GetType().Name + _localeIndex; }
		}
	}
}