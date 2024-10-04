using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QS.Dialog;

namespace QS.Project.Avalonia;

public partial class DialogWindow : Window
{
	public ImportanceLevel MessageType
	{
		get => GetValue(MessageTypeProperty);
		set => SetValue(MessageTypeProperty, value);
	}
	public static readonly StyledProperty<ImportanceLevel> MessageTypeProperty =
		AvaloniaProperty.Register<DialogWindow, ImportanceLevel>(nameof(MessageType));

	public string Message
	{
		get => GetValue(MessageProperty);
		set => SetValue(MessageProperty, value);
	}
	public static readonly StyledProperty<string> MessageProperty =
		AvaloniaProperty.Register<DialogWindow, string>(nameof(Message));

    public DialogWindow()
    {
        InitializeComponent();
    }

	public DialogWindow(object content)
	{
		InitializeComponent();
		contentControl.Content = content;
	}

	/// <summary>
	/// Avalonia-based Dialog window.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="title"></param>
	/// <param name="type"></param>
	/// <param name="buttons">You can specify additional buttons with their own Click handlers</param>
	public DialogWindow(string message, string title = "", ImportanceLevel type = ImportanceLevel.Info, params Button[] buttons)
	{
		InitializeComponent();
		Message = message;
		Title = title;
		contentControl.Content = new SelectableTextBlock { Text = message, TextWrapping = TextWrapping.Wrap };

		var b = new Bitmap(AssetLoader.Open(new Uri("avares://QS.Project.Avalonia/Assets/" + type.ToString() + ".png")));

		icon.Source = b;
		Icon = new WindowIcon(b);

		foreach (var button in buttons)
			AddButton(button);
	}

	public void AddButton(Button button)
	{
		buttonContainer.Children.Add(button);
	}

	private void OnCloseClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	#region FastMessages

	public static void Show (ImportanceLevel type, string message, string title = null) => new DialogWindow(message, title, type).Show();
	public static void Error(string message, string title = "Error") => new DialogWindow(message, title, ImportanceLevel.Error).Show();
	public static void Info(string message, string title = "Info") => new DialogWindow(message, title, ImportanceLevel.Info).Show();
	public static void Success(string message, string title = "Success") => new DialogWindow(message, title, ImportanceLevel.Success).Show();
	public static void Warning(string message, string title = "Warning") => new DialogWindow(message, title, ImportanceLevel.Warning).Show();

	#endregion
}
