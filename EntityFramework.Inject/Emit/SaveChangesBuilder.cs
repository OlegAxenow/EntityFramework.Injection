using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
			var method = typeBuilder.DefineMethod(MethodName,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(int), new Type[0]);

			Debug.Assert(typeBuilder.BaseType != null, "typeBuilder.BaseType != null");
			var baseMethod = typeBuilder.BaseType.GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);

			var parameterTypes = new[] { typeof(DbContext) };
			var beforeMethod = injectionType.GetMethod("OnBeforeSaveChanges", parameterTypes);
			var afterMethod = injectionType.GetMethod("OnAfterSaveChanges", parameterTypes);

			var il = method.GetILGenerator();
			
			EmitHelper.DeclareLocalsForInjection(injectionType, il);
			
			// declare result variable
			il.DeclareLocal(typeof(int));

			il.EmitGetInjections(injectionSetField, injectionType);

			il.EmitInjectionLoop(beforeMethod, x => x.Emit(OpCodes.Ldarg_0));

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, baseMethod);
			il.Emit(OpCodes.Stloc_3);

			il.EmitInjectionLoop(afterMethod, x => x.Emit(OpCodes.Ldarg_0));

			il.Emit(OpCodes.Ldloc_3);
			
			il.Emit(OpCodes.Ret);
		}
	}
}