using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Reflection;

namespace EntityFramework.Inject.Localization
{
	public class ComplexTypeInitializationInjection : ISaveChangesInjection
	{
		private static readonly Dictionary<Type, Func<object>> AcceptedComplexTypes;

		private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<PropertyInfo, Func<object>>> PropertyCache;

		static ComplexTypeInitializationInjection()
		{
			PropertyCache = new ConcurrentDictionary<Type, ConcurrentDictionary<PropertyInfo, Func<object>>>();
			
			// ToImprove: we can collect all complex types, if needed
			AcceptedComplexTypes = new Dictionary<Type, Func<object>>
			{
				{ typeof(LocalizedStrings), () => new LocalizedStrings() },
				{ typeof(ComputedLocalizedStrings), () => new ComputedLocalizedStrings() }
			};
		}

		public virtual void OnBeforeSaveChanges(DbContext context)
		{
			foreach (var entry in context.GetDbStateEntries())
			{
				var type = entry.Entity.GetType();
				foreach (var complexProperty in GetComplexProperties(type))
				{
					// instantiate complex type if null
					object currentValue = complexProperty.Key.GetValue(entry.Entity, new object[0]);
					object newValue = currentValue == null ? complexProperty.Value() : null;
					OnInitializeComplexType(complexProperty.Key, entry, currentValue, newValue);
				}
			}
		}

		protected virtual void OnInitializeComplexType(PropertyInfo propertyInfo, DbEntityEntry entry, object currentValue, object newValue)
		{
			if (entry.State == EntityState.Added && currentValue == null)
				propertyInfo.SetValue(entry.Entity, newValue, null);
		}

		public virtual void OnAfterSaveChanges(DbContext context)
		{
		}

		private static ConcurrentDictionary<PropertyInfo, Func<object>> GetComplexProperties(Type type)
		{
			var result = PropertyCache.GetOrAdd(type, new ConcurrentDictionary<PropertyInfo, Func<object>>());
			
			foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var set = propertyInfo.GetSetMethod(false);
				if (set == null) continue;

				var propertyType = propertyInfo.PropertyType;
				Func<object> func;
				if (AcceptedComplexTypes.TryGetValue(propertyType, out func))
				{
					result.TryAdd(propertyInfo, func);
				}
			}

			return result;
		}

		public string UniqueKey
		{
			get { return "ComplexTypeInitialization"; }
		}
	}
}