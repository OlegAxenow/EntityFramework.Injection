using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Inject.Localization;

namespace SampleLibrary.Entities
{
	[Table("Categories")]
	public class ProtectedCategory
	{
		public void InitComputed()
		{
			CategoryComputed = new ComputedLocalizedStrings();
		}

		public int Id { get; set; }

		public LocalizedStrings CategoryName { get; set; }

		public ComputedLocalizedStrings CategoryComputed { get; protected set; }

		public string NotLocalizedName { get; set; }
	}
}