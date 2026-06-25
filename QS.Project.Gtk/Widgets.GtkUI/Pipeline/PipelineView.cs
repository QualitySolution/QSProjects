using Gdk;
using Gtk;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace QS.Widgets.GtkUI.Pipeline {

	/// <summary>
	/// Виджет пайплайна - линия этапов с соединительными линиями
	/// </summary>
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public class PipelineView : HBox
    {
        private int _lineHeight = 3;
        private int _pipelineSidePadding = 20;
        private int _pipelineVerticalPadding = 20;
        private int _minStageGap = 12;
        private int _lineSideInset = 4;
        private int _connectorDrawHeight = 10;
        private int _titleHeight = 24;
        private int _titleBottomSpacing = 8;
        private int _stageCircleRadius = 20;
        private int _stagePadding = 5;
        private int _stageAdditionalInfoHeight = 20;
        private int _stageNameHeight = 34;
        private int _stageInnerSpacing = 5;
        private int _stageTextHorizontalPadding = 16;
        private double _stageNormalBorderWidth = 2.0;
        private double _stageActiveBorderWidth = 4.0;
        private Gdk.Color _connectorColor = new Gdk.Color(179, 179, 179);
        private Gdk.Color _stageNotStartedColor = new Gdk.Color(150, 150, 150);
        private Gdk.Color _stageInProgressColor = new Gdk.Color(100, 150, 200);
        private Gdk.Color _stageCompletedColor = new Gdk.Color(100, 200, 100);
        private Gdk.Color _stageFailedColor = new Gdk.Color(200, 100, 100);
        private Gdk.Color _stageBorderColor = new Gdk.Color(0, 0, 0);
        private Gdk.Color _stageSymbolColor = new Gdk.Color(255, 255, 255);
        private Gdk.Color _stageAdditionalInfoTextColor = new Gdk.Color(255, 0, 0);

		private Fixed _fixedContainer;
        private List<PipelineStageView> _stages = new List<PipelineStageView>();
        private List<DrawingArea> _connectors = new List<DrawingArea>();
        private Label _titleLabel;
        private PipelineViewModel _viewModel;
        private ObservableCollection<PipelineStageViewModel> _boundStages = new ObservableCollection<PipelineStageViewModel>();
        private float _horizontalAlignment = 0.5f;
        private float _verticalAlignment = 0.5f;
        private int _lastLayoutWidth = -1;
        private int _lastLayoutHeight = -1;

        public PipelineViewModel ViewModel
        {
            get => _viewModel; 
            set => SetViewModel(value);
        }

        #region Дизайн

        public float HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                _horizontalAlignment = Clamp01(value);
                Relayout();
            }
        }

        public float VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _verticalAlignment = Clamp01(value);
                Relayout();
            }
        }

        public int LineHeight
        {
            get => _lineHeight;
            set
            {
                _lineHeight = Math.Max(1, value);
                RedrawConnectors();
            }
        }

        public int PipelineSidePadding
        {
            get => _pipelineSidePadding;
            set
            {
                _pipelineSidePadding = Math.Max(0, value);
                Relayout();
            }
        }

        public int PipelineVerticalPadding
        {
            get => _pipelineVerticalPadding;
            set
            {
                _pipelineVerticalPadding = Math.Max(0, value);
                Relayout();
            }
        }

        public int MinStageGap
        {
            get => _minStageGap;
            set
            {
                _minStageGap = Math.Max(0, value);
                Relayout();
            }
        }

        public int LineSideInset
        {
            get => _lineSideInset;
            set
            {
                _lineSideInset = Math.Max(0, value);
                Relayout();
            }
        }

        public int ConnectorDrawHeight
        {
            get => _connectorDrawHeight;
            set
            {
                _connectorDrawHeight = Math.Max(1, value);
                RefreshConnectorHeights();
                Relayout();
            }
        }

        public int TitleHeight
        {
            get => _titleHeight;
            set
            {
                _titleHeight = Math.Max(0, value);
                _titleLabel.HeightRequest = _titleHeight;
                Relayout();
            }
        }

        public int TitleBottomSpacing
        {
            get => _titleBottomSpacing;
            set
            {
                _titleBottomSpacing = Math.Max(0, value);
                Relayout();
            }
        }

        #region Дизайн стадии

        public int StageCircleRadius
        {
            get => _stageCircleRadius;
            set
            {
                _stageCircleRadius = Math.Max(1, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public int StagePadding
        {
            get => _stagePadding;
            set
            {
                _stagePadding = Math.Max(0, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public int StageAdditionalInfoHeight
        {
            get => _stageAdditionalInfoHeight;
            set
            {
                _stageAdditionalInfoHeight = Math.Max(0, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public int StageNameHeight
        {
            get => _stageNameHeight;
            set
            {
                _stageNameHeight = Math.Max(0, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public int StageInnerSpacing
        {
            get => _stageInnerSpacing;
            set
            {
                _stageInnerSpacing = Math.Max(0, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public int StageTextHorizontalPadding
        {
            get => _stageTextHorizontalPadding;
            set
            {
                _stageTextHorizontalPadding = Math.Max(0, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public double StageNormalBorderWidth
        {
            get => _stageNormalBorderWidth;
            set
            {
                _stageNormalBorderWidth = Math.Max(0.1, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public double StageActiveBorderWidth
        {
            get => _stageActiveBorderWidth;
            set
            {
                _stageActiveBorderWidth = Math.Max(0.1, value);
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color ConnectorColor
        {
            get => _connectorColor;
            set
            {
                _connectorColor = value;
                RedrawConnectors();
            }
        }

        public Gdk.Color StageNotStartedColor
        {
            get => _stageNotStartedColor;
            set
            {
                _stageNotStartedColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color StageInProgressColor
        {
            get => _stageInProgressColor;
            set
            {
                _stageInProgressColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color StageCompletedColor
        {
            get => _stageCompletedColor;
            set
            {
                _stageCompletedColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color StageFailedColor
        {
            get => _stageFailedColor;
            set
            {
                _stageFailedColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color StageBorderColor
        {
            get => _stageBorderColor;
            set
            {
                _stageBorderColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color StageSymbolColor
        {
            get => _stageSymbolColor;
            set
            {
                _stageSymbolColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        public Gdk.Color StageAdditionalInfoTextColor
        {
            get => _stageAdditionalInfoTextColor;
            set
            {
                _stageAdditionalInfoTextColor = value;
                ApplyStageVisualSettingsToAllStages();
            }
        }

        #endregion

        #endregion

        public PipelineView()
        {
			_fixedContainer = new Fixed();
			_titleLabel = new Label();
            _titleLabel.Justify = Justification.Center;
            _titleLabel.Wrap = false;
            _titleLabel.HeightRequest = _titleHeight;
			_fixedContainer.Put(_titleLabel, 0, 0);

			PackStart(_fixedContainer, true, true, 0);

            SizeAllocated += OnPipelineSizeAllocated;
            Realized += OnPipelineRealized;

            SetViewModel(new PipelineViewModel());
            UpdateTitleLabel();

            ShowAll();
        }

        private void SetViewModel(PipelineViewModel viewModel)
        {
            if (ReferenceEquals(_viewModel, viewModel))
                return;

            UnbindViewModel();
            _viewModel = viewModel ?? new PipelineViewModel();
            BindViewModel();
            RebuildStagesFromViewModel();
        }

        private void BindViewModel()
        {
            if (_viewModel == null)
                return;

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            BindStagesCollection(_viewModel.Stages);
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            UnbindStagesCollection();
        }

        private void BindStagesCollection(ObservableCollection<PipelineStageViewModel> stages)
        {
            if (ReferenceEquals(_boundStages, stages))
                return;

            if (_boundStages != null)
                _boundStages.CollectionChanged -= OnStagesCollectionChanged;

            _boundStages = stages ?? new ObservableCollection<PipelineStageViewModel>();
            _boundStages.CollectionChanged += OnStagesCollectionChanged;
        }

        private void UnbindStagesCollection()
        {
            if (_boundStages != null)
                _boundStages.CollectionChanged -= OnStagesCollectionChanged;
            _boundStages = new ObservableCollection<PipelineStageViewModel>();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(PipelineViewModel.Title))
            {
                UpdateTitleLabel();
                Relayout();
                return;
            }

            if (args.PropertyName == nameof(PipelineViewModel.Stages))
            {
                BindStagesCollection(_viewModel.Stages);
                RebuildStagesFromViewModel();
            }
        }

        private void OnStagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RebuildStagesFromViewModel();
        }

        private void RebuildStagesFromViewModel()
        {
            foreach (var stage in _stages)
            {
                Remove(stage);
            }

            foreach (var connector in _connectors)
            {
                Remove(connector);
            }

            _stages.Clear();
            _connectors.Clear();

            var stageViewModels = _viewModel != null && _viewModel.Stages != null
                ? new List<PipelineStageViewModel>(_viewModel.Stages)
                : new List<PipelineStageViewModel>();

            for (int i = 0; i < stageViewModels.Count; i++)
            {
                var stage = new PipelineStageView();
                ApplyStageVisualSettings(stage);
                stage.StageViewModel = stageViewModels[i];

                _stages.Add(stage);
				_fixedContainer.Put(stage, 0, 0);
                stage.ShowAll();

                if (i < stageViewModels.Count - 1)
                {
                    var connector = new DrawingArea();
                    connector.WidthRequest = 1;
                    connector.HeightRequest = _connectorDrawHeight;
                    connector.ExposeEvent += OnConnectorExpose;
                    _connectors.Add(connector);
					_fixedContainer.Put(connector, 0, 0);
                    connector.Show();
                }
            }

            RefreshStageMetrics();
            Relayout();
        }

        private void OnPipelineSizeAllocated(object o, SizeAllocatedArgs args)
        {
            if (args.Allocation.Width == _lastLayoutWidth && args.Allocation.Height == _lastLayoutHeight)
                return;

            Relayout(args.Allocation.Width, args.Allocation.Height);
        }

        private void OnPipelineRealized(object sender, EventArgs args)
        {
            RefreshStageMetrics();
            Relayout();
        }

        private void RefreshStageMetrics()
        {
            foreach (var stage in _stages)
            {
                stage.RefreshLayoutMetrics();
            }
        }

        private void ApplyStageVisualSettings(PipelineStageView stage)
        {
            if (stage == null)
                return;

            stage.CircleRadius = _stageCircleRadius;
            stage.Padding = _stagePadding;
            stage.AdditionalInfoHeight = _stageAdditionalInfoHeight;
            stage.NameHeight = _stageNameHeight;
            stage.InnerSpacing = _stageInnerSpacing;
            stage.TextHorizontalPadding = _stageTextHorizontalPadding;
            stage.NormalBorderWidth = _stageNormalBorderWidth;
            stage.ActiveBorderWidth = _stageActiveBorderWidth;
            stage.NotStartedColor = _stageNotStartedColor;
            stage.InProgressColor = _stageInProgressColor;
            stage.CompletedColor = _stageCompletedColor;
            stage.FailedColor = _stageFailedColor;
            stage.BorderColor = _stageBorderColor;
            stage.SymbolColor = _stageSymbolColor;
            stage.AdditionalInfoTextColor = _stageAdditionalInfoTextColor;
            stage.ApplyVisualSettings();
        }

        private void ApplyStageVisualSettingsToAllStages()
        {
            foreach (var stage in _stages)
            {
                ApplyStageVisualSettings(stage);
            }

            RefreshStageMetrics();
            Relayout();
        }

        private void Relayout()
        {
            var allocation = Allocation;
            if (allocation.Width <= 0 || allocation.Height <= 0)
                return;

            Relayout(allocation.Width, allocation.Height);
        }

        private void Relayout(int availableWidth, int availableHeight)
        {
            if (availableWidth <= 0 || availableHeight <= 0)
                return;

            _lastLayoutWidth = availableWidth;
            _lastLayoutHeight = availableHeight;

            if (_stages.Count == 0)
                return;

            int pipelineWidth = GetPipelineWidth();
            int startX = GetPipelineStartX(availableWidth, pipelineWidth);
            int titleOffset = HasTitle() ? _titleHeight + _titleBottomSpacing : 0;

            int contentHeight = GetMaxStageHeight() + titleOffset;
            int y = GetPipelineStartY(availableHeight, contentHeight);
            int stagesY = y + titleOffset;

            if (HasTitle())
            {
                _titleLabel.WidthRequest = pipelineWidth;
				_fixedContainer.Move(_titleLabel, startX, y);
                _titleLabel.Show();
            }
            else
            {
                _titleLabel.Hide();
            }

            int stageX = startX;
            for (int i = 0; i < _stages.Count; i++)
            {
				_fixedContainer.Move(_stages[i], stageX, stagesY);

                if (i < _connectors.Count)
                {
                    int currentStageWidth = _stages[i].StageWidth;
                    int nextStageX = stageX + currentStageWidth + _minStageGap;
                    int nextStageWidth = _stages[i + 1].StageWidth;
                    int currentCenterX = stageX + currentStageWidth / 2;
                    int nextCenterX = nextStageX + nextStageWidth / 2;
                    int connectorX = currentCenterX + _stages[i].CircleRadius + _lineSideInset;
                    int connectorWidth = Math.Max(1, nextCenterX - _stages[i + 1].CircleRadius - _lineSideInset - connectorX);

                    if (_connectors[i].WidthRequest != connectorWidth)
                        _connectors[i].WidthRequest = connectorWidth;

					_fixedContainer.Move(_connectors[i], connectorX, stagesY + _stages[i].CircleCenterOffsetY - _connectorDrawHeight / 2);
                    _connectors[i].QueueDraw();
                }

                stageX += _stages[i].StageWidth + _minStageGap;
            }
        }

        private int GetPipelineWidth()
        {
            int width = 0;
            for (int i = 0; i < _stages.Count; i++)
            {
                width += _stages[i].StageWidth;
                if (i < _stages.Count - 1)
                    width += _minStageGap;
            }

            return width;
        }

        private int GetPipelineStartX(int availableWidth, int pipelineWidth)
        {
            if (pipelineWidth >= availableWidth)
                return 0;

            int leftX = _pipelineSidePadding;
            int rightX = Math.Max(0, availableWidth - pipelineWidth - _pipelineSidePadding);
            if (rightX < leftX)
                return Math.Max(0, leftX);

            return (int)Math.Round(leftX + (rightX - leftX) * _horizontalAlignment);
        }

        private int GetPipelineStartY(int availableHeight, int contentHeight)
        {
            if (contentHeight >= availableHeight)
                return 0;

            int topY = _pipelineVerticalPadding;
            int bottomY = Math.Max(0, availableHeight - contentHeight - _pipelineVerticalPadding);
            if (bottomY < topY)
                return Math.Max(0, topY);

            return (int)Math.Round(topY + (bottomY - topY) * _verticalAlignment);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;
            if (value > 1f)
                return 1f;

            return value;
        }

        private int GetMaxStageHeight()
        {
            int maxHeight = 0;
            foreach (var stage in _stages)
            {
                var req = stage.SizeRequest();
                if (req.Height > maxHeight)
                    maxHeight = req.Height;
            }

            return maxHeight;
        }

        private bool HasTitle()
        {
            return _viewModel != null && !string.IsNullOrEmpty(_viewModel.Title);
        }

        private void UpdateTitleLabel()
        {
            if (HasTitle())
                _titleLabel.Markup = $"<b>{GLib.Markup.EscapeText(_viewModel.Title)}</b>";
            else
            {
                _titleLabel.Text = string.Empty;
            }
        }

        private void OnConnectorExpose(object o, ExposeEventArgs args)
        {
            var connector = o as DrawingArea;
            if (connector == null)
                return;

            using (var context = CairoHelper.Create(args.Event.Window))
            {
                int width = connector.Allocation.Width;
                int centerY = connector.Allocation.Height / 2;

                context.SetSourceRGB(_connectorColor.Red / 65535.0, _connectorColor.Green / 65535.0, _connectorColor.Blue / 65535.0);
                context.LineWidth = _lineHeight;
                context.MoveTo(0, centerY);
                context.LineTo(width, centerY);
                context.Stroke();
            }
        }

        private void RefreshConnectorHeights()
        {
            foreach (var connector in _connectors)
            {
                connector.HeightRequest = _connectorDrawHeight;
            }
        }

        private void RedrawConnectors()
        {
            foreach (var connector in _connectors)
            {
                connector.QueueDraw();
            }
        }

    }
}
