using EntityFramework.Inject.Localization;

namespace SampleLibrary.Entities
{
	public class Category
	{
		public int Id { get; set; }

		public LocalizedStrings3 CategoryName { get; set; }

		public ComputedLocalizedStrings3 CategoryComputed { get; set; }

		public string NotLocalizedName { get; set; }
	}
}