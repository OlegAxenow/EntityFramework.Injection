using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Inject.Localization;

namespace SampleLibrary.Entities
{
	[Table("Category")]
	public class ProtectedCategory
	{
		public void InitComputed()
		{
			CategoryComputed = new ComputedLocalizedStrings();
		}

		[Key]
		public int CategoryID { get; set; }

		public LocalizedStrings CategoryName { get; set; }

		public ComputedLocalizedStrings CategoryComputed { get; protected set; }

		public string NotLocalizedName { get; set; }
	}
}