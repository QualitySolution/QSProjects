using System.Collections.Generic;

namespace QS.DbManagement.Entities
{
	public class Response {
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
	}

    public class LoginToServerResponse : Response
    {
		public bool NeedToUpdateLauncher { get; set; }
    }

	public class ChangePasswordResponse : Response { }

	public class CreateDatabaseResponse : Response { }

	public class DropDatabaseResponse : Response { }

	public class AddUserResponse : Response { }

	public class DeleteUserResponse : Response { }

	public class GetUserDatabasesResponse : Response {
		public List<DbInfo> Bases { get; set; }
	}

	/// <summary>счётчики синхронизированных</summary>
	public class RefreshMetadataResponse : Response {
		public int SyncedBases { get; set; }
		public int SyncedUsers { get; set; }
	}

	public class LoginToDatabaseResponse : Response {
		public string ConnectionString { get; set; }
		public string Login { get; set; }

		public Dictionary<string,string> Parameters { get; set; }
	}
}
