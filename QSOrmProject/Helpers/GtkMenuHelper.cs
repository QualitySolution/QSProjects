using System;
using Gtk;
using System.Reflection;
using QSWidgetLib;
using Gamma.Utilities;

namespace QSOrmProject
{
	public static class GtkMenuHelper
	{
		public static Menu GenerateMenuFromEnum<TEnum>(EventHandler activateHandler)
		{
			var enumType = typeof(TEnum);

			if (enumType == null)
				return null;

			if (enumType.IsEnum == false)
				throw new NotSupportedException (string.Format("enumType only supports enum types, specified was {0}", enumType));

			var menu = new Menu();

			foreach (FieldInfo info in enumType.GetFields()) {
				if (info.Name.Equals("value__")) continue;
				var item = new MenuItemId<TEnum>(info.GetEnumTitle ());
				var hint = info.GetFieldDescription ();

				if (!String.IsNullOrWhiteSpace (hint))
					item.TooltipText = hint;
				item.Activated += activateHandler;
				item.ID = (TEnum)info.GetValue(null);
				menu.Add (item);
			}
			menu.ShowAll ();
			return menu;
		}
	}
}

