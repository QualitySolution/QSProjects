using System.Runtime.Serialization;
using System;

namespace QSSaaS
{
	public enum ErrorType
	{
		None,
		SqlError,
		UserExists,
		AccessDenied,
		SessionsLimitReached}
	;

	[DataContract]
	public class Response
	{
		[DataMember]
		public bool Success;
		[DataMember]
		public string Description;
		[DataMember]
		public ErrorType Error;

		public Response ()
		{
			Success = true;
			Description = String.Empty;
			Error = ErrorType.None;
		}

		public Response (ErrorType errorType, string description)
		{
			Success = false;
			Error = errorType;
			Description = description;
		}
	}

	[DataContract]
	public class Base : Response
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string Name;
		[DataMember]
		public string Product;
		[DataMember]
		public double Size;

		public Base ()
		{
		}

		public Base (int id, string name, string product, double size)
		{
			Id = id;
			Name = name;
			Product = product;
			Size = size;
		}
	}

	[DataContract]
	public class User : Response
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string Login;
		[DataMember]
		public string Active;
		[DataMember]
		public string Name;

		public User (ErrorType errorType, string description) : base (errorType, description)
		{
		}

		public User (int id, string login, string active, string name)
		{
			Id = id;
			Login = login;
			Active = active;
			Name = name;
		}
	}

	[DataContract]
	public class AccountInfo : Response
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

		public AccountInfo (ErrorType errorType, string description) : base (errorType, description)
		{
		}

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
	public class UserBaseAccess : Response
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string BaseName;
		[DataMember]
		public string ProductName;
		[DataMember]
		public int HasAccess;
		[DataMember]
		public int IsAdmin;

		public UserBaseAccess (ErrorType errorType, string description) : base (errorType, description)
		{
		}

		public UserBaseAccess (int id, string baseName, string productName, int hasAccess = 0, int isAdmin = 0)
		{
			Id = id;
			BaseName = baseName;
			ProductName = productName;
			HasAccess = hasAccess;
			IsAdmin = isAdmin;
		}
	}

	[DataContract]
	public class BaseUserAccess : Response
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string UserName;
		[DataMember]
		public string UserLogin;
		[DataMember]
		public int HasAccess;
		[DataMember]
		public int IsAdmin;

		public BaseUserAccess (ErrorType errorType, string description) : base (errorType, description)
		{
		}

		public BaseUserAccess (int id, string userName, string userLogin, int hasAccess = 0, int isAdmin = 0)
		{
			Id = id;
			UserName = userName;
			UserLogin = userLogin;
			HasAccess = hasAccess;
			IsAdmin = isAdmin;
		}
	}

	[DataContract]
	public class AccountAuthResult : Response
	{
		[DataMember]
		public string SessionID;

		public AccountAuthResult (ErrorType errorType, string description) : base (errorType, description)
		{
		}

		public AccountAuthResult (string sessionID)
		{
			SessionID = sessionID;
		}
	}

	[DataContract]
	public class UserAuthResult : Response
	{
		[DataMember]
		public string SessionID;
		[DataMember]
		public string Server;
		[DataMember]
		public bool IsAdmin;
		[DataMember]
		public string Login;

		public UserAuthResult (ErrorType errorType, string description) : base (errorType, description)
		{
		}

		public UserAuthResult (string sessionID, string server, string login, bool isAdmin = false)
		{
			SessionID = sessionID;
			Login = login;
			Server = server;
			IsAdmin = isAdmin;
		}
	}

	[DataContract]
	public class UserAuthorizeResult : Response
	{
		[DataMember]
		public string Server;
		[DataMember]
		public bool IsAdmin;
		[DataMember]
		public string Login;
		[DataMember]
		public string BaseName;
		[DataMember]
		public string SessionID;

		public UserAuthorizeResult (ErrorType errorType, string description) : base (errorType, description)
		{
		}

		public UserAuthorizeResult (string sessionID, string server, string login, string baseName, bool isAdmin = false)
		{
			SessionID = sessionID;
			Login = login;
			Server = server;
			IsAdmin = isAdmin;
			BaseName = baseName;
		}
	}
}

