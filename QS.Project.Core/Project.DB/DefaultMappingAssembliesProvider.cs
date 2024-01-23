using System;
using System.Collections.Generic;
using System.Reflection;

namespace QS.Project.DB {
	public class DefaultMappingAssembliesProvider : IMappingAssembliesProvider {
		private readonly IEnumerable<Assembly> _mappingAssemblies;

		public DefaultMappingAssembliesProvider(IEnumerable<Assembly> mappingAssemblies) {
			_mappingAssemblies = mappingAssemblies ?? throw new ArgumentNullException(nameof(mappingAssemblies));
		}

		public IEnumerable<Assembly> GetMappingAssemblies() {
			return _mappingAssemblies;
		}
	}
}
