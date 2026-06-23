using System;

namespace QS.ViewModels.Widgets.Pipeline {

	public class PipelineStageViewModel : ViewModelBase {
		private string _id;
		private string _name;
		private StageStatus _status = StageStatus.NotStarted;
		private string _upperTitle;
		private bool _active;

		public string Id {
			get => _id;
			set => SetField(ref _id, value);
		}

		public string Name {
			get => _name;
			set => SetField(ref _name, value);
		}

		public StageStatus Status {
			get => _status;
			set => SetField(ref _status, value);
		}

		public string UpperTitle {
			get => _upperTitle;
			set => SetField(ref _upperTitle, value);
		}

		public virtual bool Active {
			get => _active;
			set {
				if(SetField(ref _active, value) && _active)
					Activated?.Invoke(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// Срабатывает при нажатии на этап пользователем.
		/// </summary>
		public event EventHandler Activated;
	}
}
