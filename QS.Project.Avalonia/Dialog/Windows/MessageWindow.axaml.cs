using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QS.Dialog;

namespace QS.Dialog.Windows;

public partial class MessageWindow : Window
{
	public ImportanceLevel MessageType
	{
		get => GetValue(MessageTypeProperty);
		set => SetValue(MessageTypeProperty, value);
	}
	public static readonly StyledProperty<ImportanceLevel> MessageTypeProperty =
		AvaloniaProperty.Register<MessageWindow, ImportanceLevel>(nameof(MessageType));

	public string Message
	{
		get => GetValue(MessageProperty);
		set => SetValue(MessageProperty, value);
	}
	public static readonly StyledProperty<string> MessageProperty =
		AvaloniaProperty.Register<MessageWindow, string>(nameof(Message));

    public MessageWindow()
    {
        InitializeComponent();
    }

	public MessageWindow(object content)
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
	public MessageWindow(string message, string title = "", ImportanceLevel type = ImportanceLevel.Info, params Button[] buttons)
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

	public void HideCloseButton()
	{
		closeButton.IsVisible = false;
	}

	private void OnCloseClick(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
