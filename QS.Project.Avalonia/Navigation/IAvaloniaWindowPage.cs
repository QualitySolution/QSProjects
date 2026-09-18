using System;
using Avalonia.Controls;

namespace QS.Navigation;

public interface IAvaloniaWindowPage : IPage {
	Control View { get; set; }
	// null — окно уже закрывается программно
	Window? Window { get; set; }
	Action<Window>? ConfigureWindow { get; set; }
}
