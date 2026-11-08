using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;
using QS.Dialog;

namespace QS.ErrorReporting;

public partial class ErrorReportWindow : Window {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
	private readonly ErrorReportViewModel viewModel;

	public ErrorReportWindow(ErrorReportViewModel viewModel) {
		this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		InitializeComponent();
		EmailInput.Text = viewModel.Email;
		ExceptionInput.Text = viewModel.ExceptionText;
		UpdateInputs();
	}

	public void AddException(Exception exception) {
		viewModel.AddException(exception);
		ExceptionInput.Text = viewModel.ExceptionText;
	}

	private void OnInputsChanged(object? sender, TextChangedEventArgs e) {
		if(EmailInput == null || DescriptionInput == null || SendButton == null) return;
		viewModel.Email = EmailInput.Text ?? string.Empty;
		viewModel.Description = DescriptionInput.Text ?? string.Empty;
		UpdateInputs();
	}

	private void UpdateInputs() {
		SendButton.IsEnabled = viewModel.CanSend;
		ValidationMessage.Text = viewModel.ValidationMessage;
		ValidationMessage.IsVisible = !viewModel.CanSend;
	}

	private void OnSendClicked(object? sender, RoutedEventArgs e) {
		try {
			if(viewModel.Submit(ReportType.User)) {
				Close();
				return;
			}
		} catch(Exception ex) { logger.Error(ex, "Не удалось отправить отчёт об ошибке"); }
		new AvaloniaInteractiveMessage().ShowMessage(ImportanceLevel.Warning,
			"Отправка сообщения не удалась. Проверьте подключение к интернету и повторите попытку.", "Ошибка отправки");
	}

	private async void OnCopyClicked(object? sender, RoutedEventArgs e) {
		try {
			if(Clipboard != null) await Clipboard.SetTextAsync(viewModel.CopyText);
		} catch(Exception ex) { logger.Error(ex, "Не удалось скопировать ошибку"); }
	}

	private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();

	protected override void OnClosed(EventArgs e) {
		try {
			if(viewModel.SendAutomatically && !viewModel.ReportSent && !viewModel.Submit(ReportType.Automatic))
				logger.Warn("Автоматический отчёт об ошибке отправить не удалось");
		} catch(Exception ex) { logger.Error(ex, "Не удалось автоматически отправить отчёт об ошибке"); }
		base.OnClosed(e);
	}
}
