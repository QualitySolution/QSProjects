using System.ComponentModel;

namespace QS.Navigation {
	public interface IDialogViewModel : INotifyPropertyChanged {
		string Title { get; set; }
	}
}
