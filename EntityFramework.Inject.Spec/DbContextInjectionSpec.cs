using NUnit.Framework;

namespace EntityFramework.Inject.Spec
{
	[TestFixture]
	public class DbContextInjectionSpec
	{
		[Test]
		public void Type_name_should_be_unique_key()
		{
			// arrange
			var injection = new TestEntityValidationInjection();
			
			// act + assert
			Assert.That(injection.UniqueKey, Is.EqualTo("TestEntityValidationInjection"));
		}
	}
}