using Gdk;
using Gtk;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.ComponentModel;

namespace QS.Widgets.GtkUI.Pipeline {

	internal class PipelineStageView : VBox {
		internal int CircleRadius;
		internal int Padding;
		internal int AdditionalInfoHeight;
		internal int NameHeight;
		internal int InnerSpacing;
		internal int TextHorizontalPadding;
		internal double NormalBorderWidth;
		internal double ActiveBorderWidth;
		internal int StageWidth;
		internal int CircleCenterOffsetY;
		internal Color NotStartedColor;
		internal Color InProgressColor;
		internal Color CompletedColor;
		internal Color FailedColor;
		internal Color BorderColor;
		internal Color SymbolColor;
		internal Color AdditionalInfoTextColor;

		private readonly DrawingArea _drawingArea;
		private readonly Label _additionalInfoLabel;
		private readonly Label _nameLabel;
		private PipelineStageViewModel _stageViewModel;
		private int _stageVisualSize;

		public void RefreshLayoutMetrics() {
			if(_stageViewModel == null)
				return;

			UpdateStageWidth();
			UpdateNameLabel();
		}

		// Состояния мыши
		private bool _isHovered = false;

		public PipelineStageView() {
			Spacing = InnerSpacing;

			// Создаем label для дополнительной информации (над кругом)
			_additionalInfoLabel = new Label();
			_additionalInfoLabel.Markup = "";
			_additionalInfoLabel.HeightRequest = AdditionalInfoHeight;
			_additionalInfoLabel.Justify = Justification.Center;
			_additionalInfoLabel.Wrap = false;

			// Создаем DrawingArea для отрисовки круга
			_drawingArea = new DrawingArea();
			_drawingArea.WidthRequest = StageWidth;
			_drawingArea.HeightRequest = StageWidth;
			_drawingArea.AddEvents((int)(EventMask.ButtonPressMask | EventMask.EnterNotifyMask | EventMask.LeaveNotifyMask));

			// Обработчики событий
			_drawingArea.ExposeEvent += OnDrawingAreaExpose;
			_drawingArea.ButtonPressEvent += OnDrawingAreaButtonPress;
			_drawingArea.EnterNotifyEvent += OnDrawingAreaEnter;
			_drawingArea.LeaveNotifyEvent += OnDrawingAreaLeave;

			// Создаем label для названия (под кругом)
			_nameLabel = new Label();
			_nameLabel.Justify = Justification.Center;
			_nameLabel.Wrap = false;
			_nameLabel.HeightRequest = NameHeight;

			var additionalInfoAlignment = new Alignment(0.5f, 0.5f, 0f, 0f);
			additionalInfoAlignment.Add(_additionalInfoLabel);

			var drawingAlignment = new Alignment(0.5f, 0.5f, 0f, 0f);
			drawingAlignment.Add(_drawingArea);

			var nameAlignment = new Alignment(0.5f, 0.5f, 0f, 0f);
			nameAlignment.Add(_nameLabel);

			// Добавляем все в контейнер
			PackStart(additionalInfoAlignment, false, false, 0);
			PackStart(drawingAlignment, false, false, 0);
			PackStart(nameAlignment, false, false, 0);

			ApplySizeSettings();

			ShowAll();
		}

		/// <summary>
		/// ViewModel этапа.
		/// </summary>
		public PipelineStageViewModel StageViewModel {
			get => _stageViewModel;
			set {
				if(ReferenceEquals(_stageViewModel, value))
					return;

				if(_stageViewModel != null)
					_stageViewModel.PropertyChanged -= OnStageViewModelPropertyChanged;

				_stageViewModel = value;

				if(_stageViewModel != null)
					_stageViewModel.PropertyChanged += OnStageViewModelPropertyChanged;

				UpdateNameLabel();
				UpdateAdditionalInfoLabel();
				UpdateStageWidth();

				if(_stageViewModel != null)
					_additionalInfoLabel.Show();
				else
					_additionalInfoLabel.Hide();

				_drawingArea.QueueDraw();
			}
		}

		/// <summary>
		/// </summary>
		private void UpdateStageWidth() {
			int nameWidth = MeasureTextWidth(_stageViewModel != null ? _stageViewModel.Name : string.Empty, false);

			StageWidth = Math.Max(_stageVisualSize, nameWidth + TextHorizontalPadding);
			WidthRequest = StageWidth;
			_additionalInfoLabel.WidthRequest = StageWidth;
			_nameLabel.WidthRequest = StageWidth;
			QueueResize();
		}

