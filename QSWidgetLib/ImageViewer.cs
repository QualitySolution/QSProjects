using System;
using Gdk;
using Gtk;
using NLog;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public class ImageViewer : Gtk.DrawingArea
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ImageViewer () : base ()
		{
			_pixbuf = null;
			ResizedPixbuf = null;
			_HorizontalFit = true;
			_VerticalFit = true;
			AddEvents ((int) EventMask.ButtonPressMask);

			SetSizeRequest (1, 1);
		}

		private Pixbuf ResizedPixbuf;
			
		Pixbuf _pixbuf;
		[GLib.Property ("pixbuf", "Get/Set pixbuf", "This is the description")]
		public Pixbuf pixbuf
		{
			get {
				return _pixbuf;
			}
			set {
				_pixbuf = value;
			}
		}

		bool _VerticalFit ;
		[GLib.Property ("VerticalFit", "Get/Set VerticalFit", "This is the description")]
		public bool VerticalFit
		{
			get {
				return _VerticalFit;
			}
			set {
				_VerticalFit = value;
			}
		}

		bool _HorizontalFit ;
		[GLib.Property ("Horizontal", "Get/Set Horizontal", "This is the description")]
		public bool HorizontalFit
		{
			get {
				return _HorizontalFit;
			}
			set {
				_HorizontalFit = value;
			}
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose e)
		{
			if (ResizedPixbuf == null)
				return false;
			logger.Debug("Explose w={0}, h={1}", this.Allocation.Width, this.Allocation.Height) ;
			int dest_x = this.Allocation.Width > ResizedPixbuf.Width ? (Allocation.Width - ResizedPixbuf.Width) / 2 : 0;
			int dest_y = this.Allocation.Height > ResizedPixbuf.Height ? (Allocation.Height - ResizedPixbuf.Height) / 2 : 0;
			e.Window.DrawPixbuf(this.Style.BackgroundGC(State), ResizedPixbuf, 0, 0, dest_x, dest_y, -1, -1, RgbDither.None, 0, 0);

			return true;
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			logger.Debug("Allocate w={0}, h={1}", allocation.Width, allocation.Height) ;

			if (_pixbuf == null)
				return;
			double vratio = _VerticalFit ? (double)allocation.Height / _pixbuf.Height : 1;
			double hratio = _HorizontalFit ? (double)allocation.Width / _pixbuf.Width : 1;
			int Heigth, Width;
			if(vratio < hratio)
			{
				Heigth = allocation.Height;
				Width = Convert.ToInt32(_pixbuf.Width * vratio);
			}
			else 
			{
				Heigth = Convert.ToInt32(_pixbuf.Height * hratio);
				Width = allocation.Width;
			}

			if(ResizedPixbuf == null || ResizedPixbuf.Width != Width || ResizedPixbuf.Height != Heigth)
				ResizedPixbuf = _pixbuf.ScaleSimple (Width,
					Heigth,
					InterpType.Bilinear);
			int ReqHeigth = _VerticalFit ? 1 : Heigth;
			int ReqWidth = _HorizontalFit ? 1 : Width;

			if (ReqWidth != WidthRequest || ReqHeigth != HeightRequest)
				SetSizeRequest(ReqWidth, ReqHeigth);
		}

		protected override void OnSizeRequested (ref Requisition requisition)
		{
			logger.Debug("Request w={0}, h={1}", requisition.Width, requisition.Height) ;
		}
	} 
}