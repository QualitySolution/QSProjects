using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Navigation;

public interface IAvaloniaWindowPage : IPage {
	Control View { get; set; }
	// null — окно уже закрывается программно
	Window? Window { get; set; }
}
