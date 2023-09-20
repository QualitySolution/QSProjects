using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.HistoryLog {

	/// <summary>
	/// При установки этого атрибута на свойство изменение этого свойства не будет фиксироваться в истории изменений объекта
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class IgnoreHistoryTraceAttribute : Attribute {
	}
}
