using System;
using System.Collections.Generic;
using QSSupportLib;
using QSProjectsLib;
using System.IO;

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
			Version currentDB;
			if(MainSupport.BaseParameters.All.ContainsKey("micro_updates"))
				currentDB = Version.Parse(MainSupport.BaseParameters.All["micro_updates"]);
			else 
				currentDB = Version.Parse(MainSupport.BaseParameters.Version);

			logger.Info("Проверяем микро обновления базы(текущая версия:{0})", StringWorks.VersionToShortString(currentDB));
		
			while(microUpdates.Exists(u => u.Source == currentDB))
			{
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

			MainSupport.BaseParameters.UpdateParameter(
				QSMain.ConnectionDB, 
				"micro_updates",
				StringWorks.VersionToShortString(currentDB)
			);
		}
	}

	class UpdateHop
	{
		public Version Source;
		public Version Destanation;

		public String Resource;
	}
}

