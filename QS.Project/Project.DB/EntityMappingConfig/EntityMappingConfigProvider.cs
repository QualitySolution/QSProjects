using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg;

namespace QS.Project.DB.EntityMappingConfig
{
	public class EntityMappingConfigProvider : IEntityMappingConfigProvider
	{
		private readonly Configuration _nhibernateConfiguration;

		public EntityMappingConfigProvider(Configuration nhibernateConfiguration)
		{
			_nhibernateConfiguration =
				nhibernateConfiguration ?? throw new ArgumentNullException(nameof(nhibernateConfiguration));
		}

		public IEntityMappingConfig GetEntityMappingConfig<T>() where T : class
		{
			var persistentClass = _nhibernateConfiguration.GetClassMapping(typeof(T));

			if(persistentClass == null)
			{
				throw new InvalidOperationException($"Не настроен маппинг для класса {typeof(T).FullName}");
			}

			var propertyNames = new Dictionary<string, string>();

			var identifierPropertyName = persistentClass.IdentifierProperty?.Name;
			var identifierColumnName = persistentClass.IdentifierProperty?.ColumnIterator?.FirstOrDefault()?.Text;

			if(!string.IsNullOrWhiteSpace(identifierPropertyName)
				&& !string.IsNullOrWhiteSpace(identifierColumnName)
				&& !propertyNames.ContainsKey(identifierPropertyName))
			{
				propertyNames.Add(identifierPropertyName, identifierColumnName);
			}

			var currentPersistentClass = persistentClass;
			while(currentPersistentClass != null)
			{
				foreach(var property in currentPersistentClass.PropertyIterator)
				{
					var columnName = property.ColumnIterator.FirstOrDefault()?.Text;
					if(!propertyNames.ContainsKey(property.Name) && !string.IsNullOrWhiteSpace(columnName))
					{
						propertyNames.Add(property.Name, columnName);
					}
				}
				currentPersistentClass = currentPersistentClass.Superclass;
			}

			return new EntityMappingConfig(persistentClass.Table.Name, propertyNames);
		}
	}
}
