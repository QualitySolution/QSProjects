using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Utilities.Dates {
	/// <summary>
	/// Интерфейс необходимый для замены стандартных DateTime.Now И DateTime.Today в коде, который покрывается тестами, во избежание багов. Также используется в самих тестах для создания заглушек.
	/// </summary>
	public interface ICurrentDateTime {
		DateTime Now { get; }
		DateTime Today { get; }
	}
}
