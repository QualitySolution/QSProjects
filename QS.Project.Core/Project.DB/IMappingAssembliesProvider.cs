using System.Collections.Generic;
using System.Reflection;

namespace QS.Project.DB {
	public interface IMappingAssembliesProvider {
		IEnumerable<Assembly> GetMappingAssemblies();
	}
}
