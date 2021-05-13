using System;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	[Obsolete("Замените на аналогичный из библоитеки Gamma. Планируется полностью удалить библиотеку QSWidgetLib. Остался только ВВ.")]
	public partial class RightSidePanel : Gtk.Bin
	{
		public event EventHandler PanelOpened;
		public event EventHandler PanelHided;

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
					hboxMain. PackEnd (panel);
					hboxMain.ReorderChild (panel, 1);
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
				return arrowSlider.ArrowType == Gtk.ArrowType.Left;
			}
			set
			{
				if(value)
				{
					arrowSlider.ArrowType = Gtk.ArrowType.Left;
				}
				else
				{
					arrowSlider.ArrowType = Gtk.ArrowType.Right;
				}
				if (Panel != null)
				{					
					panel.Visible = !value;
				}

				if (value)
				{
					if (PanelHided != null)
						PanelHided(this, EventArgs.Empty);
				}
				else
				{
					if (PanelOpened != null)
						PanelOpened(this, EventArgs.Empty);
				}

				ClosedByUser = false;
				OpenedByUser = false;
			}
		}			

		public RightSidePanel ()
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