using System;

namespace QSOrmProject
{
	public class EntryReferenceBeforeChangeEventArgs : EventArgs
	{
		public bool CanChange { get; set; }

		public EntryReferenceBeforeChangeEventArgs()
		{
			
		}
	}
}

