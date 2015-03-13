using System.Runtime.Serialization;
using System;

namespace QSSaaS
{
	public enum ErrorType
	{
		None,
		SqlError,
		UserExists,
		AccessDenied}
	;

	[DataContract]
	public class Base
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string Name;
		[DataMember]
		public string Product;
		[DataMember]
		public double Size;

		public Base (int id, string name, string product, double size)
		{
			Id = id;
			Name = name;
			Product = product;
			Size = size;
		}
	}

	[DataContract]
	public class User
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string Login;
		[DataMember]
		public string Active;

		public User (int id, string login, string active)
		{
			Id = id;
			Login = login;
			Active = active;
		}
	}

	[DataContract]
	public class AccountInfo
	{
		[DataMember]
		public bool IsActive;
		[DataMember]
		public int ActiveSessions;
		[DataMember]
		public int BasesCount;
		[DataMember]
		public double SpaceUsed;
		[DataMember]
		public int MaxSessions;
		[DataMember]
		public int MaxBases;
		[DataMember]
		public int MaxSpace;
		[DataMember]
		public decimal SubscriberFee;
		[DataMember]
		public DateTime EndDate;

		public AccountInfo (bool isActive, int activeSessions, int basesCount, double spaceUsed, 
		                    int maxSessions, int maxBases, int maxSpace, decimal subscriberFee, DateTime endDate)
		{
			IsActive = isActive;
			ActiveSessions = activeSessions;
			BasesCount = basesCount;
			SpaceUsed = spaceUsed;
			MaxSessions = maxSessions;
			MaxBases = maxBases;
			MaxSpace = maxSpace;
			SubscriberFee = subscriberFee;
			EndDate = endDate;
		}
	}

	[DataContract]
	public class Result
	{
		[DataMember]
		public bool Success;
		[DataMember]
		public string Description;
		[DataMember]
		public ErrorType Error = ErrorType.None;

		public Result (bool success, string description = "")
		{
			Success = success;
			Description = description;
		}

		public Result (bool success, ErrorType error, string description = "") : this (success, description)
		{
			Error = error;
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

	[DataContract]
	public class UserAuthorizeResult : AccountAuthResult
	{
		[DataMember]
		public string Server;
		[DataMember]
		public bool IsAdmin;
		[DataMember]
		public string Login;
		[DataMember]
		public string BaseName;

		public UserAuthorizeResult (bool success, string description = "") : base (success, description)
		{
		}

		public UserAuthorizeResult (bool success, string sessionID, string server, string login, string baseName, bool isAdmin = false) : base (success, sessionID, "")
		{
			Login = login;
			Server = server;
			IsAdmin = isAdmin;
			BaseName = baseName;
		}
	}
}

