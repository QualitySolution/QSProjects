using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace QS.Utilities.Debug;

/// <summary>
/// Утилиты диагностики расположения и оформления элементов Avalonia.
/// </summary>
public static class VisualTreeDebug {
	/// <summary>
	/// Возвращает текстовое визуальное дерево окна или контрола, включая элементы его шаблона.
	/// Для каждого элемента выводит тип, имя, Bounds относительно родителя, видимость и отступы;
	/// для панелей, рамок и контролов — также доступные параметры фона, границ и Padding.
	/// Помогает найти источник лишних полос, зазоров и неверных размеров без инспектора Avalonia.
	/// </summary>
	/// <param name="root">Окно или контрол, с которого начинается обход дерева.</param>
	/// <returns>Многострочное дерево с отступами по уровню вложенности, пригодное для записи в лог.</returns>
	/// <remarks>
	/// Вызывать в UI-потоке после применения шаблонов и расчёта расположения, например через
	/// Dispatcher.UIThread.Post с DispatcherPriority.Background после события Window.Opened.
	/// Метод не вызывает перерасчёт расположения: у скрытых или ещё не размещённых элементов
	/// размеры могут быть нулевыми. IsVisible — собственное значение элемента, без учёта родителей.
	/// Текст, Content и DataContext не выводятся. Метод сам ничего не записывает и доступен
	/// в любой конфигурации сборки; место и частоту диагностического вызова выбирает вызывающий код.
	/// </remarks>
	/// <example>
	/// <code>
	/// Dispatcher.UIThread.Post(() => logger.Info(VisualTreeDebug.Dump(window)),
	///     DispatcherPriority.Background);
	/// </code>
	/// </example>
	public static string Dump(Visual root) {
		ArgumentNullException.ThrowIfNull(root);
		Dispatcher.UIThread.VerifyAccess();
		var tree = new StringBuilder();
		AppendVisualTree(root, tree, 0);
		return tree.ToString();
	}

	private static void AppendVisualTree(Visual visual, StringBuilder tree, int depth) {
		tree.Append(' ', depth * 2).Append(visual.GetType().Name)
			.Append(" #").Append((visual as Control)?.Name)
			.Append(" Bounds=").Append(visual.Bounds)
			.Append(" Visible=").Append(visual.IsVisible);
		if(visual is Control control)
			tree.Append(" Margin=").Append(control.Margin);
		switch(visual) {
			case Border border:
				tree.Append(" Border=").Append(border.BorderThickness)
					.Append(" Brush=").Append(border.BorderBrush).Append(" Background=").Append(border.Background)
					.Append(" Padding=").Append(border.Padding);
				break;
			case TemplatedControl templated:
				tree.Append(" Border=").Append(templated.BorderThickness)
					.Append(" Brush=").Append(templated.BorderBrush).Append(" Background=").Append(templated.Background)
					.Append(" Padding=").Append(templated.Padding);
				break;
			case ContentPresenter presenter:
				tree.Append(" Border=").Append(presenter.BorderThickness)
					.Append(" Brush=").Append(presenter.BorderBrush).Append(" Background=").Append(presenter.Background)
					.Append(" Padding=").Append(presenter.Padding);
				break;
			case Panel panel:
				tree.Append(" Background=").Append(panel.Background);
				break;
		}
		tree.AppendLine();
		foreach(var child in visual.GetVisualChildren())
			AppendVisualTree(child, tree, depth + 1);
	}
}
