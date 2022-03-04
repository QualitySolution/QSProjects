using System;
using QS.Dialog;

namespace QS.ViewModels.Extension
{
	/// <summary>
	/// При добавлении этого интерфейса на ViewModel, Мендеджеры навигации умеющие работать с отдельными окнами
	/// должны открывать диалог в отдельном окне. Так же интерфейс позволяет указать некоторые параметры открытия
	/// окна.
	/// </summary>
	public interface IWindowDialogSettings
	{
		/// <summary>
		/// Окно должно быть модальным.
		/// </summary>
		bool IsModal { get; }

		/// <summary>
		/// Включает кнопки свернуть\развернуть у окна.
		/// </summary>
		bool EnableMinimizeMaximize { get; }

		/// <summary>
		/// Возможность изменять размеры окна
		/// </summary>
		bool Resizable { get; }
		
		/// <summary>
		/// Доступность кнопки закрыть
		/// </summary>
		bool Deletable { get; }

		/// <summary>
		/// Положение окна при открытии.
		/// </summary>
		WindowGravity WindowPosition { get; }
	}
}
