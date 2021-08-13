using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.Widgets.SidePanels
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class HideHorizontalSeparator : Gtk.Bin
	{
		public BindingControler<HideHorizontalSeparator> Binding { get; private set; }

		public event ToggledHandler Toggled;

		public string Label {get{
				return labelTitle.LabelProp;
			}
			set{
				labelTitle.LabelProp = value;
			}
		}

		public ArrowType ArrowDirection{
			get{
				return arrowRight.ArrowType;
			}
			set{
				if(arrowRight.ArrowType != value) {
					arrowLeft.ArrowType = arrowRight.ArrowType = value;
					Binding.FireChange(x => x.ArrowDirection);
				}
			}
		}

		public bool ManualArrowControl = false;

		public HideHorizontalSeparator()
		{
			this.Build();

			Binding = new BindingControler<HideHorizontalSeparator>(this, new Expression<Func<HideHorizontalSeparator, object>>[] {
				(w => w.ArrowDirection),
			});
		}

		protected void OnEventAllWidgetButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(!ManualArrowControl) {
				switch(ArrowDirection) {
					case ArrowType.Up:
						ArrowDirection = ArrowType.Down;
						break;
					case ArrowType.Down:
						ArrowDirection = ArrowType.Up;
						break;
					default:
						ArrowDirection = ArrowType.Down;
						break;
				}
			}

			Toggled?.Invoke(this, new ToggledArgs());
		}
	}
}
