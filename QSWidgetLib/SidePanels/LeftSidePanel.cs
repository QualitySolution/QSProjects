using System;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LeftSidePanel : Gtk.Bin
	{
		Gtk.Widget panel;

		public Gtk.Widget Panel
		{
			get
			{
				return panel;
			}
			set
			{ 
				if (panel == value)
					return;
				if(panel != null)
				{
					hboxMain.Remove (panel);
				}
				panel = value;

				if(panel != null)
				{
					hboxMain. PackStart (panel);
					hboxMain.ReorderChild (panel, 0);
					panel.Visible = !IsHided;
					panel.NoShowAll = true;
				}
			}
		}

		public string Title {
			get {
				return labelTitle.LabelProp;
			}
			set{ labelTitle.LabelProp = value;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public bool ClosedByUser{ get; private set;}
		[System.ComponentModel.Browsable(false)]
		public bool OpenedByUser{ get; private set;}

		public bool IsHided
		{
			get
			{
				return arrowSlider.ArrowType == Gtk.ArrowType.Right;
			}
			set
			{
				if(value)
				{
					arrowSlider.ArrowType = Gtk.ArrowType.Right;
				}
				else
				{
					arrowSlider.ArrowType = Gtk.ArrowType.Left;
				}
				if(Panel != null)
					panel.Visible = !value;
			}
		}

		public LeftSidePanel ()
		{
			this.Build ();
		}

		protected void OnEventboxArrowButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			var newHiddenState = !IsHided;
			IsHided = newHiddenState;
			ClosedByUser = newHiddenState;
			OpenedByUser = !newHiddenState;
		}
	}
}

