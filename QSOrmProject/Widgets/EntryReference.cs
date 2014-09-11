using System;
using Gtk;
using NHibernate;
using QSTDI;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EntryReference : Gtk.Bin
	{
		public System.Type SubjectType;

		private object subject;
		public object Subject
		{
			get
			{
				return subject;
			}
			set
			{
				subject = value;
				if(subject == null || displayFields == null)
				{
					entryObject.Text = String.Empty;
					return;
				}

				object[] values = new object[displayFields.Length];
				for(int i = 0; i < displayFields.Length; i++)
				{
					values[i] = SubjectType.GetProperty(displayFields[i]).GetValue(Subject, null);
				}
				entryObject.Text = String.Format(DisplayFormatString, values);
			}
		}

		public string SubjectTypeName
		{
			get
			{
				return SubjectType.Name;
			}
			set
			{
				SubjectType = System.Type.GetType(value);
			}
		}

		private string[] displayFields;
		public string[] DisplayFields
		{
			get
			{
				return displayFields;
			}
			set
			{
				displayFields = value;
			}
		}

		private string displayFormatString;
		public string DisplayFormatString
		{
			get
			{
				return (displayFormatString == null || displayFormatString == String.Empty) 
					? "{0}" : displayFormatString;
			}
			set
			{
				displayFormatString = value;
			}
		}

		public EntryReference()
		{
			this.Build();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;
				
			IOrmDialog dlg = OrmMain.FindMyDialog(this);
			ISession session;
			if (dlg != null)
				session = dlg.Session;
			else
				session = OrmMain.Sessions.OpenSession();

			var criteria = session.CreateCriteria(SubjectType);

			OrmReference SelectDialog = new OrmReference(SubjectType, session, criteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += OnSelectDialogObjectSelected;
			mytab.TabParent.AddSlaveTab(mytab, SelectDialog);
		}

		void OnSelectDialogObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Subject = e.Subject;
		}

		protected void OnButtonOpenClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}

