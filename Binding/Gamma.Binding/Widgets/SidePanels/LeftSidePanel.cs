using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;

namespace Gamma.Widgets.SidePanels
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LeftSidePanel : Gtk.Bin
	{
		public event EventHandler PanelOpened;
		public event EventHandler PanelHided;

		public BindingControler<LeftSidePanel> Binding { get; private set; }

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
					hboxMain.PackStart (panel);
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

				if (value) {
					PanelHided?.Invoke(this, EventArgs.Empty);
				}
				else {
					PanelOpened?.Invoke(this, EventArgs.Empty);
				}
				Binding.FireChange(x => x.IsHided);
			}
		}

		public LeftSidePanel()
		{
			Build ();

			Binding = new BindingControler<LeftSidePanel>(this, new Expression<Func<LeftSidePanel, object>>[] {
				(w => w.IsHided),
			});
		}

		protected void OnEventboxArrowButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			var newHiddenState = !IsHided;
			IsHided = newHiddenState;
			ClosedByUser = newHiddenState;
			OpenedByUser = !newHiddenState;
		}

		protected override void OnDestroyed() {
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			if(Binding != null) {
				Binding.CleanSources();
				Binding = null;
			}
			PanelOpened = null;
			PanelHided = null;
			base.OnDestroyed();
		}
	}
}

