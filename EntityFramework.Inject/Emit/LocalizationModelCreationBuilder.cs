using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace EntityFramework.Inject.Emit
{
	public class LocalizationModelCreationBuilder<TLocalizedStrings, TComputedLocalizedStrings> : ModelCreationBuilder
		where TLocalizedStrings : class
		where TComputedLocalizedStrings : class
	{
		// ReSharper disable StaticFieldInGenericType
		protected static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", 
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(RuntimeTypeHandle) }, null);

		protected static readonly MethodInfo GetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle", 
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(RuntimeMethodHandle) }, null);

		protected static readonly MethodInfo Parameter = typeof(Expression).GetMethod("Parameter", 
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Type), typeof(string) }, null);

		protected static readonly MethodInfo Property = typeof(Expression).GetMethod("Property", 
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Expression), typeof(MethodInfo) }, null);

		protected static readonly MethodInfo GenericLambda = typeof(Expression)
			.GetMethods(BindingFlags.Static | BindingFlags.Public)
			.Single(x => x.Name == "Lambda" && x.IsGenericMethod && 
				x.GetParameters().Skip(1).First().ParameterType == typeof(ParameterExpression[]));

		protected static readonly MethodInfo GenericConfigureProperty =
			typeof(IDataLocalizationInjection).GetMethod("ConfigureProperty");

		protected static readonly MethodInfo GenericIgnore =
			typeof(IDataLocalizationInjection).GetMethod("IgnoreProperty");

		protected static readonly PropertyInfo DefaultValueProperty =
			typeof(TLocalizedStrings).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

		protected static readonly PropertyInfo DefaultComputedValueProperty =
			typeof(TComputedLocalizedStrings).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

		protected static readonly PropertyInfo[] IndexedValueProperties =
			typeof(TLocalizedStrings).GetProperties(BindingFlags.Instance | BindingFlags.Public);

		protected static readonly PropertyInfo[] IndexedComputedValueProperties =
			typeof(TComputedLocalizedStrings).GetProperties(BindingFlags.Instance | BindingFlags.Public);

		// ReSharper restore StaticFieldInGenericType

		/// <summary>
		/// Builds method OnModelCreating (see example).
		/// </summary>
		/// <example>
		/// protected override void OnModelCreating(DbModelBuilder modelBuilder) {
		///		base.OnModelCreating(modelBuilder);
		///
		///		var injections = _injectionSet.GetInjections&lt;IModelCreationInjection&gt;();
		///	
		///		for (int i = 0; i &lt; injections.Length; i++)
		///		{
		///			injections[i].OnModelCreating(modelBuilder);
		///			injections[i].ConfigureProperty&lt;Category&gt;(modelBuilder, x => x.CategoryName.Value);
		///			...
		///		}
		/// }
		/// </example>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
			Justification = "Reviewed. Suppression is OK here.")]
		protected override void ConfigureDbSet(Type entityType, ILGenerator il)
		{
			var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			                           .Where(x => typeof(TLocalizedStrings).IsAssignableFrom(x.PropertyType) ||
				                           typeof(TComputedLocalizedStrings).IsAssignableFrom(x.PropertyType))
			                           .ToArray();

			var lambda = GenericLambda.MakeGenericMethod(typeof(Func<,>).MakeGenericType(entityType, typeof(string)));

			var configure = GenericConfigureProperty.MakeGenericMethod(entityType);

			if (properties.Length == 0) return;

			foreach (var complexProperty in properties)
			{
				var isComputed = typeof(TComputedLocalizedStrings).IsAssignableFrom(complexProperty.PropertyType);
				ConfigureProperties(entityType, il, complexProperty, lambda, configure, 
					isComputed ? IndexedComputedValueProperties : IndexedValueProperties, 
					isComputed ? DefaultComputedValueProperty : DefaultValueProperty);
			}
		}

		protected override void ConfigureDbSets(TypeBuilder typeBuilder, ILGenerator il)
		{
			base.ConfigureDbSets(typeBuilder, il);

			IgnoreProperties(il, typeof(TLocalizedStrings), DefaultValueProperty, IndexedValueProperties);
			IgnoreProperties(il, typeof(TComputedLocalizedStrings), DefaultComputedValueProperty, IndexedComputedValueProperties);
		}

		private static void IgnoreProperties(ILGenerator il, Type complexType, PropertyInfo valueProperty, PropertyInfo[] indexedProperties)
		{
			var ignore = GenericIgnore.MakeGenericMethod(complexType);
			var lambda = GenericLambda.MakeGenericMethod(typeof(Func<,>).MakeGenericType(complexType, typeof(string)));

			ConfigureProperty(il, complexType, lambda, ignore, valueProperty);

			foreach (var property in indexedProperties)
			{
				ConfigureProperty(il, complexType, lambda, ignore, property);
			}
		}

		private static void ConfigureProperties(Type entityType, ILGenerator il, PropertyInfo complexProperty, 
			MethodInfo lambda,  MethodInfo configure, IEnumerable<PropertyInfo> indexedProperties, PropertyInfo defaultProperty)
		{
			// TODO: consider to generate LocaleIndex checking to reduce lines to execute

			ConfigureProperty(il, entityType, lambda, configure, defaultProperty, complexProperty);

			foreach (var valueProperty in indexedProperties)
			{
				ConfigureProperty(il, entityType, lambda, configure, valueProperty, complexProperty);
			}
		}

		private static void ConfigureProperty(ILGenerator il, Type type, MethodInfo lambda, MethodInfo configure, 
			PropertyInfo valueProperty, PropertyInfo complexProperty = null)
		{
			// injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value1);
			// injections[i].IgnoreProperty<LocalizedStrings>(modelBuilder, x => x.Value1);

			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldloc_1);
			il.Emit(OpCodes.Ldelem_Ref);
			il.Emit(OpCodes.Ldarg_1);

			il.Emit(OpCodes.Ldtoken, type);
			il.EmitCall(OpCodes.Call, GetTypeFromHandle, null);

			il.Emit(OpCodes.Ldstr, "x");
			il.EmitCall(OpCodes.Call, Parameter, null);

			il.Emit(OpCodes.Stloc_S, 4);
			il.Emit(OpCodes.Ldloc_S, 4);

			if (complexProperty != null)
				EmitPropertyForLambda(il, complexProperty);
			EmitPropertyForLambda(il, valueProperty);

			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Newarr, typeof(ParameterExpression));
			il.Emit(OpCodes.Stloc_3);
			il.Emit(OpCodes.Ldloc_3);
			il.Emit(OpCodes.Ldc_I4_0);

			il.Emit(OpCodes.Ldloc_S, 4);
			il.Emit(OpCodes.Stelem_Ref);
			il.Emit(OpCodes.Ldloc_3);
			il.EmitCall(OpCodes.Call, lambda, null);
			il.EmitCall(OpCodes.Callvirt, configure, null);
		}

		private static void EmitPropertyForLambda(ILGenerator il, PropertyInfo complexProperty)
		{
			il.Emit(OpCodes.Ldtoken, complexProperty.GetGetMethod());
			il.EmitCall(OpCodes.Call, GetMethodFromHandle, null);
			il.Emit(OpCodes.Castclass, typeof(MethodInfo));
			il.EmitCall(OpCodes.Call, Property, null);
		}

		protected override void DeclareLocals(Type injectionType, ILGenerator il)
		{
			base.DeclareLocals(injectionType, il);

			il.DeclareLocal(typeof(ParameterExpression[])); // 3
			il.DeclareLocal(typeof(ParameterExpression)); // 4
		}
	}
}