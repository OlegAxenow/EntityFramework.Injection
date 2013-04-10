using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using EntityFramework.Inject.Emit;
using EntityFramework.Inject.Localization;
using Method.Inject;
using NUnit.Framework;
using SampleLibrary.Entities;

namespace EntityFramework.Inject.Spec.Localization
{
	[TestFixture]
	public class DataLocalizationInjectionSpec
	{
		private DbContextFactory _factory;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_factory = new DbContextFactory(/*saveAssembliesToDisk: true*/);
			MethodBuilderRegistry.Register<IDataLocalizationInjection>(
				new LocalizationModelCreationBuilder<LocalizedStrings3, ComputedLocalizedStrings3>());
			MethodBuilderRegistry.Register<ISaveChangesInjection>(new SaveChangesBuilder());
		}

		private T Create<T>(int localeIndex = 0) where T : DbContext
		{
			var localizationInjection = new DataLocalizationInjection(localeIndex);
			return _factory.Create<T>(new InjectionSet(localizationInjection, new ComplexTypeInitializationInjection()), "EntityFrameworkInject");
		}

		[Test]
		public void Non_zero_index_should_fill_default_property_and_leave_indexed_properties_empty()
		{
			using (var context = Create<LocalizedDbContext>(1))
			{
				var categories = context.Categories.ToArray();
				AssertValues(categories, c => c.CategoryName.Value, "Beverages", "Condiments", "Confections", "Dairy Products",
					"Grains/Cereals",
					"Meat/Poultry", "Produce", "Seafood");
				AssertComputedValues(categories, 1);
				AssertEmptyIndexedNames(categories);
			}

			using (var context = Create<LocalizedDbContext>(2))
			{
				var categories = context.Categories.ToArray();
				AssertValues(categories, c => c.CategoryName.Value, "1", "2", "3", "4", "5", "6", "7", "8");
				AssertComputedValues(categories, 2);
				AssertEmptyIndexedNames(categories);
			}

			using (var context = Create<LocalizedDbContext>(3))
			{
				var categories = context.Categories.ToArray();
				AssertValues(categories, c => c.CategoryName.Value, "11", "22", "33", "44", "55", "66", "77", "88");
				AssertComputedValues(categories, 3);
				AssertEmptyIndexedNames(categories);
			}
		}

		[Test]
		public void Zero_index_should_return_normal_fields()
		{
			using (var context = Create<LocalizedDbContext>())
			{
				var categories = context.Categories.ToArray();
				AssertAllNulls(categories, c => c.CategoryName.Value);
				AssertValues(categories, c => c.CategoryName.Value1, "Beverages", "Condiments", "Confections", "Dairy Products",
					"Grains/Cereals",
					"Meat/Poultry", "Produce", "Seafood");
				AssertValues(categories, c => c.CategoryName.Value2, "1", "2", "3", "4", "5", "6", "7", "8");
				AssertValues(categories, c => c.CategoryName.Value3, "11", "22", "33", "44", "55", "66", "77", "88");
			}
		}

		[Test]
		public void Insert_should_not_touch_computed_columns()
		{
			// arrange
			using (new TransactionScope())
			{
				using (var context = Create<LocalizedDbContext>())
				{
					// act
					var newCategory = new Category
					{
						CategoryName = new LocalizedStrings3 { Value1 = "c1", Value2 = "c2", Value3 = "c3" }
					};
					context.Categories.Add(newCategory);

					context.SaveChanges();

					// assert
					Assert.That(newCategory.Id, Is.GreaterThan(0));
				}
			}
		}

		[Test]
		public void Insert_should_not_touch_protected_computed_columns()
		{
			// arrange
			using (new TransactionScope())
			{
				using (var context = Create<ProtectedDbContext>())
				{
					// act
					var newCategory = new ProtectedCategory
					{
						CategoryName = new LocalizedStrings3 { Value1 = "c1", Value2 = "c2", Value3 = "c3" }
					};
					context.Categories.Add(newCategory);

					// assert
					Assert.Throws<DbUpdateException>(() => context.SaveChanges());

					newCategory.InitComputed();
					context.SaveChanges();
					Assert.That(newCategory.Id, Is.GreaterThan(0));
				}
			}
		}

		[Test]
		public void Update_should_not_touch_computed_columns()
		{
			using (new TransactionScope())
			{
				using (var context = Create<LocalizedDbContext>())
				{
					var category = context.Categories.First(x => x.Id == 1);

					category.CategoryName.Value1 = "Beverages 2";
					category.NotLocalizedName = "test1";

					context.SaveChanges();

					category = context.Categories.First(x => x.Id == 1);

					Assert.That(category.CategoryName.Value1, Is.EqualTo("Beverages 2"));
					Assert.That(category.CategoryComputed.Value1, Is.EqualTo("value1"));
					Assert.That(category.NotLocalizedName, Is.EqualTo("test1"));

					category.NotLocalizedName = "test2";

					context.SaveChanges();
					Assert.That(category.NotLocalizedName, Is.EqualTo("test2"));
				}
			}
		}

		[TestCase(-1)]
		[TestCase(-1000)]
		[TestCase(50)]
		public void Inproper_locale_index_should_produce_exception(int index)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Create<BasicDbContext>(index));
		}

		private static void AssertComputedValues(Category[] categories, int index)
		{
			var value = "value" + index;
			AssertValues(categories, c => c.CategoryComputed.Value, value, value, value, value, value, value, value, value);
		}

		private static void AssertEmptyIndexedNames(Category[] categories)
		{
			AssertAllNulls(categories, c => c.CategoryName.Value1);
			AssertAllNulls(categories, c => c.CategoryName.Value2);
			AssertAllNulls(categories, c => c.CategoryName.Value3);

			AssertAllNulls(categories, c => c.CategoryComputed.Value1);
			AssertAllNulls(categories, c => c.CategoryComputed.Value2);
			AssertAllNulls(categories, c => c.CategoryComputed.Value3);
		}

		private static void AssertAllNulls(Category[] categories, Func<Category, string> property)
		{
			AssertValues(categories, property, null, null, null, null, null, null, null, null);
		}

		private static void AssertValues(Category[] categories, Func<Category, string> getProperty, params string[] names)
		{
			Assert.That(categories, Is.Not.Null.And.Not.Empty);
			
			for (int index = 0; index < categories.Length; index++)
			{
				Assert.That(getProperty(categories[index]), Is.EqualTo(names[index]));
			}
		}
	}
}