using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.HistoryLog {

	/// <summary>
	/// При установке на класс говорит о том что изменения в этом объекте необходимо записывать в историю изменений.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class HistoryTraceAttribute : Attribute {
	}
}
