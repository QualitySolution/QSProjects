using System.ComponentModel;
using Gamma.Binding.Core;
using QSOrmProject;

namespace QS.yWidgets
{
	[ToolboxItem(true)]
	[Category("QS Widgets")]
	public class yEnumMenuButton : EnumMenuButton
	{
		public BindingControler<yEnumMenuButton> Binding { get; private set; }

		public yEnumMenuButton()
		{
			Binding = new BindingControler<yEnumMenuButton>(this);
		}
	}
}