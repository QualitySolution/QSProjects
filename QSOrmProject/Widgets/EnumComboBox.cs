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
	public class EnumComboBox : ComboBox
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ListStore comboListStore;

		public event EventHandler<EnumItemClickedEventArgs> EnumItemSelected;

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

		private object selectedItem;
		public object SelectedItem {
			get {
				return selectedItem;
			}
			set {
				if (selectedItem == value)
					return;
				selectedItem = value;
				OnEnumItemSelected ();
			}
		}
			
		private bool showSpecialStateAll = false;
		[Browsable(true)]
		public bool ShowSpecialStateAll {
			get {
				return showSpecialStateAll;
			}
			set {
				showSpecialStateAll = value;
				ResetLayout ();
			}
		}

		private bool showSpecialStateNot = false;
		[Browsable(true)]
		public bool ShowSpecialStateNot {
			get {
				return showSpecialStateNot;
			}
			set {
				showSpecialStateNot = value;
				ResetLayout ();
			}
		}

		public EnumComboBox () : base()
		{
			comboListStore = new ListStore (typeof(object), typeof(string));
			CellRendererText text = new CellRendererText ();
			Model = comboListStore;
			this.PackStart (text, false);
			this.AddAttribute (text, "text", 1);
		}

		/// <summary>
		/// Обновляет данные виджета
		/// </summary>
		private void ResetLayout()
		{
			comboListStore.Clear ();

			if (ItemsEnum == null)
				return;

			if (ItemsEnum.IsEnum == false)
				throw new NotSupportedException (string.Format("ItemsEnum only supports enum types, specified was {0}", ItemsEnum));

			//Заполняем саециальные поля
			if(ShowSpecialStateAll)
			{
				AppendEnumItem (typeof(SpecialEnumComboState).GetField ("All"));
			}
			if(ShowSpecialStateNot)
			{
				AppendEnumItem (typeof(SpecialEnumComboState).GetField ("Not"));
			}

			foreach (FieldInfo info in ItemsEnum.GetFields()) {
				AppendEnumItem (info);
			}
		}

		private void AppendEnumItem(FieldInfo info)
		{
			if (info.Name.Equals("value__"))
				return;
			string item = info.GetEnumTitle ();
			//string hint = info.GetEnumHint ();
			//p = (Gdk.Pixbuf) info.GetEnumIcon();
			//if (p != null)
			//	item.Image = new Gtk.Image (p);
			//if (!String.IsNullOrEmpty (hint))
			//	item.TooltipText = hint;
			comboListStore.AppendValues (info.GetValue (null), item);
		}

		void OnEnumItemSelected ()
		{
			if(EnumItemSelected != null)
			{
				EnumItemSelected (this, new EnumItemClickedEventArgs (SelectedItem));
			}
		}

		protected override void OnChanged ()
		{
			base.OnChanged ();
			TreeIter iter;

			if(this.GetActiveIter (out iter))
			{
				SelectedItem = Model.GetValue (iter, 0);
			}
			else
			{
				SelectedItem = SpecialEnumComboState.None;
			}
		}
	}

	public enum SpecialEnumComboState {
		[ItemTitleAttribute("Ничего")]
		None,
		[ItemTitleAttribute("Все")]
		All,
		[ItemTitleAttribute("Нет")]
		Not
	}
}

