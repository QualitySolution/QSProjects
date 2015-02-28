using System;
using System.Collections.Generic;
using System.Data.Bindings;

namespace QSHistoryLog
{
	public class HistoryChangeSet
	{
		public virtual int Id { get; set; }

		public virtual int UserId { get; set; }
		public virtual string UserName { get; set; }
		public virtual DateTime ChangeTime { get; set; }
		public virtual ChangeSetType Operation { get; set; }
		public virtual string ObjectName { get; set; }
		public virtual int ObjectId { get; set; }

		string objectTitle;
		public virtual string ObjectTitle {
			get {
				if(String.IsNullOrEmpty (objectTitle))
				{
					var Descript = HistoryMain.ObjectsDesc.Find (d => d.ObjectName == ObjectName);
					return Descript != null ? Descript.DisplayName : ObjectName;
				}
				else
					return objectTitle;
			}
			set {
				objectTitle = value;
			}
		}

		public virtual IList<FieldChange> Changes { get; set; }

		public string ChangeTimeText
		{
			get { return ChangeTime.ToShortTimeString ();}
		}

		public string OperationText
		{
			get { return Operation.GetEnumTitle ();}
		}


		public HistoryChangeSet ()
		{
		}
	}
}

