using QS.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace QS.ViewModels.Widgets.Pipeline {

	public class PipelineViewModel : ViewModelBase {
		private string _title;
		private PipelineStageViewModel _currentStage;
		private ObservableCollection<PipelineStageViewModel> _stages;
		private bool _isSynchronizingStageSelection;

		public string Title {
			get => _title;
			set => SetField(ref _title, value);
		}

		public PipelineStageViewModel CurrentStage {
			get => _currentStage;
			set {
				if(SetField(ref _currentStage, value))
					SyncActiveStagesWithCurrentStage();
			}
		}

		public ObservableCollection<PipelineStageViewModel> Stages {
			get => _stages;
			set {
				if(ReferenceEquals(_stages, value))
					return;

				UnsubscribeFromStages(_stages);
				SetField(ref _stages, value);
				SubscribeToStages(_stages);
			}
		}

		public PipelineViewModel() {
			_title = string.Empty;
			_stages = new ObservableCollection<PipelineStageViewModel>();
			SubscribeToStages(_stages);
		}

		private void SubscribeToStages(ObservableCollection<PipelineStageViewModel> stages) {
			if(stages == null)
				return;
			foreach(var stage in stages)
				stage.Activated += OnStageActivated;
			stages.CollectionChanged += OnStagesCollectionChanged;
		}

		private void UnsubscribeFromStages(ObservableCollection<PipelineStageViewModel> stages) {
			if(stages == null)
				return;
			foreach(var stage in stages)
				stage.Activated -= OnStageActivated;
			stages.CollectionChanged -= OnStagesCollectionChanged;
		}

		private void OnStagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if(e.OldItems != null)
				foreach(PipelineStageViewModel stage in e.OldItems)
					stage.Activated -= OnStageActivated;
			if(e.NewItems != null)
				foreach(PipelineStageViewModel stage in e.NewItems)
					stage.Activated += OnStageActivated;

			if(_currentStage != null && !_stages.Contains(_currentStage)) {
				CurrentStage = null;
				return;
			}

			SyncActiveStagesWithCurrentStage();
		}

		private void OnStageActivated(object sender, EventArgs e) {
			if(_isSynchronizingStageSelection)
				return;

			CurrentStage = sender as PipelineStageViewModel;
		}

		private void SyncActiveStagesWithCurrentStage() {
			if(_isSynchronizingStageSelection || _stages == null)
				return;

			_isSynchronizingStageSelection = true;
			try {
				foreach(var stage in _stages) {
					bool shouldBeActive = ReferenceEquals(stage, _currentStage);
					if(stage.Active != shouldBeActive)
						stage.Active = shouldBeActive;
				}
			}
			finally {
				_isSynchronizingStageSelection = false;
			}
		}
	}
}
