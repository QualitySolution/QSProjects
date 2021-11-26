using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace QS.Project.DB.EntityMappingConfig
{
	public class EntityMappingConfig : IEntityMappingConfig
	{
		public EntityMappingConfig(string tableName, IDictionary<string, string> propertyNames)
		{
			TableName = tableName;
			PropertyNames = new ReadOnlyDictionary<string, string>(propertyNames);
		}

		public string TableName { get; }
		public ReadOnlyDictionary<string, string> PropertyNames { get; }
	}
}
