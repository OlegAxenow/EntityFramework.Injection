using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using EntityFramework.Inject.Localization;

namespace EntityFramework.Inject.Emit
{
	public class LocalizationModelCreationBuilder : ModelCreationBuilder
	{
		public static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle",
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(RuntimeTypeHandle) }, null);

		public static readonly MethodInfo GetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle",
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(RuntimeMethodHandle) }, null);

		public static readonly MethodInfo Parameter = typeof(Expression).GetMethod("Parameter",
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Type), typeof(string) }, null);

		public static readonly MethodInfo Property = typeof(Expression).GetMethod("Property",
			BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Expression), typeof(MethodInfo) }, null);

		public static readonly MethodInfo GenericLambda = typeof(Expression).GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Single(x => x.Name == "Lambda" && x.IsGenericMethod && x.GetParameters().Skip(1).First().ParameterType == typeof(ParameterExpression[]));

		public static readonly MethodInfo GenericConfigureProperty = typeof(IDataLocalizationInjection).GetMethod("ConfigureProperty");

		public static readonly PropertyInfo DefaultValueProperty =
			typeof(LocalizedStrings).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

		public static readonly PropertyInfo DefaultComputedValueProperty =
			typeof(ComputedLocalizedStrings).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

		public static readonly PropertyInfo[] IndexedValueProperties =
			typeof(LocalizedStrings).GetProperties(BindingFlags.Instance | BindingFlags.Public);

		public static readonly PropertyInfo[] IndexedComputedValueProperties =
			typeof(ComputedLocalizedStrings).GetProperties(BindingFlags.Instance | BindingFlags.Public);

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
			                           .Where(x => typeof(LocalizedStrings).IsAssignableFrom(x.PropertyType) ||
				                           typeof(ComputedLocalizedStrings).IsAssignableFrom(x.PropertyType))
			                           .ToArray();

			var funcType = typeof(Func<,>).MakeGenericType(entityType, typeof(string));
			
			var lambda = GenericLambda.MakeGenericMethod(funcType);

			var configure = GenericConfigureProperty.MakeGenericMethod(entityType);

			if (properties.Length == 0) return;

			foreach (var complexProperty in properties)
			{
				var isComputed = typeof(ComputedLocalizedStrings).IsAssignableFrom(complexProperty.PropertyType);
				ConfigureProperties(entityType, il, complexProperty, lambda, configure, 
					isComputed ? IndexedComputedValueProperties : IndexedValueProperties, 
					isComputed ? DefaultComputedValueProperty : DefaultValueProperty);
			}
		}

		private static void ConfigureProperties(Type entityType, ILGenerator il, PropertyInfo complexProperty, MethodInfo lambda,
			MethodInfo configure, IEnumerable<PropertyInfo> indexedProperties, PropertyInfo defaultProperty)
		{
			// TODO: LocaleIndex checking

			ConfigureProperty(il, entityType, complexProperty, defaultProperty, lambda, configure);

			foreach (var valueProperty in indexedProperties)
			{
				ConfigureProperty(il, entityType, complexProperty, valueProperty, lambda, configure);
			}
		}

		private static void ConfigureProperty(ILGenerator il, Type entityType, PropertyInfo complexProperty, 
			PropertyInfo valueProperty, MethodInfo lambda, MethodInfo configure)
		{
			// injections[i].ConfigureProperty<Category>(modelBuilder, x => x.CategoryName.Value1);
			
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldloc_1);
			il.Emit(OpCodes.Ldelem_Ref);
			il.Emit(OpCodes.Ldarg_1);

			il.Emit(OpCodes.Ldtoken, entityType);
			il.EmitCall(OpCodes.Call, GetTypeFromHandle, null);

			il.Emit(OpCodes.Ldstr, "x");
			il.EmitCall(OpCodes.Call, Parameter, null);

			il.Emit(OpCodes.Stloc_S, 4);
			il.Emit(OpCodes.Ldloc_S, 4);

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