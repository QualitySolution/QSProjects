using System;
using Gtk;

namespace QS.Test.GtkUI
{
	public static class GtkInit
	{
		private static bool initialized;

		public static void AtOnceInitGtk()
		{
			if (initialized)
				return;

			Application.Init();
			initialized = true;
		}
	}
}
