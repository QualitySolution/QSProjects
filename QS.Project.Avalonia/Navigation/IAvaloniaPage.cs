using Avalonia.Controls;

namespace QS.Navigation;

public interface IAvaloniaPage : IPage {
	Control View { get; set; }
}
