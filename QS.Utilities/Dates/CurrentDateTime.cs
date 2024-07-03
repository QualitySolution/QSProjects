using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Utilities.Dates {
	/// <summary>
	/// Базовая реализация интерфейса ICurrentDateTime
	/// </summary>
	public class CurrentDateTime:ICurrentDateTime {
		public DateTime Now =>DateTime.Now;
		public DateTime Today => DateTime.Today;
	}
}
