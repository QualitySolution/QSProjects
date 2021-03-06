﻿using System.Runtime.Serialization;

namespace QSUpdater //FIXME Исправить на QS.Updater.DTO при переделке интерфейса
{
	[DataContract]
	public class UpdateResult
	{
		[DataMember]
		public bool HasUpdate;
		[DataMember]
		public string NewVersion;
		[DataMember]
		public string FileLink;
		[DataMember]
		public string InfoLink;
		[DataMember]
		public string UpdateDescription;

		public UpdateResult (bool hasUpdate, string newVersion = "", string fileLink = "", string infoLink = "", string updateDescription = "")
		{
			HasUpdate = hasUpdate;
			NewVersion = newVersion;
			FileLink = fileLink;
			InfoLink = infoLink;
			UpdateDescription = updateDescription;
		}
	}
}

