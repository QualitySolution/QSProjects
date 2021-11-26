using System;
using System.Collections.Generic;
using QS.Services;
namespace QS.Project.Journal
{
	public class JournalEntityConfig
	{
		public Type EntityType { get; }
		public IPermissionResult PermissionResult { get; set; }
		public IEnumerable<JournalEntityDocumentsConfig> EntityDocumentConfigurations { get; set; }

		public JournalEntityConfig(Type entityType, IEnumerable<JournalEntityDocumentsConfig> documentsConfigs)
		{
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			EntityDocumentConfigurations = documentsConfigs ?? throw new ArgumentNullException(nameof(documentsConfigs));
		}
	}
}
