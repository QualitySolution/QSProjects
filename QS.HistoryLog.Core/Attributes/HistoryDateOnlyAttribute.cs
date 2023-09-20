using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.HistoryLog.Core.Attributes {

	/// <summary>
	/// Для поле типа DateTime в журнал изменений пишем только дату, без времени.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
					AllowMultiple = false, Inherited = true)]
	//TODO Пока не реализовано в трекере.
	public class HistoryDateOnlyAttribute : Attribute {
	}
}
