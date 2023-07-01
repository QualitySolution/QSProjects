using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Gamma.Binding.Core;
using Gamma.Utilities;
using Gtk;
using NLog;

namespace QS.Widgets
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public class EnumMenuButton : MenuButton
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		Dictionary<ImageMenuItem, object> MenuItems;
		public event EventHandler<EnumItemClickedEventArgs> EnumItemClicked;
		List<object> sensitiveFalseItems = new List<object>();
		List<object> invisibleItems = new List<object>();

		System.Type itemsEnum;
		public System.Type ItemsEnum {
			get {
				return itemsEnum;
			}
			set {
				if(itemsEnum == value)
					return;
				itemsEnum = value;

				ResetLayout();
			}
		}

		public new BindingControler<EnumMenuButton> Binding { get; private set; }

		public EnumMenuButton() : base()
		{
			Binding = new BindingControler<EnumMenuButton>(this);
		}

		/// <summary>
		/// Обновляет данные виджета
		/// </summary>
		private void ResetLayout()
		{
			if(Menu != null) {
				Menu.Destroy();
				Menu = null;
			}

			MenuItems = new Dictionary<ImageMenuItem, object>();

			if(ItemsEnum == null)
				return;

			if(ItemsEnum.IsEnum == false)
				throw new NotSupportedException(string.Format("ItemsEnum only supports enum types, specified was {0}", ItemsEnum));

			Menu = new Gtk.Menu();

			string hint;
			//Gdk.Pixbuf p;
			ImageMenuItem item;

			foreach(FieldInfo info in ItemsEnum.GetFields()) {
				if(info.Name.Equals("value__")) continue;
				item = new ImageMenuItem(info.GetEnumTitle());
				hint = info.GetFieldDescription();
				//p = (Gdk.Pixbuf) info.GetEnumIcon();
				//if (p != null)
				//	item.Image = new Gtk.Image (p);
				if(!String.IsNullOrWhiteSpace(hint))
					item.TooltipText = hint;
				item.Activated += OnMenuItemActivated;
				if(sensitiveFalseItems.Contains(info.GetValue(null)))
					item.Sensitive = false;
				if(invisibleItems.Contains(info.GetValue(null)))
					item.Visible = false;
				MenuItems.Add(item, info.GetValue(null));
				Menu.Add(item);
			}
			Menu.ShowAll();
		}

		void OnMenuItemActivated(object sender, EventArgs e)
		{
			if(EnumItemClicked != null) {
				object item = MenuItems[(ImageMenuItem)sender];
				EnumItemClicked(this, new EnumItemClickedEventArgs(item));
			}
		}

		public void SetSensitive(object enumItem, bool sensitive)
		{
			var menuitem = MenuItems.First(pair => enumItem.Equals(pair.Value)).Key;
			menuitem.Sensitive = sensitive;
			if(sensitive)
				if(!sensitiveFalseItems.Contains(enumItem))
					sensitiveFalseItems.Add(enumItem);
				else
				if(sensitiveFalseItems.Contains(enumItem))
					sensitiveFalseItems.Remove(enumItem);
		}

		public void SetVisibility(object enumItem, bool visible)
		{
			var menuitem = MenuItems.First(pair => enumItem.Equals(pair.Value)).Key;
			menuitem.Visible = visible;
			if(visible) {
				if(!invisibleItems.Contains(enumItem))
					invisibleItems.Add(enumItem);
				else
					if(invisibleItems.Contains(enumItem))
					invisibleItems.Remove(enumItem);
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
