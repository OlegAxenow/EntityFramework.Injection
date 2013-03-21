using System.ComponentModel.DataAnnotations;
using EntityFramework.Inject.Localization;

namespace SampleLibrary.Entities
{
	public class Category
	{
		[Key]
		public int CategoryID { get; set; }

		public LocalizedStrings CategoryName { get; set; }

		public ComputedLocalizedStrings CategoryComputed { get; set; }

		public string NotLocalizedName { get; set; }
	}
}