using System;
using Gdk;

namespace QSScan
{
	public class ImageTransferEventArgs : EventArgs
	{
		public int AllImages { get; set; }
		public Pixbuf Image { get; set; }
	}

	public class ScanWorksPulseEventArgs : EventArgs
	{
		public int CurrentImage { get; set; }
		public string ProgressText { get; set; }
		public int ImageByteSize { get; set; }
		public int LoadedByteSize { get; set; }
	}

}

