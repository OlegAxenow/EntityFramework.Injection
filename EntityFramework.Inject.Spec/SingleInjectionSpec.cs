using System;
using System.Linq;
using EntityFramework.Inject.Emit;
using Method.Inject;
using NUnit.Framework;
using SampleLibrary.Entities;

namespace EntityFramework.Inject.Spec
{
	[TestFixture]
	public class SingleInjectionSpec
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			MethodBuilderRegistry.Register<IModelCreationInjection>(new ModelCreationBuilder());
			MethodBuilderRegistry.Register<ISaveChangesInjection>(new SaveChangesBuilder());
			MethodBuilderRegistry.Register<IEntityValidationInjection>(new EntityValidationBuilder());
		}

		[Test]
		public void EntityValidation_should_be_injectable()
		{
			// arrange
			var injection = new TestEntityValidationInjection();
			var injectionSet = new InjectionSet(injection);
			var factory = new DbContextFactory();

			// act
			using (var context = Create(factory, injectionSet))
			{
				context.ActionTypes.Add(new ActionType());
				
				context.GetValidationErrors();
			}

			// assert
			Assert.That(injection.Entries.Count, Is.EqualTo(1));
		}

		[Test]
		public void SaveChanges_should_be_injectable()
		{
			// arrange
			var injection = new TestSaveChangesInjection();
			var injectionSet = new InjectionSet(injection);
			var factory = new DbContextFactory();
			int count;

			// act
			using (var context = Create(factory, injectionSet))
			{
				count = context.SaveChanges();
			}

			// assert
			Assert.That(injection.BeforeList.Count, Is.EqualTo(1));
			Assert.That(injection.AfterList.Count, Is.EqualTo(1));
			Assert.That(injection.AfterList[0], Is.EqualTo(injection.BeforeList[0]));
			
			Assert.That(count, Is.EqualTo(0));
		}

		[Test]
		public void ModelCreation_should_be_injectable_and_called_once()
		{
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			
			// arrange
			var injection = new TestModelCreationInjection();
			var injectionSet = new InjectionSet(injection);
			var factory = new DbContextFactory();

			// act
			using (var context1 = Create(factory, injectionSet))
			{
				context1.ActionTypes.Count();
			}

			using (var context2 = Create(factory, injectionSet))
			{
				context2.ActionTypes.Count();
			}

			// assert
			Assert.That(injection.ModelBuilders.Count, Is.EqualTo(1));
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		[Test]
		public void Should_be_only_one_type_for_one_injection_set()
		{
			// arrange
			var injectionSet = new InjectionSet(new TestModelCreationInjection());
			var factory = new DbContextFactory();
			Type type1;
			Type type2;

			// act
			using (var context1 = Create(factory, injectionSet))
			{
				type1 = context1.GetType();
			}

			using (var context2 = Create(factory, injectionSet))
			{
				type2 = context2.GetType();
			}

			// assert
			Assert.That(type1, Is.EqualTo(type2));
		}

		private static BasicDbContext Create(DbContextFactory factory, InjectionSet injectionSet)
		{
			return factory.Create<BasicDbContext>(injectionSet, "EntityFrameworkInject");
		}
	}
}