using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
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
		///			injections[i].OnModelCreating(modelBuilder);
		/// }
		/// </example>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		public void Build(TypeBuilder typeBuilder, FieldBuilder injectionSetField, Type injectionType)
		{
			var parameterTypes = new[] { typeof(DbModelBuilder) };

			var methods = new Methods(typeBuilder, MethodName, parameterTypes);
			var injectionMethod = injectionType.GetMethod(MethodName, parameterTypes);

			var il = methods.GetILGenerator(injectionType);

			EmitHelper.DeclareLocalsForInjection(injectionType, il);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, methods.BaseMethod);

			il.EmitGetInjections(injectionSetField, injectionType);

			il.EmitInjectionLoop(injectionMethod, x => x.Emit(OpCodes.Ldarg_1));

			il.Emit(OpCodes.Ret);
		}
	}
}