using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using Method.Inject;

namespace EntityFramework.Inject.Emit
{
	public class SaveChangesBuilder : IMethodBuilder
	{
		private const string MethodName = "SaveChanges";

		/// <summary>
		/// Builds method ValidateEntity (see example).
		/// </summary>
		/// <example>
		/// public override int SaveChanges() {
		///		var injections = _injectionSet.GetInjections&lt;ISaveChangesInjection&gt;();
		///	
		///		for (int i = 0; i &lt; injections.Length; i++)
		///			injections[i].OnBeforeSaveChanges(this);
		/// 
		///		int result = base.SaveChanges();
		/// 
		///		for (int i = 0; i &lt; injections.Length; i++)
		///			injections[i].OnAfterSaveChanges(this);
		/// 
		///		return result;
		/// }
		/// </example>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		public void Build(TypeBuilder typeBuilder, FieldBuilder injectionSetField, Type injectionType)
		{
			var methods = new Methods(typeBuilder, MethodName, new Type[0], typeof(int));

			var parameterTypes = new[] { typeof(DbContext) };
			var beforeMethod = injectionType.GetMethod("OnBeforeSaveChanges", parameterTypes);
			var afterMethod = injectionType.GetMethod("OnAfterSaveChanges", parameterTypes);

			var il = methods.GetILGenerator(injectionType);
			
			EmitHelper.DeclareLocalsForInjection(injectionType, il);
			
			// declare result variable
			il.DeclareLocal(typeof(int));

			il.EmitGetInjections(injectionSetField, injectionType);

			il.EmitInjectionLoop(x =>
			{
				x.Emit(OpCodes.Ldarg_0);

				x.Emit(OpCodes.Callvirt, beforeMethod);
			});

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, methods.BaseMethod);
			il.Emit(OpCodes.Stloc_3);

			il.EmitInjectionLoop(x =>
			{
				x.Emit(OpCodes.Ldarg_0);

				x.Emit(OpCodes.Callvirt, afterMethod);
			});

			il.Emit(OpCodes.Ldloc_3);
			
			il.Emit(OpCodes.Ret);
		}
	}
}