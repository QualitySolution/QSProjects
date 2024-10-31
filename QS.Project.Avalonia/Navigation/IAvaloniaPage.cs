using Avalonia.Controls;

namespace QS.Navigation;

public interface IAvaloniaPage : IPage {
	Control AvaloniaView { get; }
}
