using System;
using SIT.Components.ObjectComparer;

namespace QSHistoryLog
{
	public class FieldChange
	{
		public virtual int Id { get; set; }

		public virtual string Path { get; set; }
		public virtual ChangeType Type { get; set; }
		public virtual string OldValue { get; set; }
		public virtual string NewValue { get; set; }

		public FieldChange ()
		{
		}
	}
}

