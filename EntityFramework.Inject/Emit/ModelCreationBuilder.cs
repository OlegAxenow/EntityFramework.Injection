using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Method.Inject;

namespace EntityFramework.Inject.Emit
{
	public class ModelCreationBuilder : IMethodBuilder
	{
		private const string MethodName = "OnModelCreating";

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
		///		}
		/// }
		/// </example>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
			Justification = "Reviewed. Suppression is OK here.")]
		public void Build(TypeBuilder typeBuilder, FieldBuilder injectionSetField, Type injectionType)
		{
			var parameterTypes = new[] { typeof(DbModelBuilder), typeof(DbContext) };

			var methods = new Methods(typeBuilder, MethodName, new[] { typeof(DbModelBuilder) });
			var injectionMethod = ReflectionHelper.GetMethod(injectionType, MethodName, BindingFlags.Instance | BindingFlags.Public, parameterTypes);

			var il = methods.GetILGenerator(injectionType);

			DeclareLocals(injectionType, il);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, methods.BaseMethod);

			il.EmitGetInjections(injectionSetField, injectionType);

			MethodInfo entityMethod = typeof(DbModelBuilder).GetMethod("Entity", BindingFlags.Public | BindingFlags.Instance);

			il.EmitInjectionLoop(x =>
			{
				x.Emit(OpCodes.Ldarg_1);
				x.Emit(OpCodes.Ldarg_0);

				x.Emit(OpCodes.Callvirt, injectionMethod);

				ConfigureDbSets(typeBuilder, x, injectionSetField, injectionType, entityMethod);
			});

			il.Emit(OpCodes.Ret);
		}

		protected virtual void DeclareLocals(Type injectionType, ILGenerator il)
		{
			EmitHelper.DeclareLocalsForInjection(injectionType, il);
		}

		protected virtual void ConfigureDbSets(TypeBuilder typeBuilder, ILGenerator il, FieldBuilder injectionSetField, 
			Type injectionType, 
			MethodInfo entityMethod)
		{
			var baseType = typeBuilder.BaseType;
			Debug.Assert(baseType != null, "baseType != null");

			foreach (var property in baseType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			                                 .Where(x => x.PropertyType.IsGenericType &&
				                                 x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
			{
				Type[] genericArguments = property.PropertyType.GetGenericArguments();
				if (genericArguments.Length != 1)
					throw new InvalidOperationException(
						string.Format("Property {0}.{1} should have one generic type argument.", baseType.FullName, property.Name));

				var entityType = genericArguments[0];
				Type entityConfigurationType = typeof(EntityTypeConfiguration<>).MakeGenericType(entityType);

				ConfigureDbSet(entityType, il, injectionSetField, injectionType, entityMethod, entityConfigurationType);
			}
		}

		protected virtual void ConfigureDbSet(Type entityType, ILGenerator il, FieldBuilder injectionSetField, 
			Type injectionType, MethodInfo entityMethod, 
			Type entityConfigurationType)
		{
		}
	}
}