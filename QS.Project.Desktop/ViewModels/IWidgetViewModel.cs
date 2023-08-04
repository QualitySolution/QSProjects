using System.ComponentModel;

namespace QS.ViewModels {
	public interface IWidgetViewModel {
		void FirePropertyChanged();
		event PropertyChangedEventHandler PropertyChanged;
	}
}
