using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Method.Inject;

namespace EntityFramework.Inject.Emit
{
	public class InjectedDbContextBuilder : InjectedTypeBuilder
	{
		public InjectedDbContextBuilder(TypeBuilder typeBuilder) : base(typeBuilder)
		{
		}

		/// <summary>
		/// Builds constructor if necessary (see example). Default implementation builds constructor for base constructor without parameters.
		/// </summary>
		/// <example>
		/// public BasicDbContext_test_model_creation(InjectionSet ps, string name) : base(name) {
		/// 	_injectionSet = ps;
		/// }
		/// </example>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		public override void BuildConstructor()
		{
			var constructorBuilder =
				TypeBuilder.DefineConstructor(
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
					CallingConventions.Standard, new[] { typeof(InjectionSet), typeof(string) });

			Debug.Assert(TypeBuilder.BaseType != null, "_typeBuilder.BaseType != null");

			var baseConstructor = TypeBuilder.BaseType.GetConstructor(new[] { typeof(string) });
			if (baseConstructor == null)
				throw new ArgumentException(
					string.Format("DbContext {0} does not have a constructor with connectionStringOrName parameter.",
						TypeBuilder.BaseType));

			var il = constructorBuilder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Call, baseConstructor);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, InjectionSetField);

			il.Emit(OpCodes.Ret);
		}
	}
}