using System;

namespace QSChat
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChatVSlider : Gtk.Bin
	{
		public Chat Chat
		{
			get
			{
				return chat1;
			}
		}
			
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
				chat1.IsHided = value;
				labelMessageCount.Visible = value;
			}
		}

		public ChatVSlider()
		{
			this.Build();
		}

		protected void OnChat1ChatUpdated(object sender, EventArgs e)
		{
			labelMessageCount.Visible = chat1.IsHided;
			labelMessageCount.Markup = chat1.NewMessageCount > 0 ?
				String.Format("<span background=\"red\" size=\"x-large\">{0}</span>", chat1.NewMessageCount) : "0";
		}

		protected void OnEventboxArrowButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			IsHided = !IsHided;
		}
	}
}

