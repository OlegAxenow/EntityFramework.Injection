using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using EntityFramework.Inject.Localization;
using Method.Inject;

namespace EntityFramework.Inject.Emit
{
	public class LocalizationModelCreationBuilder : ModelCreationBuilder
	{
		protected override void ConfigureDbSet(Type entityType, ILGenerator il, FieldBuilder injectionSetField, Type injectionType, 
			MethodInfo entityMethod, Type entityConfigurationType)
		{
			var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			                       .Where(x => typeof(LocalizedStrings).IsAssignableFrom(x.PropertyType) ||
				                       typeof(ComputedLocalizedStrings).IsAssignableFrom(x.PropertyType))
			                       .ToArray();

			var expressionType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(entityType, typeof(string)));
			var propertyMethod = entityConfigurationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						.First(x => x.Name == "Property" && x.GetParameters().First().ParameterType == expressionType);

			if (properties.Length != 0)
			{
				foreach (var property in properties)
				{		
					il.Emit(OpCodes.Ldarg_1);
				}
			}
		}
	}
}