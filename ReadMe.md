## EntityFramework.Injection: inject a dependency into Entity Framework

The goal of this project is to provide a way to inject a dependency into Entity Framework's DbContext.

## What does that mean?

For example, you can even conditionally setup ComplexType to allow localization of data on the fly. This is the most powerful feature of this library, because *you cannot do it without dynamic code generation*. It will be explained in the "OnModelCreating" section below.

## Getting started

You can build it from source or install as NuGet package:

	PM> Install-Package EntityFramework.Inject

You can inject dependency into the any **DbContext** without changing it with the the help of few lines of code:

```cs
var factory = new DbContextFactory();
var injectionSet = new InjectionSet(new MyModelCreationInjection(), new MySaveChangesInjection());
	
using (var context = factory.Create<BasicDbContext>(injectionSet)) ...
```

Of course, `DbContextFactory's` initialization as well as `InjectionSet`, should be done in other place. Actually, you should, usually, get `DbContextFactory` and `InjectionSet` inside some helper/factory method and use something like this in regular code:

```cs
using (var context = Db.Get<BasicDbContext>()) ...
```

Such approach useful not only for injection, but for using DbContext in some scope (e.g. HttpRequest.Items for ASP.NET).

## Requirements and dependencies

License: [MIT](http://opensource.org/licenses/MIT).

This project supports Visual Studio 2010 and 2012 and both .NET 4.0 and .NET 4.5. For .NET 4.0 and Visual Studio 2010 you should use EntityFramework.Injection.net40.sln.

The source code depends on following NuGet packages:

- EntityFramework
- NUnit (only for EntityFramework.Inject.Spec)
- [Method.Injection](https://github.com/OlegAxenow/Method.Injection) (provides basic method builders and injections functionality)

## Performance

Behind the scenes injection implemented with the help of Reflection.Emit. It allow to do injection fast enough. See "Advanced" section for example of generated context.

## Types of injection

At this moment dependency injection supported for the three DbContext's methods. You can see implementation details 

### OnModelCreating

You can inject dependency into this method with the help of `IModelCreationInjection.OnModelCreating`. This method will be called after calling base method.
This is the **main advantage** of this library, because it allow *to setup model differently for ONE DbContext depending on conditions.* As far as I know, [Dynamic Proxy](http://www.castleproject.org/projects/dynamicproxy/) does not fit this need, because of new class creation for each DbContext creation.

Let me explain the details. Normally, you can setup model, e.g. `ComplexType` only once for `DbContext`. But for some complex tasks, like localization of data, you can want to setup some `ComplexTypes` depending on some parameters like `Thread.CurrentUICulture`.

**Registering builder with `MethodBuilderRegistry` is mandatory step.**
In `DataLocalizationInjectionSpec` you can see how to register method builder for two complex type's localization:

	MethodBuilderRegistry.Register<IDataLocalizationInjection>(
		new LocalizationModelCreationBuilder<LocalizedStrings, ComputedLocalizedStrings>());

We need to use two complex types to process both normal and computed database fields.
Each complex type has following properties:

		public string Value { get; set; }

		public string Value1 { get; set; }
		public string Value2 { get; set; }
		public string Value3 { get; set; }
		public string Value4 { get; set; }
		public string Value5 { get; set; }

We should use database table columns like these:

	[Name_1]     NVARCHAR (100) NULL,
    [Name_2]     NVARCHAR (100) NULL,
    [Name_3]     NVARCHAR (100) NULL,
    [Name_4]     NVARCHAR (100) NULL,
    [Name_5]     NVARCHAR (100) NULL,

Then, we need to create localization injection with locale index.
Zero locale index cause using all indexed properties (default property ignored) and 1-5 cause using appropriate column for default property.

As a result, when we create context with new DataLocalizationInjection(1) and access LocalizedStrings.Value property, this property will be mapped to "Name_1" column. For DataLocalizationInjection(2) this property will be mapped to "Name_2" column and so on.
When we need to show or edit all NameX fields, we should use DataLocalizationInjection(0) and corresponding "ValueX" properties.

### SaveChanges

This method corresponds to `ISaveChangesInjection`.
You can see the sample in `ComplexTypeInitializationInjection`. This injection allows to not to care about initializing properties with LocalizedStrings or ComputedLocalizedStrings types.

### ValidateEntity

This method corresponds to `IEntityValidationInjection`. You can use `IEntityValidationInjection` as the single point for global validation (e.g. for DateTimeKind.Utc checking).