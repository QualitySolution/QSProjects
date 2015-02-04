using System.Runtime.Serialization;

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
}

