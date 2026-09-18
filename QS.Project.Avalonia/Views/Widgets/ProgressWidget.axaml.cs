using System;
using Avalonia.Controls;
using Avalonia.Threading;
using QS.Dialog;

namespace QS.Widgets;

/// <summary>
/// Методы можно вызывать из любого потока — изменения переносятся на поток GUI
/// </summary>
public partial class ProgressWidget : UserControl, IProgressBarDisplayable {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	// ProgressBar не выпускает значение за границы, и перебор остался бы незаметным
	private double madeSteps;

	public ProgressWidget() {
		InitializeComponent();
	}

	public double Value => progressBar.Value;

	public bool IsStarted { get; private set; }

	public void Start(double maxValue = 1, double minValue = 0, string? text = null, double startValue = 0) =>
		OnGuiThread(() => {
			progressBar.Minimum = minValue;
			progressBar.Maximum = maxValue;
			progressBar.Value = startValue;
			madeSteps = startValue;
			SetText(text);
			IsStarted = true;
		});

	public void Update(double curValue) =>
		OnGuiThread(() => {
			progressBar.Value = curValue;
			madeSteps = curValue;
		});

	public void Update(string? curText) => OnGuiThread(() => SetText(curText));

	public void UpdateMax(double maxValue) => OnGuiThread(() => progressBar.Maximum = maxValue);

	public void Add(double addValue = 1, string? text = null) =>
		OnGuiThread(() => {
			progressBar.Value += addValue;
			madeSteps += addValue;
			if(madeSteps > progressBar.Maximum)
				logger.Warn("Значение прогресса {0} больше максимального {1}", madeSteps, progressBar.Maximum);
			if(text != null)
				SetText(text);
		});

	public void Close() =>
		OnGuiThread(() => {
			if(IsStarted && Math.Abs(madeSteps - progressBar.Maximum) > 0.5)
				logger.Warn("Прогресс остановлен на шаге {0} из {1}", madeSteps, progressBar.Maximum);
			SetText(null);
			IsStarted = false;
		});

	private void SetText(string? text) {
		progressText.Text = text;
		progressText.IsVisible = !string.IsNullOrEmpty(text);
	}

	private static void OnGuiThread(Action change) {
		if(!Dispatcher.UIThread.CheckAccess()) {
			Dispatcher.UIThread.Post(change);
			return;
		}

		change();
		Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
	}
}
