using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings;
using System.Reflection;
using Gtk;
using NLog;
using QSWidgetLib;

namespace QSOrmProject
{
	[ToolboxItem (true)]
	[Category ("QS Widgets")]
	public class EnumMenuButton : MenuButton
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		Dictionary<ImageMenuItem, object> MenuItems;
		public event EventHandler<EnumItemClickedEventArgs> EnumItemClicked;

		[Browsable(true)]
		public String ItemsEnumName {
			get {
				return ItemsEnum.AssemblyQualifiedName;
			}
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ItemsEnum = System.Type.GetType(value);
				if (ItemsEnum == null)
					logger.Warn ("Тип {0}, не найден. Свойству ItemsEnumName должна назначатся строка в формате '<namespace>.<enumtype>, <Assembly>'", value);
			}
		}

		System.Type itemsEnum;
		public System.Type ItemsEnum {
			get {
				return itemsEnum;
			}
			set {
				if (itemsEnum == value)
					return;
				itemsEnum = value;

				ResetLayout ();
			}
		}

		public EnumMenuButton () : base()
		{
		}

		/// <summary>
		/// Обновляет данные виджета
		/// </summary>
		private void ResetLayout()
		{
			if (Menu != null) 
			{
				Menu.Destroy ();
				Menu = null;
			}

			MenuItems = new Dictionary<ImageMenuItem, object> ();

			if (ItemsEnum == null)
				return;

			if (ItemsEnum.IsEnum == false)
				throw new NotSupportedException (string.Format("ItemsEnum only supports enum types, specified was {0}", ItemsEnum));

			Menu = new Gtk.Menu ();

			string hint;
			Gdk.Pixbuf p;
			ImageMenuItem item;

			foreach (FieldInfo info in ItemsEnum.GetFields()) {
				if (info.Name.Equals("value__")) continue;
				item = new ImageMenuItem(info.GetEnumTitle ());
				hint = info.GetEnumHint ();
				p = (Gdk.Pixbuf) info.GetEnumIcon();
				if (p != null)
					item.Image = new Gtk.Image (p);
				if (!String.IsNullOrEmpty (hint))
					item.TooltipText = hint;
				item.Activated += OnMenuItemActivated;
				MenuItems.Add (item, info.GetValue (null));
				Menu.Add (item);
			}
			Menu.ShowAll ();
		}

		void OnMenuItemActivated (object sender, EventArgs e)
		{
			if(EnumItemClicked != null)
			{
				object item = MenuItems [(ImageMenuItem)sender];
				EnumItemClicked (this, new EnumItemClickedEventArgs (item));
			}
		}
	}

	public class EnumItemClickedEventArgs : EventArgs
	{
		public object ItemEnum { get; private set; }

		public EnumItemClickedEventArgs(object item)
		{
			ItemEnum = item;
		}
	}

}

