using System;
using System.Collections.Generic;
using QS.Services;
namespace QS.Project.Journal
{
	public class JournalEntityConfig<TNode>
		where TNode : JournalEntityNodeBase
	{
		public Type EntityType { get; }
		public IPermissionResult PermissionResult { get; set; }
		public IEnumerable<JournalEntityDocumentsConfig<TNode>> EntityDocumentConfigurations { get; set; }

		public JournalEntityConfig(Type entityType, IEnumerable<JournalEntityDocumentsConfig<TNode>> documentsConfigs)
		{
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			EntityDocumentConfigurations = documentsConfigs ?? throw new ArgumentNullException(nameof(documentsConfigs));
		}
	}
}
