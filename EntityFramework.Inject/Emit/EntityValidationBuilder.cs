using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Method.Inject;

namespace EntityFramework.Inject.Emit
{
	public class EntityValidationBuilder : IMethodBuilder
	{
		private const string MethodName = "ValidateEntity";

		/// <summary>
		/// Builds method ValidateEntity (see example).
		/// </summary>
		/// <example>
		/// protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary&lt;object, object&gt; items) {
		///		var result = base.ValidateEntity(entityEntry, items);
		///
		///		var injections = _injectionSet.GetInjections&lt;IEntityValidationInjection&gt;();
		///	
		///		for (int i = 0; i &lt; injections.Length; i++)
		///			injections[i].OnValidateEntity(result, entityEntry, items);
		/// 
		///		return result;
		/// }
		/// </example>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		public void Build(TypeBuilder typeBuilder, FieldBuilder injectionSetField, Type injectionType)
		{
			var parameterTypes = new[] { typeof(DbEntityEntry), typeof(IDictionary<object, object>) };
			var returnType = typeof(DbEntityValidationResult);

			var method = typeBuilder.DefineMethod(MethodName, 
				MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, returnType, parameterTypes);

			Debug.Assert(typeBuilder.BaseType != null, "typeBuilder.BaseType != null");
			var baseMethod = typeBuilder.BaseType.GetMethod(MethodName, BindingFlags.Instance | BindingFlags.NonPublic, null, 
				parameterTypes, null);
			var injectionMethod = injectionType.GetMethod("On" + MethodName, new[] { returnType, parameterTypes[0], parameterTypes[1] });

			var il = method.GetILGenerator();

			EmitHelper.DeclareLocalsForInjection(injectionType, il);

			// declare result variable
			il.DeclareLocal(typeof(DbEntityValidationResult));

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Call, baseMethod);
			il.Emit(OpCodes.Stloc_3);

			il.EmitGetInjections(injectionSetField, injectionType);

			il.EmitInjectionLoop(injectionMethod, x => 
			{
				x.Emit(OpCodes.Ldloc_3);
				x.Emit(OpCodes.Ldarg_1);
				x.Emit(OpCodes.Ldarg_2);
			});

			il.Emit(OpCodes.Ldloc_3);

			il.Emit(OpCodes.Ret);
		}
	}
}