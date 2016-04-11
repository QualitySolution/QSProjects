using System;
using QSSupportLib;
using Gtk;

namespace QSUpdater
{
	public static class MainUpdater
	{
		public static void RunCheckVersion (bool updateDB, bool updateApp, bool installMicroUpdate)
		{
			CheckBaseVersion.Check ();

			if(CheckBaseVersion.ResultFlags == CheckBaseResult.BaseVersionLess && updateDB)
			{
				DB.DBUpdater.TryUpdate ();
				RunCheckVersion (updateDB, updateApp, installMicroUpdate);
				return;
			}

			if(CheckBaseVersion.ResultFlags == CheckBaseResult.BaseVersionGreater && updateApp)
			{
				CheckUpdate.StartCheckUpdateThread (UpdaterFlags.UpdateRequired);
			}

			if(CheckBaseVersion.ResultFlags != CheckBaseResult.Ok)
			{
				MessageDialog VersionError = new MessageDialog (QSProjectsLib.QSMain.ErrorDlgParrent, 
					DialogFlags.DestroyWithParent,
					MessageType.Warning, 
					ButtonsType.Close, 
					CheckBaseVersion.TextMessage);
				VersionError.Run ();
				VersionError.Destroy ();
				Environment.Exit (0);
			}

			if (installMicroUpdate)
				DB.DBUpdater.CheckMicroUpdates ();

			if(updateApp)
				CheckUpdate.StartCheckUpdateThread (UpdaterFlags.StartInThread);
		}
	}
}

