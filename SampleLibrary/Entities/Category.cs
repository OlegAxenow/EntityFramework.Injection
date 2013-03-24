using EntityFramework.Inject.Localization;

namespace SampleLibrary.Entities
{
	public class Category
	{
		public int Id { get; set; }

		public LocalizedStrings CategoryName { get; set; }

		public ComputedLocalizedStrings CategoryComputed { get; set; }

		public string NotLocalizedName { get; set; }
	}
}