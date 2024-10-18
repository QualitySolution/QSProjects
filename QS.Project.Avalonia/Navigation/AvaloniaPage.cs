using Avalonia.Controls;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace QS.Navigation;

public class AvaloniaPage : IAvaloniaPage {
	public Control AvaloniaView { get; set; }

	public string PageHash => throw new NotImplementedException();

	public string Title { get; set; }

	public DialogViewModelBase ViewModel => throw new NotImplementedException();

	public object Tag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public IEnumerable<MasterToSlavePair> SlavePagesAll => throw new NotImplementedException();

	public IEnumerable<ParentToChildPair> ChildPagesAll => throw new NotImplementedException();

	public IEnumerable<IPage> SlavePages => throw new NotImplementedException();

	public IEnumerable<IPage> ChildPages => throw new NotImplementedException();

	public event EventHandler<PageClosedEventArgs> PageClosed;
}
