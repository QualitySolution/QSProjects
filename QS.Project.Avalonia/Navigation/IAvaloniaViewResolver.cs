using Avalonia.Controls;
using QS.ViewModels;

namespace QS.Navigation;

public interface IAvaloniaViewResolver {
	Control Resolve(ViewModelBase viewModel, string? viewSuffix = null);
}

