using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gamma.Widgets.Additions;
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
				fieldsToHide.Clear();
				FillWidgets();
			}
		}

		private readonly List<Enum> fieldsToHide = new List<Enum>();

		public void AddEnumToHideList(IEnumerable<Enum> items)
		{
			fieldsToHide.AddRange(items);
			UpdateVisibility();
		}
		
		public void RemoveEnumFromHideList(IEnumerable<Enum> items)
		{
			foreach(var item in items) {
				if(fieldsToHide.Contains(item)) {
					fieldsToHide.Remove(item);
				}
			}
			UpdateVisibility();
		}
		
		public void ClearEnumHideList()
		{
			fieldsToHide.Clear();
			UpdateVisibility();
		}

		private void UpdateVisibility()
		{
			foreach(var check in Children.Cast<yCheckButton>()) {
				if(fieldsToHide.Contains(check.Tag)) {
					if(!check.Visible) {
						continue;
					}
					check.Visible = false;
					if(!RememberStateOnHide) {
						check.Active = false;
					}
				}
				else if(!check.Visible) {
					check.Visible = true;
				}
			}
		}
		
		public void SelectAll()
		{
			foreach(var check in Children.Cast<yCheckButton>()) {
				if(!check.Active) {
					check.Active = true;
				}
			}
		}
		
		public void UnselectAll()
		{
			foreach(var check in Children.Cast<yCheckButton>()) {
				if(check.Active) {
					check.Active = false;
				}
			}
		}
		
		/// <summary>
		/// Default false
		/// If true, checkboxes will not change their state when enums are added and removed from hide list
		/// </summary>
		public bool RememberStateOnHide { get; set; } = false;

		public IEnumerable<Enum> SelectedValues {
			get {
				foreach(var check in Children.Cast<yCheckButton>()) {
					if(check.Active && check.Visible)
						yield return (Enum)check.Tag;
				}
			}
		}

		bool externalSet = false;

		/// <summary>
		/// If you want bind list to typed enum list, use <see cref="EnumsListConverter{TEnum}"/>
		/// </summary>
		/// <value>The selected enum values.</value>
		public IList<Enum> SelectedValuesList {
			get { return SelectedValues.ToList(); }
			set {
				externalSet = true;
				foreach(var check in Children.Cast<yCheckButton>()) {
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
