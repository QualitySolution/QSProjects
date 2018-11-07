using System;
using QS.Project.Dialogs;

namespace QS.DomainModel.Entity
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultReferenceButtonModeAttribute : Attribute
	{
		public ReferenceButtonMode ReferenceButtonMode;

		public DefaultReferenceButtonModeAttribute(ReferenceButtonMode mode)
		{
			ReferenceButtonMode = mode;
		}
	}
}