		internal void ApplyVisualSettings() {
			CircleRadius = Math.Max(1, CircleRadius);
			Padding = Math.Max(0, Padding);
			AdditionalInfoHeight = Math.Max(0, AdditionalInfoHeight);
			NameHeight = Math.Max(0, NameHeight);
			InnerSpacing = Math.Max(0, InnerSpacing);
			TextHorizontalPadding = Math.Max(0, TextHorizontalPadding);
			NormalBorderWidth = Math.Max(0.1, NormalBorderWidth);
			ActiveBorderWidth = Math.Max(0.1, ActiveBorderWidth);

			ApplySizeSettings();
			UpdateAdditionalInfoLabel();
			_drawingArea?.QueueDraw();
		}

		private void ApplySizeSettings() {
			if(_drawingArea == null || _additionalInfoLabel == null || _nameLabel == null)
				return;

			_stageVisualSize = (CircleRadius + Padding) * 2;
			var stageTotalHeight = AdditionalInfoHeight + InnerSpacing + _stageVisualSize + InnerSpacing + NameHeight;
			CircleCenterOffsetY = AdditionalInfoHeight + InnerSpacing + _stageVisualSize / 2;

			Spacing = InnerSpacing;
			WidthRequest = _stageVisualSize;
			HeightRequest = stageTotalHeight;

			_drawingArea.WidthRequest = _stageVisualSize;
			_drawingArea.HeightRequest = _stageVisualSize;
			_additionalInfoLabel.HeightRequest = AdditionalInfoHeight;
			_nameLabel.HeightRequest = NameHeight;

			if(_stageViewModel != null)
				UpdateStageWidth();
			else
				StageWidth = _stageVisualSize;

			QueueResize();
		}

		private int MeasureTextWidth(string text, bool isSmallText) {
			if(string.IsNullOrEmpty(text))
				return 0;

			var layout = CreatePangoLayout(text);
			if(isSmallText) {
				var fontDescription = Style.FontDescription.Copy();
				fontDescription.Size = (int)(fontDescription.Size * 0.85);
				layout.FontDescription = fontDescription;
			}

			layout.GetPixelSize(out int width, out _);
			return width;
		}

		private void UpdateNameLabel() {
			string name = _stageViewModel != null ? _stageViewModel.Name ?? string.Empty : string.Empty;
			string escapedName = GLib.Markup.EscapeText(name);
			bool isActive = _stageViewModel != null && _stageViewModel.Active;

			if(isActive)
				_nameLabel.Markup = $"<b>{escapedName}</b>";
			else {
				_nameLabel.Text = name;
			}
		}

		private void UpdateAdditionalInfoLabel() {
			if(_additionalInfoLabel == null || _stageViewModel == null)
				return;

			if(!string.IsNullOrEmpty(_stageViewModel.UpperTitle)) {
				string escapedText = GLib.Markup.EscapeText(_stageViewModel.UpperTitle);
				_additionalInfoLabel.Markup = $"<span foreground=\"{ColorToHex(AdditionalInfoTextColor)}\" size=\"small\">{escapedText}</span>";
			}
			else {
				_additionalInfoLabel.Text = string.Empty;
			}
		}

		private static string ColorToHex(Color color) {
			return string.Format("#{0:X2}{1:X2}{2:X2}", color.Red / 257, color.Green / 257, color.Blue / 257);
		}

		private void OnDrawingAreaExpose(object o, ExposeEventArgs args) {
			using(var context = CairoHelper.Create(args.Event.Window)) {
				DrawCircle(context);
			}
		}

		private void DrawCircle(Cairo.Context context) {
			if(_stageViewModel == null)
				return;

			int x = CircleRadius + Padding;
			int y = CircleRadius + Padding;

			// Получаем базовый цвет в зависимости от статуса
			Color baseColor = GetStatusColor(_stageViewModel.Status);
			double r = baseColor.Red / 65535.0;
			double g = baseColor.Green / 65535.0;
			double b = baseColor.Blue / 65535.0;

			// Осветляем цвет если виджет активен или наведен
			bool isActive = _stageViewModel.Active;
			if(isActive || _isHovered) {
				r = Math.Min(1.0, r + 0.1);
				g = Math.Min(1.0, g + 0.1);
				b = Math.Min(1.0, b + 0.1);
			}

			// Рисуем основной круг
			context.Arc(x, y, CircleRadius, 0, 2 * Math.PI);
			context.SetSourceRGB(r, g, b);
			context.Fill();

			// Рисуем обводку
			context.Arc(x, y, CircleRadius, 0, 2 * Math.PI);
			SetColor(context, BorderColor);
			context.LineWidth = isActive ? ActiveBorderWidth : NormalBorderWidth;
			context.Stroke();

			// Добавляем текст со статусом (первая буква)
			if(_stageViewModel.Status == StageStatus.Completed)
				DrawCheckMark(context, x, y);
			else if(_stageViewModel.Status == StageStatus.InProgress) {
				DrawCircularArrow(context, x, y);
			}
			else if(_stageViewModel.Status == StageStatus.Failed) {
				DrawCross(context, x, y);
			}
		}

