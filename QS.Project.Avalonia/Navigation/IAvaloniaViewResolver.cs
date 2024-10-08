using Avalonia.Controls;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Navigation;

public interface IAvaloniaViewResolver {
	Control Resolve(ViewModelBase viewModel);
}
