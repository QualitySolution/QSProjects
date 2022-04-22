using System;
using QS.Project.Dialogs;

namespace QS.DomainModel.Entity
{
	[AttributeUsage(AttributeTargets.Class)]
	[Obsolete("Думаю нужно избавляться от этого атрибута, после реализации полноценных прав пользователей на сущности. По моему используется только в банках.")]
	public class DefaultReferenceButtonModeAttribute : Attribute
	{
		public ReferenceButtonMode ReferenceButtonMode;

		public DefaultReferenceButtonModeAttribute(ReferenceButtonMode mode)
		{
			ReferenceButtonMode = mode;
		}
	}
}