		private void DrawCheckMark(Cairo.Context context, int x, int y) {
			SetColor(context, SymbolColor);
			double scale = GetSymbolScale();
			context.LineWidth = Math.Max(1.5, 2 * scale);
			context.MoveTo(x - 8 * scale, y);
			context.LineTo(x - 2 * scale, y + 6 * scale);
			context.LineTo(x + 8 * scale, y - 6 * scale);
			context.Stroke();
		}

		private void DrawCross(Cairo.Context context, int x, int y) {
			SetColor(context, SymbolColor);
			double scale = GetSymbolScale();
			context.LineWidth = Math.Max(1.5, 2 * scale);
			context.MoveTo(x - 8 * scale, y - 8 * scale);
			context.LineTo(x + 8 * scale, y + 8 * scale);
			context.Stroke();
			context.MoveTo(x + 8 * scale, y - 8 * scale);
			context.LineTo(x - 8 * scale, y + 8 * scale);
			context.Stroke();
		}

		private void DrawCircularArrow(Cairo.Context context, int x, int y) {
			SetColor(context, SymbolColor);
			double scale = GetSymbolScale();
			context.LineWidth = Math.Max(1.5, 2 * scale);
			context.LineCap = Cairo.LineCap.Round;
			context.LineJoin = Cairo.LineJoin.Round;

			double radius = Math.Max(4.3, CircleRadius * 0.31);
			double startAngle = Math.PI * 0.18;
			double endAngle = Math.PI * 1.72;

			context.Arc(x, y, radius, startAngle, endAngle);
			context.Stroke();

			double tangentAngle = endAngle + Math.PI / 2.0 - 0.144;
			double arcEndX = x + radius * Math.Cos(endAngle);
			double arcEndY = y + radius * Math.Sin(endAngle);
			double shaftLength = Math.Max(2.0, CircleRadius * 0.09);
			double arrowTipX = arcEndX + shaftLength * Math.Cos(tangentAngle);
			double arrowTipY = arcEndY + shaftLength * Math.Sin(tangentAngle);
			double arrowSize = Math.Max(4.2, CircleRadius * 0.25);
			double wingAngle = 0.68;

			context.MoveTo(arcEndX, arcEndY);
			context.LineTo(arrowTipX, arrowTipY);
			context.Stroke();

			context.MoveTo(arrowTipX, arrowTipY);
			context.LineTo(
				arrowTipX - arrowSize * Math.Cos(tangentAngle - wingAngle),
				arrowTipY - arrowSize * Math.Sin(tangentAngle - wingAngle));
			context.MoveTo(arrowTipX, arrowTipY);
			context.LineTo(
				arrowTipX - arrowSize * Math.Cos(tangentAngle + wingAngle),
				arrowTipY - arrowSize * Math.Sin(tangentAngle + wingAngle));
			context.Stroke();
		}

		private double GetSymbolScale() {
			return Math.Max(0.65, CircleRadius / 20.0);
		}

		private static void SetColor(Cairo.Context context, Color color) {
			context.SetSourceRGB(color.Red / 65535.0, color.Green / 65535.0, color.Blue / 65535.0);
		}

		private Color GetStatusColor(StageStatus status) {
			switch(status) {
				case StageStatus.NotStarted:
					return NotStartedColor;
				case StageStatus.InProgress:
					return InProgressColor;
				case StageStatus.Completed:
					return CompletedColor;
				case StageStatus.Failed:
					return FailedColor;
				default:
					return NotStartedColor;
			}
		}

		private void OnDrawingAreaButtonPress(object o, ButtonPressEventArgs args) {
			_stageViewModel.Active = true;
		}

		private void OnStageViewModelPropertyChanged(object sender, PropertyChangedEventArgs args) {
			UpdateNameLabel();
			UpdateAdditionalInfoLabel();
			UpdateStageWidth();
			_drawingArea.QueueDraw();
		}

		private void OnDrawingAreaEnter(object o, EnterNotifyEventArgs args) {
			_isHovered = true;
			_drawingArea.QueueDraw();
		}

		private void OnDrawingAreaLeave(object o, LeaveNotifyEventArgs args) {
			_isHovered = false;
			_drawingArea.QueueDraw();
		}
	}
}
