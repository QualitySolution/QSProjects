using System;
using System.ComponentModel;
using System.Data.Bindings;
using System.Reflection;
using Gtk;
using NLog;
using System.Collections;
using QSOrmProject;
using QSProjectsLib;
using Gamma.Widgets;

namespace QSOrmProject.Gamma
{
	[ToolboxItem (true)]
	[Category ("QS Widgets")]
	public class ySpecComboBox : yListComboBox
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public override object SelectedItem {
			get {
				return  base.SelectedItem;
			}
			set {
				if (base.SelectedItem == value)
					return;
				
				TreeIter iter;
				if (value == null)
				{
					if (ShowSpecialStateAll)
						base.SelectedItem = SpecialComboState.All;
					else if(ShowSpecialStateNot)
						base.SelectedItem = SpecialComboState.Not;
					else
						base.SelectedItem = null;
				}
				if (value is IDomainObject) {
					if (ListStoreWorks.SearchListStore<IDomainObject> ((ListStore)Model, item => item.Id == ((IDomainObject)value).Id, 1, out iter))
					{
						base.SelectedItem = ((ListStore)Model).GetValue (iter, (int)comboDataColumns.Item);
					}
				} else {
					base.SelectedItem = value;
				}
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

		IEnumerable itemsList;

		public override IEnumerable ItemsList {
			get{
				if (ShowSpecialStateAll)
					yield return SpecialComboState.All;
				if (ShowSpecialStateNot)
					yield return SpecialComboState.Not;
				if (itemsList == null)
					yield break;
				foreach (var item in itemsList)
					yield return item;
			}
			set {
				if(itemsList == value)
					return;
				itemsList = value;
				ResetLayout ();
			}
		}

		public new Func<object, string> RenderTextFunc;

		public ySpecComboBox () : base()
		{
			base.RenderTextFunc = RenderText;
		}

		protected override void ResetLayout()
		{
			base.ResetLayout ();
			if (ShowSpecialStateAll || ShowSpecialStateNot)
				Active = 0;
		}

		private string RenderText(object item)
		{
			if (item is SpecialComboState)
				return ((SpecialComboState)item).GetEnumTitle ();
			if (RenderTextFunc != null)
				return RenderTextFunc (item);
			return DomainHelper.GetObjectTilte (item);
		}
	}
}

