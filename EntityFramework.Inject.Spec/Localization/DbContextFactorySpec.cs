using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Inject.Emit;
using EntityFramework.Inject.Localization;
using EntityFramework.Inject.Spec.Entities;
using EntityFramework.Inject.Spec.Helpers;
using Method.Inject;
using NUnit.Framework;
using SampleLibrary.Entities;

namespace EntityFramework.Inject.Spec.Localization
{
	[TestFixture]
	public class DbContextFactorySpec
	{
		private DbContextFactory _factory;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_factory = new DbContextFactory();
			MethodBuilderRegistry.Register<IDataLocalizationInjection>(
				new LocalizationModelCreationBuilder<LocalizedStrings3, ComputedLocalizedStrings3>());
		}

		private T Create<T>(int localeIndex = 0) where T : DbContext
		{
			var injection = new DataLocalizationInjection(localeIndex);
			return _factory.Create<T>(new InjectionSet(injection), "EntityFrameworkInject");
		}

		[Test]
		public void Zero_index_should_not_produce_the_same_context()
		{
			using (var context = Create<BasicDbContext>())
			{
				Assert.That(context.GetType(), Is.Not.EqualTo(typeof(BasicDbContext)));
			}
		}

		[Test]
		public void Same_context_creation_should_not_produce_new_assembly()
		{
			using (Create<TestDbContext>(1))
			{
			}

			int assemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;
			using (Create<TestDbContext>(1))
			{
			}

			Assert.That(AppDomain.CurrentDomain.GetAssemblies().Length, Is.EqualTo(assemblyCount));
		}

		[Test]
		public void Types_should_be_reused_with_different_combinations()
		{
			var injection = new DataLocalizationInjection(1);
			
			int assemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;

			var types = _factory.CreateTypes(new InjectionSet(injection), null, typeof(TestDbContext1), typeof(TestDbContext2));
			Assert.That(types.Length, Is.EqualTo(2));
			Assert.That(AppDomain.CurrentDomain.GetAssemblies().Length, Is.EqualTo(assemblyCount + 1));
			var type1 = types[0];
			var type2 = types[1];

			types = _factory.CreateTypes(new InjectionSet(injection), null, typeof(TestDbContext3), typeof(TestDbContext2));
			Assert.That(types.Length, Is.EqualTo(2));
			Assert.That(types[1], Is.EqualTo(type2));
			
			Assert.That(AppDomain.CurrentDomain.GetAssemblies().Length, Is.EqualTo(assemblyCount + 2));

			types = _factory.CreateTypes(new InjectionSet(injection), null, typeof(TestDbContext1), typeof(TestDbContext3));
			Assert.That(types.Length, Is.EqualTo(2));
			Assert.That(types[0], Is.EqualTo(type1));
			
			Assert.That(AppDomain.CurrentDomain.GetAssemblies().Length, Is.EqualTo(assemblyCount + 2), "all types should be already created");
		}

		[Test]
		public void Concurrent_threads_should_not_cause_errors()
		{
			var helper = new MultiThreadTestHelper(() =>
			{
				using (Create<TestConcurrencyDbContext>(1))
				{
				}

				int length =
					AppDomain.CurrentDomain.GetAssemblies().Count(x => x.GetTypes().Any(t => t.Name.StartsWith("TestConcurrencyDbContext_")));
				Assert.That(length, Is.EqualTo(1));
			});
			helper.Run(10);
		}

	    [Test]
	    public void Should_pass_constructor_parameter_to_context()
	    {
            var injectionSet = new InjectionSet();
	        var stringParameter = "stringParameter";
	        var objectParameter = (object) "objectParameter";
            var classParameter = new InjectionSet();
	        var interfaceParameter = (IEnumerable<int>)new[] {1, 2, 3};

	        var context = _factory.Create<TestDbContext>(injectionSet, new [] { "EntityFrameworkInject", stringParameter, objectParameter, classParameter, interfaceParameter } );

            Assert.That(context.StringParameter, Is.EqualTo(stringParameter));
            Assert.That( context.ObjectParameter, Is.EqualTo( objectParameter ) );
            Assert.That( context.ClassParameter, Is.EqualTo( classParameter ) );
            Assert.That( context.InterfaceParameter, Is.EqualTo( interfaceParameter ) );
        }
	}
}