using System;
using System.ComponentModel;
using System.Data.Bindings;
using System.Reflection;
using Gtk;
using NLog;
using System.Collections.Generic;

namespace QSOrmProject
{
	[ToolboxItem (true)]
	[Category ("QS Widgets")]
	[Obsolete("Используйте аналог yEnumComboBox, все исправления будут в нем, этот виджет не будет поддерживаться и будет удален просле 01.09.2016.")]
	public class EnumComboBox : ComboBox
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		ListStore comboListStore;
		List<object> fieldsToHide = new List<object> ();

		public void AddEnumToHideList (object[] items)
		{
			fieldsToHide.AddRange (items);
			ResetLayout ();
		}

		public void RemoveEnumFromHideList (object[] items)
		{
			foreach (object item in items)
				if (fieldsToHide.Contains (item))
					fieldsToHide.Remove (item);
			ResetLayout ();
		}

		public void ClearEnumHideList ()
		{
			fieldsToHide.Clear ();
			ResetLayout ();
		}

		public event EventHandler<EnumItemClickedEventArgs> EnumItemSelected;

		[Browsable (true)]
		public String ItemsEnumName {
			get {
				return ItemsEnum.AssemblyQualifiedName;
			}
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ItemsEnum = Type.GetType (value);
				if (ItemsEnum == null)
					logger.Warn ("Тип {0}, не найден. Свойству ItemsEnumName должна назначатся строка в формате '<namespace>.<enumtype>, <Assembly>'", value);
			}
		}

		Type itemsEnum;

		public Type ItemsEnum {
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

		object selectedItem;

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

		bool showSpecialStateAll;

		[Browsable (true)]
		public bool ShowSpecialStateAll {
			get {
				return showSpecialStateAll;
			}
			set {
				showSpecialStateAll = value;
				ResetLayout ();
			}
		}

		bool showSpecialStateNot;

		[Browsable (true)]
		public bool ShowSpecialStateNot {
			get {
				return showSpecialStateNot;
			}
			set {
				showSpecialStateNot = value;
				ResetLayout ();
			}
		}

		public EnumComboBox ()
		{
			comboListStore = new ListStore (typeof(object), typeof(string));
			CellRendererText text = new CellRendererText ();
			Model = comboListStore;
			PackStart (text, false);
			AddAttribute (text, "text", 1);
		}

		/// <summary>
		/// Обновляет данные виджета
		/// </summary>
		void ResetLayout ()
		{
			comboListStore.Clear ();

			if (ItemsEnum == null)
				return;

			if (!ItemsEnum.IsEnum)
				throw new NotSupportedException (string.Format ("ItemsEnum only supports enum types, specified was {0}", ItemsEnum));

			//Заполняем специальные поля
			if (ShowSpecialStateAll) {
				AppendEnumItem (typeof(SpecialComboState).GetField ("All"));
			}
			if (ShowSpecialStateNot) {
				AppendEnumItem (typeof(SpecialComboState).GetField ("Not"));
			}

			foreach (FieldInfo info in ItemsEnum.GetFields()) {
				AppendEnumItem (info);
			}

			if (ShowSpecialStateAll || ShowSpecialStateNot)
				Active = 0;
		}

		void AppendEnumItem (FieldInfo info)
		{
			if (info.Name.Equals ("value__"))
				return;
			if (fieldsToHide.Contains (info.GetValue (null)))
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
			if (EnumItemSelected != null) {
				EnumItemSelected (this, new EnumItemClickedEventArgs (SelectedItem));
			}
		}

		protected override void OnChanged ()
		{
			base.OnChanged ();
			TreeIter iter;

			if (GetActiveIter (out iter)) {
				SelectedItem = Model.GetValue (iter, 0);
			} else {
				SelectedItem = SpecialComboState.None;
			}
		}
	}

}

