using System;
using Gtk;
using QS.Tdi.Gtk;

namespace QS.Tdi
{
	public static class TDIMain
	{
		public static TdiNotebook MainNotebook;

        /// <summary>
        /// Включает цветные префиксы для группированных вкладок
        /// </summary>
        /// <param name="colors">Список чередуемых цветов</param>
        /// <param name="keepColors">Сохранение цветов за каждой вкладкой, но возможны повторения цветов</param>
        /// <param name="prefix">Используемый символ-префикс</param>
        public static void SetTabsColorHighlighting(bool enable, bool keepColors, string[] colors = null, char prefix = '\u25CF')
        {
            if (MainNotebook == null)
                throw new NullReferenceException("Не присвоен MainNotebook для настройки цветных префиксов.");

            MainNotebook.Colors = enable ? colors ?? new[] { "aqua", "orange" } : null;
            MainNotebook.Markup = enable ? "<span color='{0}'>" + prefix + "</span> {1}" : null;
            MainNotebook.UseTabColors = enable;
            MainNotebook.KeepColors = keepColors;
        }

        /// <summary>
        /// Устанавливает возможность перемещать вкладки
        /// </summary>
        public static void SetTabsReordering(bool enable = true)
        {
            if (MainNotebook == null)
                throw new NullReferenceException("Не присвоен MainNotebook для настройки перемещения вкладок.");

            MainNotebook.AllowToReorderTabs = enable;
        }

        public static void TDIHandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
		{
			if(MainNotebook == null)
				throw new InvalidOperationException("Вызвано событие TDIHandleKeyReleaseEvent, но для его корректной работы необходимо заполнить TDIMain.MainNotebook.");

			int platform = (int)Environment.OSVersion.Platform;
			int version = (int)Environment.OSVersion.Version.Major;
			Gdk.ModifierType modifier;

			//Kind of MacOSX
			if ((platform == 4 || platform == 6 || platform == 128) && version > 8)
				modifier = Gdk.ModifierType.MetaMask | Gdk.ModifierType.Mod1Mask;
			//Kind of Windows or Unix
			else
				modifier = Gdk.ModifierType.ControlMask;

			//CTRL+S || CTRL+ENTER
			if ((args.Event.Key == Gdk.Key.S
				|| args.Event.Key == Gdk.Key.s
				|| args.Event.Key == Gdk.Key.Cyrillic_yeru
				|| args.Event.Key == Gdk.Key.Cyrillic_YERU
				|| args.Event.Key == Gdk.Key.Return) && args.Event.State.HasFlag(modifier)) {
				var w = MainNotebook.CurrentPageWidget;
				if (w is TabVBox) {
					var tab = (w as TabVBox).Tab;
					if (tab is TdiSliderTab slider) {
						if(slider.ActiveDialog is ITdiDialog dialog)
							dialog.SaveAndClose ();
					}
					if(tab is ITdiDialog)
					{
						(tab as ITdiDialog).SaveAndClose();
					}
				}
			}

		}

	}
}

