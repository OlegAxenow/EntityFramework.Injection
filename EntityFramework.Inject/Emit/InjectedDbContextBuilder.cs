using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Method.Inject;

namespace EntityFramework.Inject.Emit
{
	public class InjectedDbContextBuilder : InjectedTypeBuilder
	{
	    private readonly Type[] _constructorParameterTypes;

	    public InjectedDbContextBuilder(TypeBuilder typeBuilder, Type[] constructorParameterTypes = null) : base(typeBuilder)
	    {
	        _constructorParameterTypes = constructorParameterTypes;
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
	        Debug.Assert(TypeBuilder.BaseType != null, "_typeBuilder.BaseType != null");

	        var parameters = _constructorParameterTypes == null
	            ? new[] {typeof (InjectionSet), typeof (string)}
	            : new[] {typeof (InjectionSet)}.Concat(_constructorParameterTypes).ToArray();

            var constructorBuilder = TypeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, parameters );

            var baseConstructor = TypeBuilder.BaseType.GetConstructor( _constructorParameterTypes ?? new []{ typeof( string ) } );
            if ( baseConstructor == null )
                throw new ArgumentException(
                    string.Format( "DbContext {0} does not have a constructor with the specified parameter types. ({1})",
                        TypeBuilder.BaseType, String.Join(",", parameters.Select( t => t.Name ) ) ) );


            var il = constructorBuilder.GetILGenerator();

            il.Emit( OpCodes.Ldarg_0 );
	        if (_constructorParameterTypes == null)
	        {
                il.Emit( OpCodes.Ldarg_2 );    
            }
            else
	        {
                for ( int i = 2; i < _constructorParameterTypes.Length + 2; ++i )
                {
                    il.Emit( OpCodes.Ldarg, i );
                }
            }
            il.Emit( OpCodes.Call, baseConstructor );

            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Ldarg_1 );
            il.Emit( OpCodes.Stfld, InjectionSetField );

            il.Emit( OpCodes.Ret );
        }
    }
}