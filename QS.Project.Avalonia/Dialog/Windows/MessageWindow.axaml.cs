using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace QS.Dialog.Windows;

public partial class MessageWindow : Window
{
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
	/// <param name="icon">Иконка окна и заголовка</param>
	/// <param name="buttons">You can specify additional buttons with their own Click handlers</param>
	public MessageWindow(string message, string title = "", MessageIcon icon = MessageIcon.Info, params Button[] buttons)
	{
		InitializeComponent();
		Message = message;
		Title = title;
		contentControl.Content = new SelectableTextBlock { Text = message, TextWrapping = TextWrapping.Wrap };

		var b = new Bitmap(AssetLoader.Open(new Uri("avares://QS.Project.Avalonia/Assets/" + icon.ToString() + ".png")));

		this.icon.Source = b;
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
