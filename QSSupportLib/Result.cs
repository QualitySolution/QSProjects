using System;
using System.Runtime.Serialization;

namespace QSSupportLib
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



	[DataContract]
	public class Result
	{
		[DataMember]
		public bool Success;
		[DataMember]
		public string Description;

		public Result (bool success, string description = "")
		{
			Success = success;
			Description = description;
		}
	}

	[DataContract]
	public class AccountAuthResult : Result
	{
		[DataMember]
		public string SessionID;

		public AccountAuthResult (bool success, string description = "") : base (success, description)
		{
		}

		public AccountAuthResult (bool success, string sessionID, string description) : base (success, description)
		{
			SessionID = sessionID;
		}
	}

	[DataContract]
	public class UserAuthResult : AccountAuthResult
	{
		[DataMember]
		public string Server;
		[DataMember]
		public bool IsAdmin;
		[DataMember]
		public string Login;

		public UserAuthResult (bool success, string description = "") : base (success, description)
		{
		}

		public UserAuthResult (bool success, string sessionID, string server, string login, bool isAdmin = false) : base (success, sessionID, "")
		{
			Login = login;
			Server = server;
			IsAdmin = isAdmin;
		}
	}
}

