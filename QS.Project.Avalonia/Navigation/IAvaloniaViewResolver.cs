using Avalonia.Controls;
using Avalonia.Controls.Templates;
using QS.ViewModels;

namespace QS.Navigation;

public interface IAvaloniaViewResolver : IDataTemplate {
	Control Resolve(object viewModel, string? viewSuffix = null);
}

