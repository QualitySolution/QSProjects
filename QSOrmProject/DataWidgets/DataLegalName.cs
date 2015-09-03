using System;
using System.ComponentModel;
using Gamma.Binding.Core;
using QSWidgetLib;

namespace QSOrmProject
{
	[ToolboxItem (true)]
	public class DataLegalName : LegalName
	{
		public BindingControler<DataLegalName> Binding { get; private set;}

		public DataLegalName ()
		{
			Binding = new BindingControler<DataLegalName> (this, new string[]{
				this.GetPropertyName (w => w.OwnName),
				this.GetPropertyName (w => w.Ownership),
				this.GetPropertyName (w => w.FullName)
			});
			NameChanged += DataLegalName_NameChanged;
			OwnershipChanged += DataLegalName_OwnershipChanged;
			FullNameChanged += DataLegalName_FullNameChanged;
		}

		void DataLegalName_FullNameChanged (object sender, EventArgs e)
		{
			Binding.FireChange (w => w.FullName);
		}

		void DataLegalName_OwnershipChanged (object sender, EventArgs e)
		{
			Binding.FireChange (w => w.Ownership);
		}

		void DataLegalName_NameChanged (object sender, EventArgs e)
		{
			Binding.FireChange (w => w.OwnName);
		}
	}
}

