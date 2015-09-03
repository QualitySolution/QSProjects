using System;
using System.Collections.Generic;
using QSSupportLib;
using QSProjectsLib;
using System.IO;
using Gtk;

namespace QSUpdater.DB
{
	public static class DBUpdater
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		static readonly List<UpdateHop> microUpdates = new List<UpdateHop>();

		public static void AddMicroUpdate( Version source, Version destination, string scriptResource)
		{
			microUpdates.Add(new UpdateHop
				{
					Source = source,
					Destanation = destination,
					Resource = scriptResource
				});
		}

		public static void CheckMicroUpdates()
		{
			Version currentDB, beforeUpdates;
			if(MainSupport.BaseParameters.All.ContainsKey("micro_updates"))
				currentDB = beforeUpdates = Version.Parse(MainSupport.BaseParameters.All["micro_updates"]);
			else 
				currentDB = beforeUpdates = Version.Parse(MainSupport.BaseParameters.Version);

			logger.Info("Проверяем микро обновления базы(текущая версия:{0})", StringWorks.VersionToShortString(currentDB));
		
			while(microUpdates.Exists(u => u.Source == currentDB))
			{
                if (!QSMain.User.Admin)
                    NotAdminErrorAndExit();
                var update = microUpdates.Find(u => u.Source == currentDB);
				logger.Info("Обновляемся до {0}", StringWorks.VersionToShortString(update.Destanation));
				var trans = QSMain.ConnectionDB.BeginTransaction();
				try
				{
					string sql;
					using(Stream stream = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream(update.Resource))
					{
						if(stream == null)
							throw new InvalidOperationException( String.Format("Ресурс {0} указанный в обновлениях не найден.", update.Resource));
						StreamReader reader = new StreamReader(stream);
						sql = reader.ReadToEnd();
					}

					var cmd = QSMain.ConnectionDB.CreateCommand();
					cmd.CommandText = sql;
					cmd.Transaction = trans;
					cmd.ExecuteNonQuery();
					trans.Commit();
					currentDB = update.Destanation;
				}
				catch(Exception ex)
				{
					trans.Rollback();
					QSMain.ErrorMessageWithLog("Ошибка при обновлении базы..", logger, ex);
					return;
				}
			}

			if(currentDB != beforeUpdates)
				MainSupport.BaseParameters.UpdateParameter(
					QSMain.ConnectionDB, 
					"micro_updates",
					StringWorks.VersionToShortString(currentDB)
				);
		}

        private static void NotAdminErrorAndExit()
        {
            MessageDialog md = new MessageDialog (null, DialogFlags.DestroyWithParent,
                MessageType.Error, 
                ButtonsType.Close,
                "Для работы текущей версии программы необходимо провести микро обновление базы, " +
                "но у вас нет для этого прав. Зайдите в программу под администратором.");
            md.Show ();
            md.Run ();
            md.Destroy ();
            Environment.Exit(1);
        }
	}

	class UpdateHop
	{
		public Version Source;
		public Version Destanation;

		public String Resource;
	}
}

