using System.Collections.ObjectModel;

namespace QS.Project.DB.EntityMappingConfig
{
	public interface IEntityMappingConfig
	{
		string TableName { get; }
		ReadOnlyDictionary<string, string> PropertyNames { get; }
	}
}
