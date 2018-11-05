using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;

namespace Gamma.Widgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public class EnumCheckList : VBox
	{
		public BindingControler<EnumCheckList> Binding { get; private set; }

		private Type enumType;

		public Type EnumType {
			get => enumType; set {
				enumType = value;
				FillWidgets();
			}
		}

		public IEnumerable<Enum> SelectedValues {
			get {
				foreach(yCheckButton check in Children) {
					if(check.Active)
						yield return (Enum)check.Tag;
				}
			}
		}

		bool externalSet = false;

		/// <summary>
		/// If you want bind list to typed enum list, use EnumsListConverter<TEnum>.
		/// </summary>
		/// <value>The selected enum values.</value>
		public IList<Enum> SelectedValuesList {
			get { return SelectedValues.ToList(); }
			set {
				externalSet = true;
				foreach(yCheckButton check in Children) {
					var itemValue = (Enum)check.Tag;
					var active = value.Contains(itemValue);
					if(check.Active != active)
						check.Active = active;
				}
				externalSet = false;
			}
		}

		public EnumCheckList()
		{
			Binding = new BindingControler<EnumCheckList>(this, new Expression<Func<EnumCheckList, object>>[] {
				(w => w.SelectedValuesList),
			});
		}

		private void FillWidgets()
		{
			foreach(var wid in Children.ToList()) {
				Remove(wid);
			}

			foreach(Enum item in Enum.GetValues(EnumType)) {
				var title = item.GetEnumTitle();
				var check = new yCheckButton(title);
				check.Label = title;
				check.Tag = item;
				check.Toggled += Check_Toggled;
				Add(check);
			}

			ShowAll();
		}

		void Check_Toggled(object sender, EventArgs e)
		{
			if(!externalSet)
				Binding.FireChange(x => x.SelectedValuesList);
		}

	}
}
