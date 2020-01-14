using System;
using Gtk;
using QS.Dialog.Gtk;
using QS.ViewModels.Dialog;

namespace QS.Tdi
{
	public class ViewModelTdiTab : TdiTabBase
	{
		public DialogViewModelBase ViewModel;

		public ViewModelTdiTab(DialogViewModelBase viewModel, Widget widget)
		{
			if(widget == null) throw new ArgumentNullException(nameof(widget));

			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;

			//Нужно чтобы класс Gtk.bin работал как надо, то есть чтобы можно было наследоваться от TdiTabBase. Без этой строки добавленные "Add(widget)" виджеты не появлялись.
			Stetic.BinContainer.Attach(this);

			Add(widget);
			Child.Show();
		}

		public override string TabName { get => ViewModel.Title; protected set => ViewModel.Title = value; }

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Title))
				OnTabNameChanged();
		}
	}
}
