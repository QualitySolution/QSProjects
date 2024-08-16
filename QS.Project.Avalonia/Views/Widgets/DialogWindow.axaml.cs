using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace QS.Project.Avalonia;

public enum DialogType
{
	Info,
	Warning,
	Error,
	Success
}

public partial class DialogWindow : Window
{
	public DialogType MessageType
	{
		get => GetValue(MessageTypeProperty);
		set => SetValue(MessageTypeProperty, value);
	}
	public static readonly StyledProperty<DialogType> MessageTypeProperty =
		AvaloniaProperty.Register<DialogWindow, DialogType>(nameof(DialogType));

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
	public DialogWindow(string message, string title = "", DialogType type = DialogType.Info, params Button[] buttons)
	{
		InitializeComponent();
		Message = message;
		Title = title;
		contentControl.Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };

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
}
