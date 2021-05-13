using System;
using Gtk;

namespace QSWidgetLib.SidePanels
{
	[System.ComponentModel.ToolboxItem(true)]
	[Obsolete("Замените на аналогичный из библоитеки Gamma. Планируется полностью удалить библиотеку QSWidgetLib. Остался только ВВ.")]
	public partial class HideHorizontalSeparator : Gtk.Bin
	{
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
				arrowLeft.ArrowType = arrowRight.ArrowType = value;
			}
		}

		public bool ManualArrowControl = false;

		public HideHorizontalSeparator()
		{
			this.Build();
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
