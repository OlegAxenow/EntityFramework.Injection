## EntityFramework.Injection: inject a dependency into Entity Framework

The goal of this project is to provide a way to inject a dependency into Entity Framework's DbContext.

## What does that mean?

For example, you can even conditionally setup ComplexType to allow localization of data on the fly. This is the most powerful feature of this library, because *you cannot do it without dynamic code generation*. It will be explained in the "OnModelCreating" section below.

## Getting started

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

## Performance

Behind the scenes injection implemented with the help of Reflection.Emit. It allow to do injection fast enough. See "Advanced" section for example of generated context.

## Types of injection

At this moment dependency injection supported for the three DbContext's methods. You can see implementation details 

### OnModelCreating

You can inject dependency into this method with the help of `IModelCreationInjection.OnModelCreating`. This method will be called after calling base method.
This is the **main advantage** of this library, because it allow *to setup model differently for **one** DbContext depending on conditions.* As far as I know, [Dynamic Proxy](http://www.castleproject.org/projects/dynamicproxy/) does not fit this need, because of new class creation for each DbContext creation.

Let me explain the details. Normally, you can setup model, e.g. `ComplexType` only once for `DbContext`. But for some complex tasks, like localization of data, you can want to setup some `ComplexTypes` depending on some parameters like `Thread.CurrentUICulture`.

**!TODO!**

### SaveChanges

**!TODO!**

### ValidateEntity

**!TODO!**