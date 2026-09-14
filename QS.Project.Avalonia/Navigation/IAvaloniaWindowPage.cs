using Avalonia.Controls;

namespace QS.Navigation;

public interface IAvaloniaWindowPage : IPage {
	Control View { get; set; }
	// null — окно уже закрывается программно
	Window? Window { get; set; }
}
