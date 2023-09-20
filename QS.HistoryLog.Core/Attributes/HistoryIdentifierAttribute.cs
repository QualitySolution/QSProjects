using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.HistoryLog.Core.Attributes {

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
					AllowMultiple = false, Inherited = true)]
	public class HistoryIdentifierAttribute : Attribute {
		public Type TargetType { get; set; }
	}
}
