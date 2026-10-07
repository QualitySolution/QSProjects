using System;
using Avalonia.Controls;
using Avalonia.Threading;
using QS.Dialog;

namespace QS.Controls;

/// <summary>
/// Методы можно вызывать из любого потока — изменения переносятся на поток GUI
/// </summary>
public partial class ProgressPanel : UserControl, IProgressBarDisplayable {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	// ProgressBar не выпускает значение за границы, и перебор остался бы незаметным
	private double madeSteps;

	public ProgressPanel() {
		InitializeComponent();
	}

	public double Value => progressBar.Value;

	public bool IsStarted { get; private set; }

	public void Start(double maxValue = 1, double minValue = 0, string? text = null, double startValue = 0) =>
		OnGuiThread(() => {
			progressBar.IsIndeterminate = false;
			progressBar.Minimum = minValue;
			progressBar.Maximum = maxValue;
			progressBar.Value = startValue;
			madeSteps = startValue;
			SetText(text);
			IsStarted = true;
		}, first: true);

	/// <summary>
	/// Прогресс без известного количества шагов. Пока кто-то не вызовет <see cref="Start"/>, показывает только текст.
	/// </summary>
	public void StartIndeterminate(string? text = null) =>
		OnGuiThread(() => {
			progressBar.IsIndeterminate = true;
			SetText(text);
		}, first: true);

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

	// Не ждем отрисовку чаще чем раз в это время, чтобы не тормозить быстрые операции с большим количеством шагов.
	private const int redrawIntervalMilliseconds = 50;

	private static void OnGuiThread(Action change, bool first = false) {
		if(!Dispatcher.UIThread.CheckAccess()) {
			Dispatcher.UIThread.Post(change);
			return;
		}

		change();
		AvaloniaGuiDispatcher.WaitForRedraw(first ? 0 : redrawIntervalMilliseconds);
	}
}
