using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DbManagement.Entities {
	internal class BaseAccessRow {
		public int BaseId { get; set; }
		public string BaseName { get; set; }
		public string BaseTitle { get; set; }
		public bool HasAccess { get; set; }
		public bool Admin { get; set; }
		public bool ReadOnly { get; set; }
	}
}
