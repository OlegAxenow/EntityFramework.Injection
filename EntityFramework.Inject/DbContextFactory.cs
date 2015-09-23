using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using EntityFramework.Inject.Emit;
using Method.Inject;

namespace EntityFramework.Inject
{
	/// <summary>
	/// Provides methods to get injected contexts.
	/// </summary>
	public class DbContextFactory
	{
		private readonly object _syncObject = new object();
		private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
		private readonly string _defaultNameOrConnectionString;
		private readonly bool _saveAssembliesToDisk;

		public DbContextFactory(string defaultNameOrConnectionString = null, bool saveAssembliesToDisk = false)
		{
			_defaultNameOrConnectionString = defaultNameOrConnectionString;
			_saveAssembliesToDisk = saveAssembliesToDisk;
		}

		/// <summary>
		/// Creates new injected instance of context with specified injections.</summary>
		public T Create<T>(InjectionSet injectionSet, string nameOrConnectionString = null) 
		{
			var type = typeof(T);
			var contextType = CreateTypes(injectionSet, null, type)[0];

			Debug.Assert(type.BaseType != null, "type.BaseType != null");
			nameOrConnectionString = nameOrConnectionString ?? _defaultNameOrConnectionString ?? type.BaseType.Name;

			var constructor = contextType.GetConstructor(new[] { typeof(InjectionSet), typeof(string) });
			if (constructor == null)
				throw new InvalidOperationException(
					string.Format("DbContext constructor with injection set and connection string does not supported by {0}.",
					contextType));
			var parameters = new object[] { injectionSet, nameOrConnectionString };

			return (T)constructor.Invoke(parameters);
		}

        /// <summary>
        /// Creates new injected instance of context with specified injections.</summary>
        public T Create<T>( InjectionSet injectionSet, object[] constructorParameters )
        {
            var type = typeof( T );
            var constructorParameterTypes = constructorParameters.Select( o => o.GetType() ).ToArray();
            var contextType = CreateTypes( injectionSet, constructorParameterTypes, type )[ 0 ];

            Debug.Assert( type.BaseType != null, "type.BaseType != null" );            

            var constructor = contextType.GetConstructor( new []{typeof(InjectionSet)}.Concat( constructorParameterTypes ).ToArray() );
            if ( constructor == null )
                throw new InvalidOperationException(
                    string.Format( "DbContext constructor with injection set and the specified constructor parameters not supported by {0}.",
                    contextType ) );

            var parameters = new[] { injectionSet }.Concat(constructorParameters).ToArray();

            return (T) constructor.Invoke( parameters.ToArray() );
        }

        /// <summary>
        /// Creates <see cref="Type"/> for injected instance of context with specified injections.</summary>
        /// <remarks> Can be used to create multiple types to avoid overhead of creation too many dynamic assemblies.</remarks>
        public Type[] CreateTypes(InjectionSet injectionSet, Type[] constructorParameterTypes = null, params Type[] baseContextTypes)
		{
			if (injectionSet == null) throw new ArgumentNullException("injectionSet");
			if (baseContextTypes == null) throw new ArgumentNullException("baseContextTypes");
			if (baseContextTypes.Length == 0) return new Type[0];

			var result = new Type[baseContextTypes.Length];
			InjectedAssemblyBuilder builder = null;
			Func<TypeBuilder, InjectedTypeBuilder> factory = tb => new InjectedDbContextBuilder(tb, constructorParameterTypes);
			
			lock (_syncObject)
			{
				for (int index = 0; index < baseContextTypes.Length; index++)
				{
					var contextType = baseContextTypes[index];
					var key = contextType.FullName + injectionSet.UniqueKey;

					Type type;
					if (!_types.TryGetValue(key, out type))
					{
						if (builder == null)
							builder = new InjectedAssemblyBuilder(injectionSet, factory, _saveAssembliesToDisk);
						
						var newContextType = builder.Append(contextType);

						result[index] = newContextType;
						_types.Add(key, newContextType);
					}
					else
					{
						result[index] = type;
					}
				}
			}

			if (builder != null && _saveAssembliesToDisk)
				builder.Save();

			return result;
		}
	}
}